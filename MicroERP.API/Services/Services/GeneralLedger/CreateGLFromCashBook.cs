using Dapper;
using Microsoft.Data.SqlClient;
using MicroERP.API.Helper;
using MicroERP.API.Services;
using MicroERP.Shared.Models;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MicroERP.API.Services.GeneralLedger;

public static class CreateGLFromCashBookHelper
{
    public static async Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromCashBook(
        IGeneralLedgerService service,
        int cashBookId,
        int userId,
        string clientInfo)
    {
        try
        {
            using var connection = new SqlConnection(new Config().DefaultConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get cashbook data
                string SQLCashBook = $@"SELECT * FROM Cashbook WHERE Id = {cashBookId} AND IsSoftDeleted = 0";
                var cashBook = (await connection.QueryAsync<CashBookModel>(SQLCashBook, transaction: transaction))?.FirstOrDefault();
                
                if (cashBook == null || cashBook.Id == 0)
                {
                    transaction.Rollback();
                    return (false, null!, "Cashbook entry not found.");
                }

                // Check if already posted
                if (cashBook.IsPostedToGL == 1)
                {
                    transaction.Rollback();
                    return (false, null!, "Cashbook entry is already posted to General Ledger.");
                }

                // Validate period for CASHBOOK module
                var periodCheck = await service.ValidatePeriod(cashBook.OrganizationId, cashBook.TranDate ?? DateTime.Now, "CASHBOOK");
                if (!periodCheck.Item1)
                {
                    transaction.Rollback();
                    return (false, null!, periodCheck.Item2);
                }

                // Generate EntryNo within transaction
                string prefix = "GL";
                int codeLength = 6;
                string format = new string('0', codeLength);
                string sqlGetCode = $@"SELECT MAX(CAST(SUBSTRING(EntryNo, {prefix.Length + 1}, LEN(EntryNo) - {prefix.Length}) AS INT)) AS SEQNO
                     FROM GeneralLedgerHeader WHERE LEFT(EntryNo, {prefix.Length}) = '{prefix}'";
                var codeResult = await connection.QueryFirstOrDefaultAsync<int?>(sqlGetCode, transaction: transaction);
                int nextCode = (codeResult ?? 0) + 1;
                string entryNo = prefix + nextCode.ToString(format);
                
                GeneralLedgerHeaderModel glHeader = new GeneralLedgerHeaderModel
                {
                    OrganizationId = cashBook.OrganizationId,
                    EntryNo = entryNo,
                    EntryDate = cashBook.TranDate ?? DateTime.Now,
                    Source = "CASHBOOK",
                    Description = $"Cashbook {cashBook.SeqNo}",
                    ReferenceNo = cashBook.SeqNo,
                    ReferenceType = "CASHBOOK",
                    ReferenceId = cashBook.Id,
                    PartyId = cashBook.PartyId,
                    LocationId = cashBook.LocationId,
                    BaseCurrencyId = cashBook.BaseCurrencyId,
                    EnteredCurrencyId = cashBook.EnteredCurrencyId,
                    CreatedBy = userId,
                    CreatedFrom = clientInfo,
                    Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
                };

                // Get payment method account (cash/bank account) from Chart of Accounts with InterfaceType='PAYMENT METHOD'
                int paymentMethodAccountId = 0;
                ChartOfAccountsModel? paymentMethodAccount = null;
                
                if (string.IsNullOrWhiteSpace(cashBook.PaymentMethod))
                {
                    transaction.Rollback();
                    return (false, null!, "Payment Method is required.");
                }
                
                // Find Chart of Account that matches the payment method value
                // Match payment method value to account Name where InterfaceType = 'PAYMENT METHOD'
                string paymentMethodUpper = cashBook.PaymentMethod.ToUpper().Replace("'", "''");
                string sqlPaymentMethod = $@"SELECT TOP 1 coa.* FROM ChartOfAccounts coa 
                                            WHERE coa.OrganizationId = {cashBook.OrganizationId} 
                                            AND coa.InterfaceType = 'PAYMENT METHOD'
                                            AND coa.IsActive = 1
                                            AND (
                                                UPPER(coa.Name) = '{paymentMethodUpper}'
                                                OR UPPER(coa.Name) LIKE '%{paymentMethodUpper}%'
                                                OR UPPER(coa.Name) LIKE '%{paymentMethodUpper.Replace(" ", "%")}%'
                                                OR '{paymentMethodUpper}' LIKE '%' + UPPER(coa.Name) + '%'
                                            )
                                            ORDER BY 
                                                CASE 
                                                    WHEN UPPER(coa.Name) = '{paymentMethodUpper}' THEN 1
                                                    WHEN UPPER(coa.Name) LIKE '{paymentMethodUpper}%' THEN 2
                                                    WHEN UPPER(coa.Name) LIKE '%{paymentMethodUpper}%' THEN 3
                                                    WHEN '{paymentMethodUpper}' LIKE '%' + UPPER(coa.Name) + '%' THEN 4
                                                    ELSE 5
                                                END,
                                                coa.Code";
                var paymentMethodResult = await connection.QueryAsync<ChartOfAccountsModel>(sqlPaymentMethod, transaction: transaction);
                paymentMethodAccount = paymentMethodResult?.FirstOrDefault();
                
                // If no match found, fallback to any payment method account
                if (paymentMethodAccount == null || paymentMethodAccount.Id == 0)
                {
                    string sqlPaymentMethodFallback = $@"SELECT TOP 1 coa.* FROM ChartOfAccounts coa 
                                                        WHERE coa.OrganizationId = {cashBook.OrganizationId} 
                                                        AND coa.InterfaceType = 'PAYMENT METHOD'
                                                        AND coa.IsActive = 1
                                                        ORDER BY coa.Code";
                    var fallbackResult = await connection.QueryAsync<ChartOfAccountsModel>(sqlPaymentMethodFallback, transaction: transaction);
                    paymentMethodAccount = fallbackResult?.FirstOrDefault();
                }
                
                if (paymentMethodAccount != null && paymentMethodAccount.Id > 0)
                {
                    paymentMethodAccountId = paymentMethodAccount.Id;
                }

                // Get account details for the selected account
                string sqlSelectedAccount = $@"SELECT coa.* FROM ChartOfAccounts coa WHERE coa.Id = {cashBook.AccountId} AND coa.IsActive = 1";
                var selectedAccountResult = await connection.QueryAsync<ChartOfAccountsModel>(sqlSelectedAccount, transaction: transaction);
                var selectedAccount = selectedAccountResult?.FirstOrDefault();
                
                if (selectedAccount == null || selectedAccount.Id == 0)
                {
                    transaction.Rollback();
                    return (false, null!, $"Selected account (ID: {cashBook.AccountId}) not found or inactive. Please ensure the account exists in Chart of Accounts.");
                }

                // Create GL entries based on transaction type
                string description = cashBook.Description ?? $"{cashBook.TranType} {cashBook.SeqNo}";
                
                if (cashBook.TranType?.ToUpper() == "RECEIPT")
                {
                    // Debit cash/bank account (from PaymentMethod)
                    if (paymentMethodAccountId > 0 && paymentMethodAccount != null)
                    {
                        GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, cashBook.Amount, 0, description, cashBook.PartyId);
                    }
                    else
                    {
                        transaction.Rollback();
                        return (false, null!, $"No Chart of Account found with InterfaceType='PAYMENT METHOD'. Please add a Chart of Account with InterfaceType='PAYMENT METHOD' for your organization.");
                    }
                    
                    // Credit the specified account (Revenue/Accounts Receivable)
                    GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, 0, cashBook.Amount, description, cashBook.PartyId);
                }
                else if (cashBook.TranType?.ToUpper() == "PAYMENT")
                {
                    // Debit the specified account (Expense/Accounts Payable)
                    GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, cashBook.Amount, 0, description, cashBook.PartyId);
                    
                    // Credit cash/bank account (from PaymentMethod)
                    if (paymentMethodAccountId > 0 && paymentMethodAccount != null)
                    {
                        GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, 0, cashBook.Amount, description, cashBook.PartyId);
                    }
                    else
                    {
                        transaction.Rollback();
                        return (false, null!, $"No Chart of Account found with InterfaceType='PAYMENT METHOD'. Please add a Chart of Account with InterfaceType='PAYMENT METHOD' for your organization.");
                    }
                }
                else if (cashBook.TranType?.ToUpper() == "BANK DEPOSIT")
                {
                    // For BANK DEPOSIT: Debit bank account, Credit cash account
                    if (paymentMethodAccountId > 0 && paymentMethodAccount != null)
                    {
                        // Debit bank account
                        GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, cashBook.Amount, 0, description, cashBook.PartyId);
                        
                        // Credit cash account (the selected account)
                        GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, 0, cashBook.Amount, description, cashBook.PartyId);
                    }
                    else
                    {
                        transaction.Rollback();
                        return (false, null!, $"No Chart of Account found with InterfaceType='PAYMENT METHOD'. Please add a Chart of Account with InterfaceType='PAYMENT METHOD' for your organization.");
                    }
                }

                // Calculate totals
                glHeader.TotalDebit = glHeader.Details.Sum(d => d.DebitAmount);
                glHeader.TotalCredit = glHeader.Details.Sum(d => d.CreditAmount);

                // Validate balance
                if (Math.Abs(glHeader.TotalDebit - glHeader.TotalCredit) > 0.01)
                {
                    transaction.Rollback();
                    return (false, null!, $"GL entry is not balanced. Total Debit: {glHeader.TotalDebit:N2}, Total Credit: {glHeader.TotalCredit:N2}");
                }

                // Insert GL Header
                string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{entryNo.ToUpper()}' AND IsSoftDeleted = 0;";
                string SQLInsertHeader = $@"INSERT INTO GeneralLedgerHeader 
                    (
                        OrganizationId, EntryNo, EntryDate, Source, Description, ReferenceNo, ReferenceType, ReferenceId,
                        PartyId, LocationId, BaseCurrencyId, EnteredCurrencyId, TotalDebit, TotalCredit, IsReversed, ReversedEntryNo, IsPosted,
                        PostedDate, PostedBy, IsAdjusted, AdjustmentEntryNo, FileAttachment, Notes,
                        CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                    ) 
                    VALUES 
                    (
                        {glHeader.OrganizationId}, '{entryNo.ToUpper()}', '{glHeader.EntryDate:yyyy-MM-dd}',
                        '{glHeader.Source?.ToUpper().Replace("'", "''") ?? "CASHBOOK"}', '{glHeader.Description!.ToUpper().Replace("'", "''")}',
                        '{glHeader.ReferenceNo?.ToUpper().Replace("'", "''") ?? ""}', '{glHeader.ReferenceType?.ToUpper().Replace("'", "''") ?? ""}',
                        {glHeader.ReferenceId}, {(glHeader.PartyId > 0 ? glHeader.PartyId.ToString() : "NULL")},
                        {(glHeader.LocationId > 0 ? glHeader.LocationId.ToString() : "NULL")},
                        {(glHeader.BaseCurrencyId > 0 ? glHeader.BaseCurrencyId.ToString() : "NULL")},
                        {(glHeader.EnteredCurrencyId > 0 ? glHeader.EnteredCurrencyId.ToString() : "NULL")},
                        {glHeader.TotalDebit}, {glHeader.TotalCredit},
                        {glHeader.IsReversed}, '{glHeader.ReversedEntryNo?.ToUpper().Replace("'", "''") ?? ""}',
                        {glHeader.IsPosted}, {(glHeader.PostedDate.HasValue ? $"'{glHeader.PostedDate.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
                        {(glHeader.PostedBy > 0 ? glHeader.PostedBy.ToString() : "NULL")}, {glHeader.IsAdjusted},
                        '{glHeader.AdjustmentEntryNo?.ToUpper().Replace("'", "''") ?? ""}', '{glHeader.FileAttachment?.Replace("'", "''") ?? ""}',
                        '{glHeader.Notes?.ToUpper().Replace("'", "''") ?? ""}', {glHeader.CreatedBy},
                        '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', '{glHeader.CreatedFrom!.ToUpper()}', {glHeader.IsSoftDeleted}
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                // Check for duplicate
                if (!string.IsNullOrEmpty(SQLDuplicate))
                {
                    var duplicateCheck = await connection.QueryAsync(SQLDuplicate, transaction: transaction);
                    if (duplicateCheck.Any())
                    {
                        transaction.Rollback();
                        return (false, null!, "Duplicate Entry No Found.");
                    }
                }

                // Insert header
                await connection.ExecuteAsync(SQLInsertHeader.TrimEnd(';'), transaction: transaction);
                int headerId = Convert.ToInt32(await connection.ExecuteScalarAsync("SELECT SCOPE_IDENTITY();", transaction: transaction));

                // Insert Details
                int seqNo = 1;
                foreach (var detail in glHeader.Details)
                {
                    string SQLInsertDetail = $@"INSERT INTO GeneralLedgerDetail 
                        (HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo,
                         CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted)
                        VALUES
                        ({headerId}, {detail.AccountId}, '{detail.Description?.ToUpper().Replace("'", "''") ?? ""}',
                         {detail.DebitAmount}, {detail.CreditAmount}, {(detail.PartyId > 0 ? detail.PartyId.ToString() : "NULL")},
                         {seqNo++}, {glHeader.CreatedBy}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                         '{glHeader.CreatedFrom!.ToUpper()}', {detail.IsSoftDeleted});";
                    
                    await connection.ExecuteAsync(SQLInsertDetail.TrimEnd(';'), transaction: transaction);
                }

                // Update cashbook with GL posting info
                string SQLUpdateCashBook = $@"UPDATE Cashbook SET 
                    IsPostedToGL = 1,
                    PostedToGLDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    PostedToGLBy = {userId},
                    GLEntryNo = '{entryNo}'
                WHERE Id = {cashBookId}";

                await connection.ExecuteAsync(SQLUpdateCashBook, transaction: transaction);
                transaction.Commit();

                // Get the created GL entry
                var result = await service.Get(headerId);
                return (true, result!.Item2!, "");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CreateGLFromCashBookHelper CreateGLFromCashBook Error");
            return (false, null!, ex.Message);
        }
    }

}


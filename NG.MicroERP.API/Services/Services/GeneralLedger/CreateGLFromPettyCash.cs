using Dapper;
using Microsoft.Data.SqlClient;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Models;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services.GeneralLedger;

public static class CreateGLFromPettyCashHelper
{
    public static async Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromPettyCash(
        IGeneralLedgerService service,
        int pettyCashId,
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
                // Get pettycash data
                string SQLPettyCash = $@"SELECT * FROM PettyCash WHERE Id = {pettyCashId} AND IsSoftDeleted = 0";
                var pettyCash = (await connection.QueryAsync<PettyCashModel>(SQLPettyCash, transaction: transaction))?.FirstOrDefault();
                
                if (pettyCash == null || pettyCash.Id == 0)
                {
                    transaction.Rollback();
                    return (false, null!, "PettyCash entry not found.");
                }

                // Check if already posted
                if (pettyCash.IsPostedToGL == 1)
                {
                    transaction.Rollback();
                    return (false, null!, "PettyCash entry is already posted to General Ledger.");
                }

                // Validate period for PETTYCASH module
                var periodCheck = await service.ValidatePeriod(pettyCash.OrganizationId, pettyCash.TranDate ?? DateTime.Now, "PETTYCASH");
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
                    OrganizationId = pettyCash.OrganizationId,
                    EntryNo = entryNo,
                    EntryDate = pettyCash.TranDate ?? DateTime.Now,
                    Source = "PETTYCASH",
                    Description = $"PettyCash {pettyCash.SeqNo}",
                    ReferenceNo = pettyCash.SeqNo,
                    ReferenceType = "PETTYCASH",
                    ReferenceId = pettyCash.Id,
                    PartyId = 0, // PettyCash uses EmployeeId, not PartyId
                    LocationId = pettyCash.LocationId,
                    BaseCurrencyId = pettyCash.BaseCurrencyId,
                    EnteredCurrencyId = pettyCash.EnteredCurrencyId,
                    CreatedBy = userId,
                    CreatedFrom = clientInfo,
                    Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
                };

                // Get payment method account (cash/bank account) from Chart of Accounts with InterfaceType='PAYMENT METHOD'
                int paymentMethodAccountId = 0;
                ChartOfAccountsModel? paymentMethodAccount = null;
                
                if (string.IsNullOrWhiteSpace(pettyCash.PaymentMethod))
                {
                    transaction.Rollback();
                    return (false, null!, "Payment Method is required.");
                }
                
                // Find Chart of Account that matches the payment method value
                // Match payment method value to account Name where InterfaceType = 'PAYMENT METHOD'
                string paymentMethodUpper = pettyCash.PaymentMethod.ToUpper().Replace("'", "''");
                string sqlPaymentMethod = $@"SELECT TOP 1 coa.* FROM ChartOfAccounts coa 
                                            WHERE coa.OrganizationId = {pettyCash.OrganizationId} 
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
                                                        WHERE coa.OrganizationId = {pettyCash.OrganizationId} 
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
                string sqlSelectedAccount = $@"SELECT coa.* FROM ChartOfAccounts coa WHERE coa.Id = {pettyCash.AccountId} AND coa.IsActive = 1";
                var selectedAccountResult = await connection.QueryAsync<ChartOfAccountsModel>(sqlSelectedAccount, transaction: transaction);
                var selectedAccount = selectedAccountResult?.FirstOrDefault();
                
                if (selectedAccount == null || selectedAccount.Id == 0)
                {
                    transaction.Rollback();
                    return (false, null!, $"Selected account (ID: {pettyCash.AccountId}) not found or inactive. Please ensure the account exists in Chart of Accounts.");
                }

                // Create GL entries based on transaction type
                string description = pettyCash.Description ?? $"{pettyCash.TranType} {pettyCash.SeqNo}";
                
                if (pettyCash.TranType?.ToUpper() == "RECEIPT")
                {
                    // Debit cash/bank account (from PaymentMethod)
                    if (paymentMethodAccountId > 0 && paymentMethodAccount != null)
                    {
                        GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, pettyCash.Amount, 0, description, 0);
                    }
                    else
                    {
                        transaction.Rollback();
                        return (false, null!, $"No Chart of Account found with InterfaceType='PAYMENT METHOD'. Please add a Chart of Account with InterfaceType='PAYMENT METHOD' for your organization.");
                    }
                    
                    // Credit the specified account (Revenue/Employee)
                    GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, 0, pettyCash.Amount, description, 0);
                }
                else if (pettyCash.TranType?.ToUpper() == "PAYMENT")
                {
                    // Debit the specified account (Expense/Employee)
                    GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, pettyCash.Amount, 0, description, 0);
                    
                    // Credit cash/bank account (from PaymentMethod)
                    if (paymentMethodAccountId > 0 && paymentMethodAccount != null)
                    {
                        GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, 0, pettyCash.Amount, description, 0);
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
                        EmployeeId, LocationId, BaseCurrencyId, EnteredCurrencyId, TotalDebit, TotalCredit, IsReversed, ReversedEntryNo, IsPosted,
                        PostedDate, PostedBy, IsAdjusted, AdjustmentEntryNo, FileAttachment, Notes,
                        CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                    ) 
                    VALUES 
                    (
                        {glHeader.OrganizationId}, '{entryNo.ToUpper()}', '{glHeader.EntryDate:yyyy-MM-dd}',
                        '{glHeader.Source?.ToUpper().Replace("'", "''") ?? "PETTYCASH"}', '{glHeader.Description!.ToUpper().Replace("'", "''")}',
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
                        (HeaderId, AccountId, Description, DebitAmount, CreditAmount, EmployeeId, SeqNo,
                         CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted)
                        VALUES
                        ({headerId}, {detail.AccountId}, '{detail.Description?.ToUpper().Replace("'", "''") ?? ""}',
                         {detail.DebitAmount}, {detail.CreditAmount}, {(detail.PartyId > 0 ? detail.PartyId.ToString() : "NULL")},
                         {seqNo++}, {glHeader.CreatedBy}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                         '{glHeader.CreatedFrom!.ToUpper()}', {detail.IsSoftDeleted});";
                    
                    await connection.ExecuteAsync(SQLInsertDetail.TrimEnd(';'), transaction: transaction);
                }

                // Update pettycash with GL posting info
                string SQLUpdatePettyCash = $@"UPDATE PettyCash SET 
                    IsPostedToGL = 1,
                    PostedToGLDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    PostedToGLBy = {userId},
                    GLEntryNo = '{entryNo}'
                WHERE Id = {pettyCashId}";

                await connection.ExecuteAsync(SQLUpdatePettyCash, transaction: transaction);
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
            Log.Error(ex, "CreateGLFromPettyCashHelper CreateGLFromPettyCash Error");
            return (false, null!, ex.Message);
        }
    }

}

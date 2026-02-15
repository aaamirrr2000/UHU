using MicroERP.API.Helper;
using MicroERP.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroERP.API.Services.GeneralLedger;

public static class PreviewGLFromCashBookHelper
{
    public static async Task<(bool, GeneralLedgerHeaderModel?, string)> PreviewGLFromCashBook(CashBookModel cashBook)
    {
        try
        {
            if (cashBook == null)
            {
                string errorMsg = "PreviewGLFromCashBook: CashBook model is null";
                Log.Warning($"PreviewGLFromCashBook: {errorMsg}");
                return (false, null, errorMsg);
            }

            // Validate required fields for preview
            if (cashBook.AccountId == 0)
            {
                string errorMsg = "PreviewGLFromCashBook: Please select an Account first";
                Log.Warning($"PreviewGLFromCashBook: {errorMsg}");
                return (false, null, errorMsg);
            }

            if (cashBook.Amount == 0)
            {
                string errorMsg = "PreviewGLFromCashBook: Please enter an Amount first";
                Log.Warning($"PreviewGLFromCashBook: {errorMsg}");
                return (false, null, errorMsg);
            }

            if (string.IsNullOrWhiteSpace(cashBook.TranType))
            {
                string errorMsg = "PreviewGLFromCashBook: Please select a Transaction Type first";
                Log.Warning($"PreviewGLFromCashBook: {errorMsg}");
                return (false, null, errorMsg);
            }

            if (cashBook.OrganizationId == 0)
            {
                string errorMsg = "PreviewGLFromCashBook: OrganizationId is required";
                Log.Warning($"PreviewGLFromCashBook: {errorMsg}");
                return (false, null, errorMsg);
            }

            DapperFunctions dapper = new DapperFunctions();
            
            // If cashbook is already posted, return the actual GL entry
            if (cashBook.Id > 0 && cashBook.IsPostedToGL == 1 && !string.IsNullOrWhiteSpace(cashBook.GLEntryNo))
            {
                // Get the actual posted GL entry by EntryNo
                string sqlPostedEntry = $@"SELECT glh.*, p.Name as PartyName, loc.Name as LocationName
                                          FROM GeneralLedgerHeader as glh
                                          LEFT JOIN Parties as p on p.Id=glh.PartyId
                                          LEFT JOIN Locations as loc on loc.Id=glh.LocationId
                                          WHERE glh.EntryNo = '{cashBook.GLEntryNo}' AND glh.IsSoftDeleted=0";
                
                var postedHeader = (await dapper.SearchByQuery<GeneralLedgerHeaderModel>(sqlPostedEntry))?.FirstOrDefault();
                
                if (postedHeader != null && postedHeader.Id > 0)
                {
                    // Get details for the posted entry
                    string sqlPostedDetails = $@"SELECT gld.*, coa.Code as AccountCode, coa.Name as AccountName, coa.Type as AccountType,
                                                p.Name as PartyName, loc.Name as LocationName
                                                FROM GeneralLedgerDetail as gld
                                                LEFT JOIN ChartOfAccounts as coa on coa.Id=gld.AccountId
                                                LEFT JOIN Parties as p on p.Id=gld.PartyId
                                                LEFT JOIN Locations as loc on loc.Id=gld.LocationId
                                                WHERE gld.HeaderId={postedHeader.Id} AND gld.IsSoftDeleted=0
                                                ORDER BY gld.SeqNo, gld.Id";
                    
                    var postedDetails = await dapper.SearchByQuery<GeneralLedgerDetailModel>(sqlPostedDetails);
                    postedHeader.Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>(postedDetails ?? new List<GeneralLedgerDetailModel>());
                    
                    return (true, postedHeader, "");
                }
            }
            
            // Create preview GL header (not saved)
            GeneralLedgerHeaderModel glHeader = new GeneralLedgerHeaderModel
            {
                OrganizationId = cashBook.OrganizationId,
                EntryNo = "PREVIEW",
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
                Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>(),
                Warnings = new List<string>()
            };

            // Get account details for cashbook account
            string sqlAccount = $@"SELECT coa.* FROM ChartOfAccounts coa WHERE coa.Id = {cashBook.AccountId} AND coa.IsActive = 1";
            var accountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlAccount);
            var account = accountResult?.FirstOrDefault();

            ChartOfAccountsModel? selectedAccount = null;
            if (account == null || account.Id == 0)
            {
                string warningMsg = $"Selected Account (ID: {cashBook.AccountId}) not found or inactive. Entry will show as missing account.";
                glHeader.Warnings.Add(warningMsg);
                Log.Warning($"PreviewGLFromCashBook: {warningMsg}");
                // Create placeholder account
                selectedAccount = new ChartOfAccountsModel
                {
                    Id = 0,
                    Code = "MISSING",
                    Name = "SELECTED ACCOUNT (NOT FOUND)",
                    Type = "ASSET"
                };
            }
            else
            {
                selectedAccount = account;
            }

            // Get payment method account using common helper
            ChartOfAccountsModel? paymentMethodAccount = null;
            if (string.IsNullOrWhiteSpace(cashBook.PaymentMethod))
            {
                string warningMsg = "Payment Method not selected. Payment method entry will be skipped.";
                glHeader.Warnings.Add(warningMsg);
                Log.Warning($"PreviewGLFromCashBook: {warningMsg}");
                // Create placeholder account
                paymentMethodAccount = new ChartOfAccountsModel
                {
                    Id = 0,
                    Code = "MISSING",
                    Name = "PAYMENT METHOD (NOT SELECTED)",
                    Type = "ASSET"
                };
            }
            else
            {
                var paymentMethodResult = await GLAccountLookupHelper.GetPaymentMethodAccount(
                    dapper,
                    cashBook.OrganizationId,
                    cashBook.PaymentMethod,
                    required: false);
                
                if (paymentMethodResult == null || paymentMethodResult.Id == 0)
                {
                    string warningMsg = $"No Chart of Account found with InterfaceType='PAYMENT METHOD' for PaymentMethod='{cashBook.PaymentMethod}'. Payment method entry will show as missing account.";
                    glHeader.Warnings.Add(warningMsg);
                    Log.Warning($"PreviewGLFromCashBook: {warningMsg}");
                    // Create placeholder account
                    paymentMethodAccount = new ChartOfAccountsModel
                    {
                        Id = 0,
                        Code = "MISSING",
                        Name = "PAYMENT METHOD ACCOUNT (NOT CONFIGURED)",
                        Type = "ASSET"
                    };
                }
                else
                {
                    paymentMethodAccount = paymentMethodResult;
                }
            }

            // Create GL entries based on transaction type
            string description = cashBook.Description ?? $"{cashBook.TranType} {cashBook.SeqNo}";
            
            if (cashBook.TranType?.ToUpper() == "RECEIPT")
            {
                // Debit cash/bank account (from PaymentMethod)
                GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, (double)cashBook.Amount, 0, description, cashBook.PartyId);
                
                // Credit the specified account (Revenue/Accounts Receivable)
                GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, 0, (double)cashBook.Amount, description, cashBook.PartyId);
            }
            else if (cashBook.TranType?.ToUpper() == "PAYMENT")
            {
                // Debit the specified account (Expense/Accounts Payable)
                GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, (double)cashBook.Amount, 0, description, cashBook.PartyId);
                
                // Credit cash/bank account (from PaymentMethod)
                GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, 0, (double)cashBook.Amount, description, cashBook.PartyId);
            }
            else if (cashBook.TranType?.ToUpper() == "BANK DEPOSIT")
            {
                // For BANK DEPOSIT: Debit bank account, Credit cash account
                // Debit bank account
                GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, (double)cashBook.Amount, 0, description, cashBook.PartyId);
                
                // Credit cash account (the selected account)
                GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, 0, (double)cashBook.Amount, description, cashBook.PartyId);
            }

            // Calculate totals
            glHeader.TotalDebit = glHeader.Details.Sum(d => d.DebitAmount);
            glHeader.TotalCredit = glHeader.Details.Sum(d => d.CreditAmount);

            // Always return success with warnings (if any)
            string warningSummary = glHeader.Warnings.Any() 
                ? $"Preview generated with {glHeader.Warnings.Count} warning(s). Please review and configure missing accounts before posting." 
                : "";
            return (true, glHeader, warningSummary);
        }
        catch (Exception ex)
        {
            string errorMsg = $"PreviewGLFromCashBook: Error generating preview: {ex.Message}";
            Log.Error(ex, "PreviewGLFromCashBook Error: {Message}", ex.Message);
            return (false, null, errorMsg);
        }
    }

}


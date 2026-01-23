using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services.GeneralLedger;

public static class PreviewGLFromPettyCashHelper
{
    public static async Task<(bool, GeneralLedgerHeaderModel?, string)> PreviewGLFromPettyCash(PettyCashModel pettyCash)
    {
        try
        {
            if (pettyCash == null)
            {
                string errorMsg = "PreviewGLFromPettyCash: PettyCash model is null";
                Log.Warning($"PreviewGLFromPettyCash: {errorMsg}");
                return (false, null, errorMsg);
            }

            // Validate required fields for preview
            if (pettyCash.AccountId == 0)
            {
                string errorMsg = "PreviewGLFromPettyCash: Please select an Account first";
                Log.Warning($"PreviewGLFromPettyCash: {errorMsg}");
                return (false, null, errorMsg);
            }

            if (pettyCash.Amount == 0)
            {
                string errorMsg = "PreviewGLFromPettyCash: Please enter an Amount first";
                Log.Warning($"PreviewGLFromPettyCash: {errorMsg}");
                return (false, null, errorMsg);
            }

            if (string.IsNullOrWhiteSpace(pettyCash.TranType))
            {
                string errorMsg = "PreviewGLFromPettyCash: Please select a Transaction Type first";
                Log.Warning($"PreviewGLFromPettyCash: {errorMsg}");
                return (false, null, errorMsg);
            }

            if (pettyCash.OrganizationId == 0)
            {
                string errorMsg = "PreviewGLFromPettyCash: OrganizationId is required";
                Log.Warning($"PreviewGLFromPettyCash: {errorMsg}");
                return (false, null, errorMsg);
            }

            DapperFunctions dapper = new DapperFunctions();
            
            // If pettycash is already posted, return the actual GL entry
            if (pettyCash.Id > 0 && pettyCash.IsPostedToGL == 1 && !string.IsNullOrWhiteSpace(pettyCash.GLEntryNo))
            {
                // Get the actual posted GL entry by EntryNo
                string sqlPostedEntry = $@"SELECT glh.*, loc.Name as LocationName
                                          FROM GeneralLedgerHeader as glh
                                          LEFT JOIN Locations as loc on loc.Id=glh.LocationId
                                          WHERE glh.EntryNo = '{pettyCash.GLEntryNo}' AND glh.IsSoftDeleted=0";
                
                var postedHeader = (await dapper.SearchByQuery<GeneralLedgerHeaderModel>(sqlPostedEntry))?.FirstOrDefault();
                
                if (postedHeader != null && postedHeader.Id > 0)
                {
                    // Get details for the posted entry
                    string sqlPostedDetails = $@"SELECT gld.*, coa.Code as AccountCode, coa.Name as AccountName, coa.Type as AccountType,
                                                loc.Name as LocationName
                                                FROM GeneralLedgerDetail as gld
                                                LEFT JOIN ChartOfAccounts as coa on coa.Id=gld.AccountId
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
                OrganizationId = pettyCash.OrganizationId,
                EntryNo = "PREVIEW",
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
                Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>(),
                Warnings = new List<string>()
            };

            // Get account details for pettycash account
            string sqlAccount = $@"SELECT coa.* FROM ChartOfAccounts coa WHERE coa.Id = {pettyCash.AccountId} AND coa.IsActive = 1";
            var accountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlAccount);
            var account = accountResult?.FirstOrDefault();

            ChartOfAccountsModel? selectedAccount = null;
            if (account == null || account.Id == 0)
            {
                string warningMsg = $"Selected Account (ID: {pettyCash.AccountId}) not found or inactive. Entry will show as missing account.";
                glHeader.Warnings.Add(warningMsg);
                Log.Warning($"PreviewGLFromPettyCash: {warningMsg}");
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
            if (string.IsNullOrWhiteSpace(pettyCash.PaymentMethod))
            {
                string warningMsg = "Payment Method not selected. Payment method entry will be skipped.";
                glHeader.Warnings.Add(warningMsg);
                Log.Warning($"PreviewGLFromPettyCash: {warningMsg}");
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
                    pettyCash.OrganizationId,
                    pettyCash.PaymentMethod,
                    required: false);
                
                if (paymentMethodResult == null || paymentMethodResult.Id == 0)
                {
                    string warningMsg = $"No Chart of Account found with InterfaceType='PAYMENT METHOD' for PaymentMethod='{pettyCash.PaymentMethod}'. Payment method entry will show as missing account.";
                    glHeader.Warnings.Add(warningMsg);
                    Log.Warning($"PreviewGLFromPettyCash: {warningMsg}");
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
            string description = pettyCash.Description ?? $"{pettyCash.TranType} {pettyCash.SeqNo}";
            
            if (pettyCash.TranType?.ToUpper() == "RECEIPT")
            {
                // Debit cash/bank account (from PaymentMethod)
                GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, (double)pettyCash.Amount, 0, description, 0);
                
                // Credit the specified account (Revenue/Employee)
                GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, 0, (double)pettyCash.Amount, description, 0);
            }
            else if (pettyCash.TranType?.ToUpper() == "PAYMENT")
            {
                // Debit the specified account (Expense/Employee)
                GLAccountLookupHelper.AddGLEntry(glHeader, selectedAccount, (double)pettyCash.Amount, 0, description, 0);
                
                // Credit cash/bank account (from PaymentMethod)
                GLAccountLookupHelper.AddGLEntry(glHeader, paymentMethodAccount, 0, (double)pettyCash.Amount, description, 0);
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
            string errorMsg = $"PreviewGLFromPettyCash: Error generating preview: {ex.Message}";
            Log.Error(ex, "PreviewGLFromPettyCash Error: {Message}", ex.Message);
            return (false, null, errorMsg);
        }
    }

}

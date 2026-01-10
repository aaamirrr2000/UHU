using Dapper;
using Microsoft.Data.SqlClient;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services;

public interface IGeneralLedgerService
{
    Task<(bool, List<GeneralLedgerHeaderModel>)>? Search(string Criteria = "");
    Task<(bool, GeneralLedgerHeaderModel?)>? Get(int id);
    Task<(bool, GeneralLedgerReportModel?)>? GetGeneralLedgerReport(int id);
    Task<(bool, GeneralLedgerHeaderModel, string)> Post(GeneralLedgerHeaderModel obj);
    Task<(bool, GeneralLedgerHeaderModel, string)> Put(GeneralLedgerHeaderModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(GeneralLedgerHeaderModel obj);
    Task<(bool, string)> PostEntry(string entryNo);
    Task<(bool, string)> ValidatePeriod(int organizationId, DateTime date, string moduleType = "GENERALLEDGER");
    Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromInvoice(int invoiceId, int userId, string clientInfo);
    Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromCashBook(int cashBookId, int userId, string clientInfo);
    Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromPettyCash(int pettyCashId, int userId, string clientInfo);
    Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromAdvances(int advancesId, int userId, string clientInfo);
    Task<(bool, GeneralLedgerHeaderModel?)> PreviewGLFromInvoice(InvoicesModel invoice);
    Task<(bool, GeneralLedgerHeaderModel?)> PreviewGLFromCashBook(CashBookModel cashBook);
    Task<(bool, GeneralLedgerHeaderModel?)> PreviewGLFromPettyCash(PettyCashModel pettyCash);
    Task<(bool, GeneralLedgerHeaderModel?)> PreviewGLFromAdvances(AdvancesModel advances);
}

public class GeneralLedgerService : IGeneralLedgerService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<GeneralLedgerHeaderModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT glh.*, p.Name as PartyName, loc.Name as LocationName
                        FROM GeneralLedgerHeader as glh
                        LEFT JOIN Parties as p on p.Id=glh.PartyId
                        LEFT JOIN Locations as loc on loc.Id=glh.LocationId
                        Where glh.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by glh.EntryDate Desc, glh.EntryNo";

        List<GeneralLedgerHeaderModel> result = (await dapper.SearchByQuery<GeneralLedgerHeaderModel>(SQL)) ?? new List<GeneralLedgerHeaderModel>();

        if (result == null || result.Count == 0)
            return (false, new List<GeneralLedgerHeaderModel>());
        else
            return (true, result);
    }

    public async Task<(bool, GeneralLedgerHeaderModel?)>? Get(int id)
    {
        // Get Header
        string SQLHeader = $@"SELECT glh.*, p.Name as PartyName, loc.Name as LocationName
                             FROM GeneralLedgerHeader as glh
                             LEFT JOIN Parties as p on p.Id=glh.PartyId
                             LEFT JOIN Locations as loc on loc.Id=glh.LocationId
                             Where glh.Id={id} AND glh.IsSoftDeleted=0";

        GeneralLedgerHeaderModel header = (await dapper.SearchByQuery<GeneralLedgerHeaderModel>(SQLHeader))?.FirstOrDefault() ?? new GeneralLedgerHeaderModel();
        
        if (header == null || header.Id == 0)
            return (false, null);

        // Get Details
        string SQLDetails = $@"SELECT gld.*, coa.Code as AccountCode, coa.Name as AccountName, coa.Type as AccountType,
                               p.Name as PartyName, loc.Name as LocationName
                               FROM GeneralLedgerDetail as gld
                               LEFT JOIN ChartOfAccounts as coa on coa.Id=gld.AccountId
                               LEFT JOIN Parties as p on p.Id=gld.PartyId
                               LEFT JOIN Locations as loc on loc.Id=gld.LocationId
                               Where gld.HeaderId={id} AND gld.IsSoftDeleted=0
                               Order by gld.SeqNo, gld.Id";

        List<GeneralLedgerDetailModel> details = (await dapper.SearchByQuery<GeneralLedgerDetailModel>(SQLDetails)) ?? new List<GeneralLedgerDetailModel>();
        
        header.Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>(details);
        
        return (true, header);
    }

    public async Task<(bool, GeneralLedgerReportModel?)>? GetGeneralLedgerReport(int id)
    {
        GeneralLedgerReportModel result = (await dapper.SearchByID<GeneralLedgerReportModel>("vw_GeneralLedgerReport", id)) ?? new GeneralLedgerReportModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, GeneralLedgerHeaderModel, string)> Post(GeneralLedgerHeaderModel obj)
    {
        try
        {
            // Validate details
            if (obj.Details == null || obj.Details.Count == 0)
                return (false, null!, "At least one detail line is required.");

            // Calculate totals
            double totalDebit = obj.Details.Sum(d => d.DebitAmount);
            double totalCredit = obj.Details.Sum(d => d.CreditAmount);

            // Validate double-entry balance
            if (Math.Abs(totalDebit - totalCredit) > 0.01)
                return (false, null!, $"Entry is not balanced. Total Debit: {totalDebit:N2}, Total Credit: {totalCredit:N2}");

            // Validate period is open
            var periodCheck = await ValidatePeriod(obj.OrganizationId, obj.EntryDate ?? DateTime.Now, "GENERALLEDGER");
            if (!periodCheck.Item1)
                return (false, null!, periodCheck.Item2);

            // Auto-set ReferenceType based on Source
            if (string.IsNullOrWhiteSpace(obj.ReferenceType))
            {
                obj.ReferenceType = obj.Source?.ToUpper() == "MANUAL" ? "JOURNAL" : obj.Source?.ToUpper() ?? "JOURNAL";
            }

            // Generate EntryNo if not provided
            string entryNo = obj.EntryNo;
            if (string.IsNullOrWhiteSpace(entryNo))
            {
                entryNo = dapper.GetCode("GL", "GeneralLedgerHeader", "EntryNo")!;
            }

            string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{entryNo.ToUpper()}';";
            
            // Insert Header
            string SQLInsertHeader = $@"INSERT INTO GeneralLedgerHeader 
			(
				OrganizationId, 
				EntryNo,
				EntryDate,
				Source,
				Description,
				ReferenceNo,
				ReferenceType,
				ReferenceId,
				PartyId,
				LocationId,
				BaseCurrencyId,
				EnteredCurrencyId,
				TotalDebit,
				TotalCredit,
				IsReversed,
				ReversedEntryNo,
				IsPosted,
				PostedDate,
				PostedBy,
				IsAdjusted,
				AdjustmentEntryNo,
				FileAttachment,
				Notes,
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{entryNo.ToUpper()}', 
				'{obj.EntryDate:yyyy-MM-dd}',
				'{obj.Source?.ToUpper().Replace("'", "''") ?? "MANUAL"}',
				'{obj.Description!.ToUpper().Replace("'", "''")}', 
				'{obj.ReferenceNo?.ToUpper().Replace("'", "''") ?? ""}',
				'{obj.ReferenceType?.ToUpper().Replace("'", "''") ?? ""}',
				{obj.ReferenceId},
				{(obj.PartyId > 0 ? obj.PartyId.ToString() : "NULL")},
				{(obj.LocationId > 0 ? obj.LocationId.ToString() : "NULL")},
				{(obj.BaseCurrencyId > 0 ? obj.BaseCurrencyId.ToString() : "NULL")},
				{(obj.EnteredCurrencyId > 0 ? obj.EnteredCurrencyId.ToString() : "NULL")},
				{totalDebit},
				{totalCredit},
				{obj.IsReversed},
				'{obj.ReversedEntryNo?.ToUpper().Replace("'", "''") ?? ""}',
				{obj.IsPosted},
				{(obj.PostedDate.HasValue ? $"'{obj.PostedDate.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
				{(obj.PostedBy > 0 ? obj.PostedBy.ToString() : "NULL")},
				{obj.IsAdjusted},
				'{obj.AdjustmentEntryNo?.ToUpper().Replace("'", "''") ?? ""}',
				'{obj.FileAttachment?.Replace("'", "''") ?? ""}',
				'{obj.Notes?.ToUpper().Replace("'", "''") ?? ""}',
				{obj.CreatedBy},
				'{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);
			SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var res = await dapper.Insert(SQLInsertHeader, SQLDuplicate);
            if (!res.Item1)
                return (false, null!, "Duplicate Entry No found or insert failed.");

            int headerId = res.Item2;

            // Insert Details
            int seqNo = 1;
            foreach (var detail in obj.Details)
            {
                string SQLInsertDetail = $@"INSERT INTO GeneralLedgerDetail 
				(
					HeaderId,
					AccountId,
					Description,
					DebitAmount,
					CreditAmount,
					PartyId,
					LocationId,
					CostCenterId,
					ProjectId,
					CurrencyId,
					ExchangeRate,
					SeqNo,
					CreatedBy, 
					CreatedOn, 
					CreatedFrom, 
					IsSoftDeleted
				) 
				VALUES 
				(
					{headerId},
					{detail.AccountId},
					'{detail.Description?.ToUpper().Replace("'", "''") ?? ""}',
					{detail.DebitAmount},
					{detail.CreditAmount},
					{(detail.PartyId > 0 ? detail.PartyId.ToString() : "NULL")},
					{(detail.LocationId > 0 ? detail.LocationId.ToString() : "NULL")},
					{(detail.CostCenterId > 0 ? detail.CostCenterId.ToString() : "NULL")},
					{(detail.ProjectId > 0 ? detail.ProjectId.ToString() : "NULL")},
					{(detail.CurrencyId > 0 ? detail.CurrencyId.ToString() : "NULL")},
					{detail.ExchangeRate},
					{seqNo++},
					{obj.CreatedBy},
					'{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
					'{obj.CreatedFrom!.ToUpper()}', 
					{detail.IsSoftDeleted}
				);";

                var detailRes = await dapper.Insert(SQLInsertDetail);
                if (!detailRes.Item1)
                    return (false, null!, $"Failed to insert detail line {seqNo - 1}.");
            }

            // Return the complete header with details
            var result = await Get(headerId);
            return (true, result.Item2!, "");
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel, string)> Put(GeneralLedgerHeaderModel obj)
    {
        try
        {
            // Validate details
            if (obj.Details == null || obj.Details.Count == 0)
                return (false, null!, "At least one detail line is required.");

            // Calculate totals
            double totalDebit = obj.Details.Sum(d => d.DebitAmount);
            double totalCredit = obj.Details.Sum(d => d.CreditAmount);

            // Validate double-entry balance
            if (Math.Abs(totalDebit - totalCredit) > 0.01)
                return (false, null!, $"Entry is not balanced. Total Debit: {totalDebit:N2}, Total Credit: {totalCredit:N2}");

            string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{obj.EntryNo!.ToUpper()}' AND Id != {obj.Id};";
            
            // Update Header
            string SQLUpdateHeader = $@"UPDATE GeneralLedgerHeader SET 
					OrganizationId = {obj.OrganizationId}, 
					EntryNo = '{obj.EntryNo!.ToUpper()}',
					EntryDate = '{obj.EntryDate:yyyy-MM-dd}',
					Source = '{obj.Source?.ToUpper().Replace("'", "''") ?? "MANUAL"}',
					Description = '{obj.Description!.ToUpper().Replace("'", "''")}', 
					ReferenceNo = '{obj.ReferenceNo?.ToUpper().Replace("'", "''") ?? ""}',
					ReferenceType = '{obj.ReferenceType?.ToUpper().Replace("'", "''") ?? ""}',
					ReferenceId = {obj.ReferenceId},
					PartyId = {(obj.PartyId > 0 ? obj.PartyId.ToString() : "NULL")},
					LocationId = {(obj.LocationId > 0 ? obj.LocationId.ToString() : "NULL")},
					BaseCurrencyId = {(obj.BaseCurrencyId > 0 ? obj.BaseCurrencyId.ToString() : "NULL")},
					EnteredCurrencyId = {(obj.EnteredCurrencyId > 0 ? obj.EnteredCurrencyId.ToString() : "NULL")},
					TotalDebit = {totalDebit},
					TotalCredit = {totalCredit},
					IsReversed = {obj.IsReversed},
					ReversedEntryNo = '{obj.ReversedEntryNo?.ToUpper().Replace("'", "''") ?? ""}',
					IsPosted = {obj.IsPosted},
					PostedDate = {(obj.PostedDate.HasValue ? $"'{obj.PostedDate.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
					PostedBy = {(obj.PostedBy > 0 ? obj.PostedBy.ToString() : "NULL")},
					IsAdjusted = {obj.IsAdjusted},
					AdjustmentEntryNo = '{obj.AdjustmentEntryNo?.ToUpper().Replace("'", "''") ?? ""}',
					FileAttachment = '{obj.FileAttachment?.Replace("'", "''") ?? ""}',
					Notes = '{obj.Notes?.ToUpper().Replace("'", "''") ?? ""}',
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdateHeader, SQLDuplicate);
            if (!res.Item1)
                return (false, null!, "Duplicate Entry No found or update failed.");

            // Delete existing details
            string SQLDeleteDetails = $@"UPDATE GeneralLedgerDetail SET IsSoftDeleted = 1 WHERE HeaderId = {obj.Id};";
            await dapper.Update(SQLDeleteDetails);

            // Insert new details
            int seqNo = 1;
            foreach (var detail in obj.Details)
            {
                string SQLInsertDetail = $@"INSERT INTO GeneralLedgerDetail 
				(
					HeaderId,
					AccountId,
					Description,
					DebitAmount,
					CreditAmount,
					PartyId,
					LocationId,
					CostCenterId,
					ProjectId,
					CurrencyId,
					ExchangeRate,
					SeqNo,
					CreatedBy, 
					CreatedOn, 
					CreatedFrom, 
					IsSoftDeleted
				) 
				VALUES 
				(
					{obj.Id},
					{detail.AccountId},
					'{detail.Description?.ToUpper().Replace("'", "''") ?? ""}',
					{detail.DebitAmount},
					{detail.CreditAmount},
					{(detail.PartyId > 0 ? detail.PartyId.ToString() : "NULL")},
					{(detail.LocationId > 0 ? detail.LocationId.ToString() : "NULL")},
					{(detail.CostCenterId > 0 ? detail.CostCenterId.ToString() : "NULL")},
					{(detail.ProjectId > 0 ? detail.ProjectId.ToString() : "NULL")},
					{(detail.CurrencyId > 0 ? detail.CurrencyId.ToString() : "NULL")},
					{detail.ExchangeRate},
					{seqNo++},
					{obj.UpdatedBy},
					'{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
					'{obj.UpdatedFrom!.ToUpper()}', 
					{detail.IsSoftDeleted}
				);";

                var detailRes = await dapper.Insert(SQLInsertDetail);
                if (!detailRes.Item1)
                    return (false, null!, $"Failed to insert detail line {seqNo - 1}.");
            }

            // Return the complete header with details
            var result = await Get(obj.Id);
            return (true, result.Item2!, "");
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("GeneralLedgerHeader", id);
    }

    public async Task<(bool, string)> SoftDelete(GeneralLedgerHeaderModel obj)
    {
        string SQLUpdate = $@"UPDATE GeneralLedgerHeader SET 
					UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
					UpdatedBy = {obj.UpdatedBy},
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }

    public async Task<(bool, string)> PostEntry(string entryNo)
    {
        try
        {
            string SQLUpdate = $@"UPDATE GeneralLedgerHeader SET 
					IsPosted = 1,
					PostedDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
					PostedBy = (SELECT TOP 1 CreatedBy FROM GeneralLedgerHeader WHERE EntryNo = '{entryNo.ToUpper()}' AND IsSoftDeleted = 0)
				WHERE EntryNo = '{entryNo.ToUpper()}' AND IsSoftDeleted = 0;";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                return (true, "Entry posted successfully.");
            }
            else
            {
                return (false, "Failed to post entry.");
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool, string)> ValidatePeriod(int organizationId, DateTime date, string moduleType = "GENERALLEDGER")
    {
        try
        {
            string SQL = $@"SELECT * FROM PeriodClose 
                           WHERE OrganizationId = {organizationId}
                           AND IsSoftDeleted = 0
                           AND Status IN ('OPEN', 'OPEN_PENDING')
                           AND (ModuleType = '{moduleType}' OR ModuleType = 'ALL')
                           AND '{date:yyyy-MM-dd}' BETWEEN StartDate AND EndDate";

            var period = await dapper.SearchByQuery<PeriodCloseModel>(SQL);
            if (period == null || !period.Any())
            {
                return (false, $"No open period found for {moduleType} module on date {date:yyyy-MM-dd}. Please ensure the period is open before posting.");
            }
            return (true, "");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GeneralLedgerService ValidatePeriod Error");
            return (false, ex.Message);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromInvoice(int invoiceId, int userId, string clientInfo)
    {
        try
        {
            using var connection = new SqlConnection(new Config().DefaultConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get invoice data
                string SQLInvoice = $@"SELECT * FROM Invoice WHERE Id = {invoiceId} AND IsSoftDeleted = 0";
                var invoice = (await connection.QueryAsync<InvoiceModel>(SQLInvoice, transaction: transaction))?.FirstOrDefault();
                
                if (invoice == null || invoice.Id == 0)
                {
                    transaction.Rollback();
                    return (false, null!, "Invoice not found.");
                }

                // Check if already posted
                if (invoice.IsPostedToGL == 1)
                {
                    transaction.Rollback();
                    return (false, null!, "Invoice is already posted to General Ledger.");
                }

                // Validate period for SALE or PURCHASE invoice module
                string moduleType = invoice.InvoiceType?.ToUpper() == "PURCHASE" ? "PURCHASE" : "SALE";
                var periodCheck = await ValidatePeriod(invoice.OrganizationId, invoice.TranDate ?? DateTime.Now, moduleType);
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
                    OrganizationId = invoice.OrganizationId,
                    EntryNo = entryNo,
                    EntryDate = invoice.TranDate ?? DateTime.Now,
                    Source = "INVOICE",
                    Description = $"Invoice {invoice.Code}",
                    ReferenceNo = invoice.Code,
                    ReferenceType = "INVOICE",
                    ReferenceId = invoice.Id,
                    PartyId = invoice.PartyId,
                    LocationId = invoice.LocationId,
                    BaseCurrencyId = invoice.BaseCurrencyId,
                    EnteredCurrencyId = invoice.EnteredCurrencyId,
                    CreatedBy = userId,
                    CreatedFrom = clientInfo,
                    Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
                };

                // Map invoice to GL entry - use AccountId from invoice if available, otherwise lookup AR/AP
                int arApAccountId = 0;
                string accountTypeName = "";
                bool isSale = invoice.InvoiceType?.ToUpper() == "SALE";
                
                if (invoice.AccountId > 0)
                {
                    // Use the account specified in the invoice
                    arApAccountId = invoice.AccountId;
                    // Verify the account exists and get its code/name/type
                    string sqlVerifyAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                                 WHERE Id = {arApAccountId} AND OrganizationId = {invoice.OrganizationId} AND IsActive = 1";
                    var verifyResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlVerifyAccount, transaction: transaction);
                    if (verifyResult == null || verifyResult.Value.Id == 0)
                    {
                        transaction.Rollback();
                        return (false, null!, $"Account ID {arApAccountId} specified in invoice is not valid or inactive. Please select a valid account.");
                    }
                    accountTypeName = verifyResult.Value.Name;
                }
                else
                {
                    // Fallback to AR/AP lookup if AccountId not specified
                    string accountCode = isSale ? "AR" : "AP";
                    accountTypeName = isSale ? "Accounts Receivable (AR)" : "Accounts Payable (AP)";
                    string sqlAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {invoice.OrganizationId} 
                                          AND (Code = '{accountCode}' OR Code LIKE '{accountCode}%')
                                          AND IsActive = 1
                                          ORDER BY Code";
                    var accountResult = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlAccount, transaction: transaction);
                    
                    // Validate AR/AP account ID
                    if (accountResult == null || accountResult.Value.Id == 0)
                    {
                        transaction.Rollback();
                        return (false, null!, $"{accountTypeName} account not found for OrganizationId {invoice.OrganizationId}. Please configure the account in Chart of Accounts or specify an AccountId in the invoice before posting to GL.");
                    }
                    arApAccountId = accountResult.Value.Id;
                }

                // Get AR/AP account details for GL entry
                string sqlArApDetails = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts WHERE Id = {arApAccountId}";
                var arApAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(sqlArApDetails, transaction: transaction);
                if (arApAccount == null)
                {
                    transaction.Rollback();
                    return (false, null!, $"AR/AP account details not found.");
                }

                // For SALE: Debit AR, Credit Revenue/Tax/Charges, Debit Discount
                // For PURCHASE: Credit AP, Debit Expense/Tax/Charges, Credit Discount
                if (isSale)
                {
                    // Debit AR for TotalInvoiceAmount
                    glHeader.Details.Add(new GeneralLedgerDetailModel
                    {
                        AccountId = arApAccount.Value.Id,
                        AccountCode = arApAccount.Value.Code,
                        AccountName = arApAccount.Value.Name,
                        AccountType = arApAccount.Value.Type,
                        DebitAmount = (double)invoice.TotalInvoiceAmount,
                        CreditAmount = 0,
                        Description = $"Invoice {invoice.Code}",
                        PartyId = invoice.PartyId
                    });

                    // Credit Revenue (from InvoiceAmount)
                    if (invoice.InvoiceAmount > 0)
                    {
                        string revenueSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                              WHERE OrganizationId = {invoice.OrganizationId} 
                                              AND Type = 'REVENUE'
                                              AND IsActive = 1
                                              ORDER BY Code";
                        var revenueAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(revenueSql, transaction: transaction);
                        
                        if (revenueAccount != null && revenueAccount.Value.Id > 0)
                        {
                            glHeader.Details.Add(new GeneralLedgerDetailModel
                            {
                                AccountId = revenueAccount.Value.Id,
                                AccountCode = revenueAccount.Value.Code,
                                AccountName = revenueAccount.Value.Name,
                                AccountType = revenueAccount.Value.Type,
                                DebitAmount = 0,
                                CreditAmount = (double)invoice.InvoiceAmount,
                                Description = $"Sales Revenue - Invoice {invoice.Code}"
                            });
                        }
                    }

                    // Credit Tax accounts
                    if (invoice.TaxAmount > 0)
                    {
                        string taxSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {invoice.OrganizationId} 
                                          AND InterfaceType = 'TAX'
                                          AND IsActive = 1
                                          ORDER BY Code";
                        var taxAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(taxSql, transaction: transaction);
                        
                        if (taxAccount != null && taxAccount.Value.Id > 0)
                        {
                            glHeader.Details.Add(new GeneralLedgerDetailModel
                            {
                                AccountId = taxAccount.Value.Id,
                                AccountCode = taxAccount.Value.Code,
                                AccountName = taxAccount.Value.Name,
                                AccountType = taxAccount.Value.Type,
                                DebitAmount = 0,
                                CreditAmount = (double)invoice.TaxAmount,
                                Description = $"Tax - Invoice {invoice.Code}"
                            });
                        }
                    }

                    // Credit Service Charges
                    if (invoice.ChargesAmount > 0)
                    {
                        string chargeSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                             WHERE OrganizationId = {invoice.OrganizationId} 
                                             AND InterfaceType = 'SERVICE'
                                             AND IsActive = 1
                                             ORDER BY Code";
                        var chargeAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(chargeSql, transaction: transaction);
                        
                        if (chargeAccount != null && chargeAccount.Value.Id > 0)
                        {
                            glHeader.Details.Add(new GeneralLedgerDetailModel
                            {
                                AccountId = chargeAccount.Value.Id,
                                AccountCode = chargeAccount.Value.Code,
                                AccountName = chargeAccount.Value.Name,
                                AccountType = chargeAccount.Value.Type,
                                DebitAmount = 0,
                                CreditAmount = (double)invoice.ChargesAmount,
                                Description = $"Service Charges - Invoice {invoice.Code}"
                            });
                        }
                    }

                    // Debit Discount
                    if (invoice.DiscountAmount > 0)
                    {
                        string discountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                               WHERE OrganizationId = {invoice.OrganizationId} 
                                               AND InterfaceType = 'DISCOUNT'
                                               AND IsActive = 1
                                               ORDER BY Code";
                        var discountAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(discountSql, transaction: transaction);
                        
                        if (discountAccount != null && discountAccount.Value.Id > 0)
                        {
                            glHeader.Details.Add(new GeneralLedgerDetailModel
                            {
                                AccountId = discountAccount.Value.Id,
                                AccountCode = discountAccount.Value.Code,
                                AccountName = discountAccount.Value.Name,
                                AccountType = discountAccount.Value.Type,
                                DebitAmount = (double)invoice.DiscountAmount,
                                CreditAmount = 0,
                                Description = $"Discount - Invoice {invoice.Code}"
                            });
                        }
                    }
                }
                else
                {
                    // PURCHASE: Credit AP, Debit Expense/Tax/Charges, Credit Discount
                    // Credit AP for TotalInvoiceAmount
                    glHeader.Details.Add(new GeneralLedgerDetailModel
                    {
                        AccountId = arApAccount.Value.Id,
                        AccountCode = arApAccount.Value.Code,
                        AccountName = arApAccount.Value.Name,
                        AccountType = arApAccount.Value.Type,
                        DebitAmount = 0,
                        CreditAmount = (double)invoice.TotalInvoiceAmount,
                        Description = $"Invoice {invoice.Code}",
                        PartyId = invoice.PartyId
                    });

                    // Debit Expense (from InvoiceAmount)
                    if (invoice.InvoiceAmount > 0)
                    {
                        string expenseSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                              WHERE OrganizationId = {invoice.OrganizationId} 
                                              AND Type = 'EXPENSE'
                                              AND IsActive = 1
                                              ORDER BY Code";
                        var expenseAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(expenseSql, transaction: transaction);
                        
                        if (expenseAccount != null && expenseAccount.Value.Id > 0)
                        {
                            glHeader.Details.Add(new GeneralLedgerDetailModel
                            {
                                AccountId = expenseAccount.Value.Id,
                                AccountCode = expenseAccount.Value.Code,
                                AccountName = expenseAccount.Value.Name,
                                AccountType = expenseAccount.Value.Type,
                                DebitAmount = (double)invoice.InvoiceAmount,
                                CreditAmount = 0,
                                Description = $"Purchase Expense - Invoice {invoice.Code}"
                            });
                        }
                    }

                    // Debit Tax accounts
                    if (invoice.TaxAmount > 0)
                    {
                        string taxSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {invoice.OrganizationId} 
                                          AND InterfaceType = 'TAX'
                                          AND IsActive = 1
                                          ORDER BY Code";
                        var taxAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(taxSql, transaction: transaction);
                        
                        if (taxAccount != null && taxAccount.Value.Id > 0)
                        {
                            glHeader.Details.Add(new GeneralLedgerDetailModel
                            {
                                AccountId = taxAccount.Value.Id,
                                AccountCode = taxAccount.Value.Code,
                                AccountName = taxAccount.Value.Name,
                                AccountType = taxAccount.Value.Type,
                                DebitAmount = (double)invoice.TaxAmount,
                                CreditAmount = 0,
                                Description = $"Tax - Invoice {invoice.Code}"
                            });
                        }
                    }

                    // Debit Service Charges
                    if (invoice.ChargesAmount > 0)
                    {
                        string chargeSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                             WHERE OrganizationId = {invoice.OrganizationId} 
                                             AND InterfaceType = 'SERVICE'
                                             AND IsActive = 1
                                             ORDER BY Code";
                        var chargeAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(chargeSql, transaction: transaction);
                        
                        if (chargeAccount != null && chargeAccount.Value.Id > 0)
                        {
                            glHeader.Details.Add(new GeneralLedgerDetailModel
                            {
                                AccountId = chargeAccount.Value.Id,
                                AccountCode = chargeAccount.Value.Code,
                                AccountName = chargeAccount.Value.Name,
                                AccountType = chargeAccount.Value.Type,
                                DebitAmount = (double)invoice.ChargesAmount,
                                CreditAmount = 0,
                                Description = $"Service Charges - Invoice {invoice.Code}"
                            });
                        }
                    }

                    // Credit Discount
                    if (invoice.DiscountAmount > 0)
                    {
                        string discountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                               WHERE OrganizationId = {invoice.OrganizationId} 
                                               AND InterfaceType = 'DISCOUNT'
                                               AND IsActive = 1
                                               ORDER BY Code";
                        var discountAccount = await connection.QueryFirstOrDefaultAsync<(int Id, string Code, string Name, string Type)?>(discountSql, transaction: transaction);
                        
                        if (discountAccount != null && discountAccount.Value.Id > 0)
                        {
                            glHeader.Details.Add(new GeneralLedgerDetailModel
                            {
                                AccountId = discountAccount.Value.Id,
                                AccountCode = discountAccount.Value.Code,
                                AccountName = discountAccount.Value.Name,
                                AccountType = discountAccount.Value.Type,
                                DebitAmount = 0,
                                CreditAmount = (double)invoice.DiscountAmount,
                                Description = $"Discount - Invoice {invoice.Code}"
                            });
                        }
                    }
                }

                // Calculate totals
                glHeader.TotalDebit = glHeader.Details.Sum(d => d.DebitAmount);
                glHeader.TotalCredit = glHeader.Details.Sum(d => d.CreditAmount);

                // Validate balance
                if (Math.Abs(glHeader.TotalDebit - glHeader.TotalCredit) > 0.01)
                {
                    transaction.Rollback();
                    return (false, null!, $"Entry is not balanced. Total Debit: {glHeader.TotalDebit:N2}, Total Credit: {glHeader.TotalCredit:N2}");
                }

                // Insert GL Header
                string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{entryNo.ToUpper()}';";
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
                        '{glHeader.Source?.ToUpper().Replace("'", "''") ?? "INVOICE"}', '{glHeader.Description!.ToUpper().Replace("'", "''")}',
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

                // Validate all account IDs before inserting details
                var invalidDetails = glHeader.Details.Where(d => d.AccountId == 0).ToList();
                if (invalidDetails.Any())
                {
                    transaction.Rollback();
                    var invalidDescriptions = string.Join(", ", invalidDetails.Select(d => d.Description ?? "Unknown"));
                    return (false, null!, $"Invalid Account ID found in GL entries. One or more accounts have AccountId = 0. Details: {invalidDescriptions}. Please ensure all accounts are properly configured in Chart of Accounts.");
                }

                // Insert Details
                int seqNo = 1;
                foreach (var detail in glHeader.Details)
                {
                    // Double-check AccountId before inserting (defensive programming)
                    if (detail.AccountId == 0)
                    {
                        transaction.Rollback();
                        return (false, null!, $"Invalid Account ID (0) found in GL detail entry: {detail.Description ?? "Unknown"}. AccountId must be a valid account from Chart of Accounts.");
                    }

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

                // Update invoice with GL posting info
                string SQLUpdateInvoice = $@"UPDATE Invoice SET 
                    IsPostedToGL = 1,
                    PostedToGLDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    PostedToGLBy = {userId},
                    GLEntryNo = '{entryNo}'
                WHERE Id = {invoiceId}";

                await connection.ExecuteAsync(SQLUpdateInvoice, transaction: transaction);
                transaction.Commit();

                // Get the created GL entry
                var result = await Get(headerId);
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
            Log.Error(ex, "GeneralLedgerService CreateGLFromInvoice Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromCashBook(int cashBookId, int userId, string clientInfo)
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
                var periodCheck = await ValidatePeriod(cashBook.OrganizationId, cashBook.TranDate ?? DateTime.Now, "CASHBOOK");
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

                // Create GL entries based on transaction type
                if (cashBook.TranType?.ToUpper() == "RECEIPT")
                {
                    // Debit cash account, Credit the specified account
                    glHeader.Details.Add(new GeneralLedgerDetailModel
                    {
                        AccountId = cashBook.AccountId, // Cash account
                        DebitAmount = cashBook.Amount,
                        CreditAmount = 0,
                        Description = cashBook.Description,
                        PartyId = cashBook.PartyId
                    });
                    // Credit side - you'll need to determine based on your business logic
                    // This is a placeholder
                }
                else if (cashBook.TranType?.ToUpper() == "PAYMENT")
                {
                    // Credit cash account, Debit the specified account
                    glHeader.Details.Add(new GeneralLedgerDetailModel
                    {
                        AccountId = cashBook.AccountId, // Expense/Other account
                        DebitAmount = cashBook.Amount,
                        CreditAmount = 0,
                        Description = cashBook.Description,
                        PartyId = cashBook.PartyId
                    });
                    // Credit side - you'll need to determine based on your business logic
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
                string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{entryNo.ToUpper()}';";
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
                var result = await Get(headerId);
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
            Log.Error(ex, "GeneralLedgerService CreateGLFromCashBook Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel?)> PreviewGLFromInvoice(InvoicesModel invoice)
    {
        try
        {
            if (invoice == null || invoice.Invoice == null)
            {
                Log.Warning("PreviewGLFromInvoice: Invoice model is null");
                return (false, null);
            }

            DapperFunctions dapper = new DapperFunctions();
            
            // If invoice is already posted, return the actual GL entry
            if (invoice.Invoice.Id > 0 && invoice.Invoice.IsPostedToGL == 1 && !string.IsNullOrWhiteSpace(invoice.Invoice.GLEntryNo))
            {
                // Get the actual posted GL entry by EntryNo
                string sqlPostedEntry = $@"SELECT glh.*, p.Name as PartyName, loc.Name as LocationName
                                          FROM GeneralLedgerHeader as glh
                                          LEFT JOIN Parties as p on p.Id=glh.PartyId
                                          LEFT JOIN Locations as loc on loc.Id=glh.LocationId
                                          WHERE glh.EntryNo = '{invoice.Invoice.GLEntryNo}' AND glh.IsSoftDeleted=0";
                
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
                    
                    return (true, postedHeader);
                }
            }
            
            // Create preview GL header (not saved)
            GeneralLedgerHeaderModel glHeader = new GeneralLedgerHeaderModel
            {
                OrganizationId = invoice.Invoice.OrganizationId,
                EntryNo = "PREVIEW",
                EntryDate = invoice.Invoice.TranDate ?? DateTime.Now,
                Source = "INVOICE",
                Description = $"Invoice {invoice.Invoice.Code ?? "NEW"}",
                ReferenceNo = invoice.Invoice.Code ?? "NEW",
                ReferenceType = "INVOICE",
                ReferenceId = invoice.Invoice.Id,
                PartyId = invoice.Invoice.PartyId,
                LocationId = invoice.Invoice.LocationId,
                BaseCurrencyId = invoice.Invoice.BaseCurrencyId,
                EnteredCurrencyId = invoice.Invoice.EnteredCurrencyId,
                Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
            };

            bool isSale = invoice.Invoice.InvoiceType?.ToUpper() == "SALE";
            
            // Get AR/AP account - use AccountId from invoice if available, otherwise lookup by code
            ChartOfAccountsModel arApAccount = null;
            
            if (invoice.Invoice.AccountId > 0)
            {
                // Use the account specified in the invoice
                string sqlAccountById = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                           WHERE Id = {invoice.Invoice.AccountId} 
                                           AND OrganizationId = {invoice.Invoice.OrganizationId} 
                                           AND IsActive = 1";
                var accountResultById = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlAccountById);
                arApAccount = accountResultById?.FirstOrDefault();
                
                if (arApAccount == null || arApAccount.Id == 0)
                {
                    string errorMsg = $"Account ID {invoice.Invoice.AccountId} specified in invoice is not valid or inactive. Please select a valid account.";
                    Log.Warning($"PreviewGLFromInvoice: {errorMsg}");
                    return (false, null);
                }
            }
            else
            {
                // Fallback to AR/AP lookup if AccountId not specified
                string accountCode = isSale ? "AR" : "AP";
                string sqlAccount = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                      WHERE OrganizationId = {invoice.Invoice.OrganizationId} 
                                      AND (Code = '{accountCode}' OR Code LIKE '{accountCode}%')
                                      AND IsActive = 1
                                      ORDER BY Code";
                var accountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlAccount);
                arApAccount = accountResult?.FirstOrDefault();
                
                if (arApAccount == null || arApAccount.Id == 0)
                {
                    string errorMsg = $"{(isSale ? "Accounts Receivable (AR)" : "Accounts Payable (AP)")} account not found for OrganizationId {invoice.Invoice.OrganizationId}. Please configure the account in Chart of Accounts or specify an AccountId in the invoice.";
                    Log.Warning($"PreviewGLFromInvoice: {errorMsg}");
                    return (false, null);
                }
            }

            // For SALE: Debit AR, Credit Revenue/Tax/Charges, Debit Discount
            // For PURCHASE: Credit AP, Debit Expense/Tax/Charges, Credit Discount
            if (isSale)
            {
                // Debit AR
                glHeader.Details.Add(new GeneralLedgerDetailModel
                {
                    AccountId = arApAccount.Id,
                    AccountCode = arApAccount.Code,
                    AccountName = arApAccount.Name,
                    AccountType = arApAccount.Type,
                    DebitAmount = (double)invoice.Invoice.TotalInvoiceAmount,
                    CreditAmount = 0,
                    Description = $"Invoice {invoice.Invoice.Code ?? "NEW"}",
                    PartyId = invoice.Invoice.PartyId
                });

                // Credit Revenue (from items) - simplified: use total invoice amount minus tax, charges, discount
                decimal revenueAmount = invoice.Invoice.InvoiceAmount;
                if (revenueAmount > 0)
                {
                    string revenueSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {invoice.Invoice.OrganizationId} 
                                          AND Type = 'REVENUE'
                                          AND IsActive = 1
                                          ORDER BY Code";
                    var revenueAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(revenueSql);
                    var revenueAccount = revenueAccountResult?.FirstOrDefault();
                    
                    if (revenueAccount != null && revenueAccount.Id > 0)
                    {
                        glHeader.Details.Add(new GeneralLedgerDetailModel
                        {
                            AccountId = revenueAccount.Id,
                            AccountCode = revenueAccount.Code,
                            AccountName = revenueAccount.Name,
                            AccountType = revenueAccount.Type,
                            DebitAmount = 0,
                            CreditAmount = (double)revenueAmount,
                            Description = $"Sales Revenue - Invoice {invoice.Invoice.Code ?? "NEW"}"
                        });
                    }
                }

                // Credit Tax accounts
                if (invoice.Invoice.TaxAmount > 0)
                {
                    string taxSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                      WHERE OrganizationId = {invoice.Invoice.OrganizationId} 
                                      AND InterfaceType = 'TAX'
                                      AND IsActive = 1
                                      ORDER BY Code";
                    var taxAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(taxSql);
                    var taxAccount = taxAccountResult?.FirstOrDefault();
                    
                    if (taxAccount != null && taxAccount.Id > 0)
                    {
                        glHeader.Details.Add(new GeneralLedgerDetailModel
                        {
                            AccountId = taxAccount.Id,
                            AccountCode = taxAccount.Code,
                            AccountName = taxAccount.Name,
                            AccountType = taxAccount.Type,
                            DebitAmount = 0,
                            CreditAmount = (double)invoice.Invoice.TaxAmount,
                            Description = $"Tax - Invoice {invoice.Invoice.Code ?? "NEW"}"
                        });
                    }
                }

                // Credit Service Charges
                if (invoice.Invoice.ChargesAmount > 0)
                {
                    string chargeSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                         WHERE OrganizationId = {invoice.Invoice.OrganizationId} 
                                         AND InterfaceType = 'SERVICE'
                                         AND IsActive = 1
                                         ORDER BY Code";
                    var chargeAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(chargeSql);
                    var chargeAccount = chargeAccountResult?.FirstOrDefault();
                    
                    if (chargeAccount != null && chargeAccount.Id > 0)
                    {
                        glHeader.Details.Add(new GeneralLedgerDetailModel
                        {
                            AccountId = chargeAccount.Id,
                            AccountCode = chargeAccount.Code,
                            AccountName = chargeAccount.Name,
                            AccountType = chargeAccount.Type,
                            DebitAmount = 0,
                            CreditAmount = (double)invoice.Invoice.ChargesAmount,
                            Description = $"Service Charges - Invoice {invoice.Invoice.Code ?? "NEW"}"
                        });
                    }
                }

                // Debit Discount
                if (invoice.Invoice.DiscountAmount > 0)
                {
                    string discountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                           WHERE OrganizationId = {invoice.Invoice.OrganizationId} 
                                           AND InterfaceType = 'DISCOUNT'
                                           AND IsActive = 1
                                           ORDER BY Code";
                    var discountAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(discountSql);
                    var discountAccount = discountAccountResult?.FirstOrDefault();
                    
                    if (discountAccount != null && discountAccount.Id > 0)
                    {
                        glHeader.Details.Add(new GeneralLedgerDetailModel
                        {
                            AccountId = discountAccount.Id,
                            AccountCode = discountAccount.Code,
                            AccountName = discountAccount.Name,
                            AccountType = discountAccount.Type,
                            DebitAmount = (double)invoice.Invoice.DiscountAmount,
                            CreditAmount = 0,
                            Description = $"Discount - Invoice {invoice.Invoice.Code ?? "NEW"}"
                        });
                    }
                }
            }
            else
            {
                // PURCHASE: Credit AP, Debit Expense/Tax/Charges, Credit Discount
                // Credit AP
                glHeader.Details.Add(new GeneralLedgerDetailModel
                {
                    AccountId = arApAccount.Id,
                    AccountCode = arApAccount.Code,
                    AccountName = arApAccount.Name,
                    AccountType = arApAccount.Type,
                    DebitAmount = 0,
                    CreditAmount = (double)invoice.Invoice.TotalInvoiceAmount,
                    Description = $"Invoice {invoice.Invoice.Code ?? "NEW"}",
                    PartyId = invoice.Invoice.PartyId
                });

                // Debit Expense
                decimal expenseAmount = invoice.Invoice.InvoiceAmount;
                if (expenseAmount > 0)
                {
                    string expenseSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                          WHERE OrganizationId = {invoice.Invoice.OrganizationId} 
                                          AND Type = 'EXPENSE'
                                          AND IsActive = 1
                                          ORDER BY Code";
                    var expenseAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(expenseSql);
                    var expenseAccount = expenseAccountResult?.FirstOrDefault();
                    
                    if (expenseAccount != null && expenseAccount.Id > 0)
                    {
                        glHeader.Details.Add(new GeneralLedgerDetailModel
                        {
                            AccountId = expenseAccount.Id,
                            AccountCode = expenseAccount.Code,
                            AccountName = expenseAccount.Name,
                            AccountType = expenseAccount.Type,
                            DebitAmount = (double)expenseAmount,
                            CreditAmount = 0,
                            Description = $"Purchase Expense - Invoice {invoice.Invoice.Code ?? "NEW"}"
                        });
                    }
                }

                // Debit Tax
                if (invoice.Invoice.TaxAmount > 0)
                {
                    string taxSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                      WHERE OrganizationId = {invoice.Invoice.OrganizationId} 
                                      AND InterfaceType = 'TAX'
                                      AND IsActive = 1
                                      ORDER BY Code";
                    var taxAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(taxSql);
                    var taxAccount = taxAccountResult?.FirstOrDefault();
                    
                    if (taxAccount != null && taxAccount.Id > 0)
                    {
                        glHeader.Details.Add(new GeneralLedgerDetailModel
                        {
                            AccountId = taxAccount.Id,
                            AccountCode = taxAccount.Code,
                            AccountName = taxAccount.Name,
                            AccountType = taxAccount.Type,
                            DebitAmount = (double)invoice.Invoice.TaxAmount,
                            CreditAmount = 0,
                            Description = $"Tax - Invoice {invoice.Invoice.Code ?? "NEW"}"
                        });
                    }
                }

                // Debit Service Charges
                if (invoice.Invoice.ChargesAmount > 0)
                {
                    string chargeSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                         WHERE OrganizationId = {invoice.Invoice.OrganizationId} 
                                         AND InterfaceType = 'SERVICE'
                                         AND IsActive = 1
                                         ORDER BY Code";
                    var chargeAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(chargeSql);
                    var chargeAccount = chargeAccountResult?.FirstOrDefault();
                    
                    if (chargeAccount != null && chargeAccount.Id > 0)
                    {
                        glHeader.Details.Add(new GeneralLedgerDetailModel
                        {
                            AccountId = chargeAccount.Id,
                            AccountCode = chargeAccount.Code,
                            AccountName = chargeAccount.Name,
                            AccountType = chargeAccount.Type,
                            DebitAmount = (double)invoice.Invoice.ChargesAmount,
                            CreditAmount = 0,
                            Description = $"Service Charges - Invoice {invoice.Invoice.Code ?? "NEW"}"
                        });
                    }
                }

                // Credit Discount
                if (invoice.Invoice.DiscountAmount > 0)
                {
                    string discountSql = $@"SELECT TOP 1 Id, Code, Name, Type FROM ChartOfAccounts 
                                         WHERE OrganizationId = {invoice.Invoice.OrganizationId} 
                                         AND InterfaceType = 'DISCOUNT'
                                         AND IsActive = 1
                                         ORDER BY Code";
                    var discountAccountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(discountSql);
                    var discountAccount = discountAccountResult?.FirstOrDefault();
                    
                    if (discountAccount != null && discountAccount.Id > 0)
                    {
                        glHeader.Details.Add(new GeneralLedgerDetailModel
                        {
                            AccountId = discountAccount.Id,
                            AccountCode = discountAccount.Code,
                            AccountName = discountAccount.Name,
                            AccountType = discountAccount.Type,
                            DebitAmount = 0,
                            CreditAmount = (double)invoice.Invoice.DiscountAmount,
                            Description = $"Discount - Invoice {invoice.Invoice.Code ?? "NEW"}"
                        });
                    }
                }
            }

            // Calculate totals
            glHeader.TotalDebit = glHeader.Details.Sum(d => d.DebitAmount);
            glHeader.TotalCredit = glHeader.Details.Sum(d => d.CreditAmount);

            // Validate that we have at least one entry
            if (glHeader.Details == null || glHeader.Details.Count == 0)
            {
                Log.Warning("PreviewGLFromInvoice: No GL details generated");
                return (false, null);
            }

            return (true, glHeader);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GeneralLedgerService PreviewGLFromInvoice Error: {Message}", ex.Message);
            return (false, null);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel?)> PreviewGLFromCashBook(CashBookModel cashBook)
    {
        try
        {
            if (cashBook == null || cashBook.Id == 0)
            {
                Log.Warning("PreviewGLFromCashBook: CashBook model is null or invalid");
                return (false, null);
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
                    
                    return (true, postedHeader);
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
                Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
            };

            // Get account details for cashbook account
            string sqlAccount = $@"SELECT coa.* FROM ChartOfAccounts coa WHERE coa.Id = {cashBook.AccountId} AND coa.IsActive = 1";
            var accountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlAccount);
            var account = accountResult?.FirstOrDefault();

            if (account == null || account.Id == 0)
            {
                Log.Warning($"PreviewGLFromCashBook: Account {cashBook.AccountId} not found");
                return (false, null);
            }

            // Create GL entries based on transaction type
            if (cashBook.TranType?.ToUpper() == "RECEIPT")
            {
                // Debit cash account
                glHeader.Details.Add(new GeneralLedgerDetailModel
                {
                    AccountId = cashBook.AccountId,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    AccountType = account.Type,
                    DebitAmount = cashBook.Amount,
                    CreditAmount = 0,
                    Description = cashBook.Description ?? $"Receipt {cashBook.SeqNo}",
                    PartyId = cashBook.PartyId
                });
                
                // Credit side - need to determine based on business logic
                // For now, we'll add a placeholder that needs to be configured
                // In a real scenario, this might be a revenue account or another account
                Log.Warning("PreviewGLFromCashBook: RECEIPT transaction requires credit account configuration");
            }
            else if (cashBook.TranType?.ToUpper() == "PAYMENT")
            {
                // Debit expense/other account
                glHeader.Details.Add(new GeneralLedgerDetailModel
                {
                    AccountId = cashBook.AccountId,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    AccountType = account.Type,
                    DebitAmount = cashBook.Amount,
                    CreditAmount = 0,
                    Description = cashBook.Description ?? $"Payment {cashBook.SeqNo}",
                    PartyId = cashBook.PartyId
                });
                
                // Credit side - need to determine based on business logic
                // For now, we'll add a placeholder that needs to be configured
                // In a real scenario, this might be a cash account
                Log.Warning("PreviewGLFromCashBook: PAYMENT transaction requires credit account configuration");
            }

            // Calculate totals
            glHeader.TotalDebit = glHeader.Details.Sum(d => d.DebitAmount);
            glHeader.TotalCredit = glHeader.Details.Sum(d => d.CreditAmount);

            if (glHeader.Details.Count == 0)
            {
                Log.Warning("PreviewGLFromCashBook: No GL details generated");
                return (false, null);
            }

            return (true, glHeader);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GeneralLedgerService PreviewGLFromCashBook Error: {Message}", ex.Message);
            return (false, null);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel?)> PreviewGLFromPettyCash(PettyCashModel pettyCash)
    {
        try
        {
            if (pettyCash == null || pettyCash.Id == 0)
            {
                Log.Warning("PreviewGLFromPettyCash: PettyCash model is null or invalid");
                return (false, null);
            }

            DapperFunctions dapper = new DapperFunctions();
            
            // If petty cash is already posted, return the actual GL entry
            if (pettyCash.Id > 0 && pettyCash.IsPostedToGL == 1 && !string.IsNullOrWhiteSpace(pettyCash.GLEntryNo))
            {
                // Get the actual posted GL entry by EntryNo
                string sqlPostedEntry = $@"SELECT glh.*, p.Name as PartyName, loc.Name as LocationName
                                          FROM GeneralLedgerHeader as glh
                                          LEFT JOIN Parties as p on p.Id=glh.PartyId
                                          LEFT JOIN Locations as loc on loc.Id=glh.LocationId
                                          WHERE glh.EntryNo = '{pettyCash.GLEntryNo}' AND glh.IsSoftDeleted=0";
                
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
                    
                    return (true, postedHeader);
                }
            }
            
            // Create preview GL header (not saved)
            GeneralLedgerHeaderModel glHeader = new GeneralLedgerHeaderModel
            {
                OrganizationId = pettyCash.OrganizationId,
                EntryNo = "PREVIEW",
                EntryDate = pettyCash.TranDate ?? DateTime.Now,
                Source = "PETTYCASH",
                Description = $"Petty Cash {pettyCash.SeqNo}",
                ReferenceNo = pettyCash.SeqNo,
                ReferenceType = "PETTYCASH",
                ReferenceId = pettyCash.Id,
                PartyId = pettyCash.PartyId,
                LocationId = pettyCash.LocationId,
                BaseCurrencyId = pettyCash.BaseCurrencyId,
                EnteredCurrencyId = pettyCash.EnteredCurrencyId,
                Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
            };

            // Get account details for petty cash account
            string sqlAccount = $@"SELECT coa.* FROM ChartOfAccounts coa WHERE coa.Id = {pettyCash.AccountId} AND coa.IsActive = 1";
            var accountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlAccount);
            var account = accountResult?.FirstOrDefault();

            if (account == null || account.Id == 0)
            {
                Log.Warning($"PreviewGLFromPettyCash: Account {pettyCash.AccountId} not found");
                return (false, null);
            }

            // Create GL entries based on transaction type (similar to CashBook)
            if (pettyCash.TranType?.ToUpper() == "RECEIPT")
            {
                // Debit petty cash account
                glHeader.Details.Add(new GeneralLedgerDetailModel
                {
                    AccountId = pettyCash.AccountId,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    AccountType = account.Type,
                    DebitAmount = pettyCash.Amount,
                    CreditAmount = 0,
                    Description = pettyCash.Description ?? $"Receipt {pettyCash.SeqNo}",
                    PartyId = pettyCash.PartyId
                });
            }
            else if (pettyCash.TranType?.ToUpper() == "PAYMENT")
            {
                // Debit expense/other account, Credit petty cash account
                glHeader.Details.Add(new GeneralLedgerDetailModel
                {
                    AccountId = pettyCash.AccountId,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    AccountType = account.Type,
                    DebitAmount = pettyCash.Amount,
                    CreditAmount = 0,
                    Description = pettyCash.Description ?? $"Payment {pettyCash.SeqNo}",
                    PartyId = pettyCash.PartyId
                });
            }

            // Calculate totals
            glHeader.TotalDebit = glHeader.Details.Sum(d => d.DebitAmount);
            glHeader.TotalCredit = glHeader.Details.Sum(d => d.CreditAmount);

            if (glHeader.Details.Count == 0)
            {
                Log.Warning("PreviewGLFromPettyCash: No GL details generated");
                return (false, null);
            }

            return (true, glHeader);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GeneralLedgerService PreviewGLFromPettyCash Error: {Message}", ex.Message);
            return (false, null);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel?)> PreviewGLFromAdvances(AdvancesModel advances)
    {
        try
        {
            if (advances == null || advances.Id == 0)
            {
                Log.Warning("PreviewGLFromAdvances: Advances model is null or invalid");
                return (false, null);
            }

            DapperFunctions dapper = new DapperFunctions();
            
            // If advance is already posted, return the actual GL entry
            if (advances.Id > 0 && advances.IsPostedToGL == 1 && !string.IsNullOrWhiteSpace(advances.GLEntryNo))
            {
                // Get the actual posted GL entry by EntryNo
                string sqlPostedEntry = $@"SELECT glh.*, p.Name as PartyName, loc.Name as LocationName
                                          FROM GeneralLedgerHeader as glh
                                          LEFT JOIN Parties as p on p.Id=glh.PartyId
                                          LEFT JOIN Locations as loc on loc.Id=glh.LocationId
                                          WHERE glh.EntryNo = '{advances.GLEntryNo}' AND glh.IsSoftDeleted=0";
                
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
                    
                    return (true, postedHeader);
                }
            }
            
            // Create preview GL header (not saved)
            GeneralLedgerHeaderModel glHeader = new GeneralLedgerHeaderModel
            {
                OrganizationId = advances.OrganizationId,
                EntryNo = "PREVIEW",
                EntryDate = advances.TranDate ?? DateTime.Now,
                Source = "ADVANCES",
                Description = $"Advance {advances.SeqNo}",
                ReferenceNo = advances.SeqNo,
                ReferenceType = "ADVANCES",
                ReferenceId = advances.Id,
                PartyId = advances.PartyId,
                LocationId = advances.LocationId,
                BaseCurrencyId = advances.BaseCurrencyId,
                EnteredCurrencyId = advances.EnteredCurrencyId,
                Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
            };

            // Get account details for advance account
            string sqlAccount = $@"SELECT coa.* FROM ChartOfAccounts coa WHERE coa.Id = {advances.AccountId} AND coa.IsActive = 1";
            var accountResult = await dapper.SearchByQuery<ChartOfAccountsModel>(sqlAccount);
            var account = accountResult?.FirstOrDefault();

            if (account == null || account.Id == 0)
            {
                Log.Warning($"PreviewGLFromAdvances: Account {advances.AccountId} not found");
                return (false, null);
            }

            // Create GL entries based on transaction type (similar to CashBook)
            if (advances.TranType?.ToUpper() == "RECEIPT")
            {
                // Debit advance account
                glHeader.Details.Add(new GeneralLedgerDetailModel
                {
                    AccountId = advances.AccountId,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    AccountType = account.Type,
                    DebitAmount = advances.Amount,
                    CreditAmount = 0,
                    Description = advances.Description ?? $"Receipt {advances.SeqNo}",
                    PartyId = advances.PartyId
                });
            }
            else if (advances.TranType?.ToUpper() == "PAYMENT")
            {
                // Debit expense/other account, Credit advance account
                glHeader.Details.Add(new GeneralLedgerDetailModel
                {
                    AccountId = advances.AccountId,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    AccountType = account.Type,
                    DebitAmount = advances.Amount,
                    CreditAmount = 0,
                    Description = advances.Description ?? $"Payment {advances.SeqNo}",
                    PartyId = advances.PartyId
                });
            }

            // Calculate totals
            glHeader.TotalDebit = glHeader.Details.Sum(d => d.DebitAmount);
            glHeader.TotalCredit = glHeader.Details.Sum(d => d.CreditAmount);

            if (glHeader.Details.Count == 0)
            {
                Log.Warning("PreviewGLFromAdvances: No GL details generated");
                return (false, null);
            }

            return (true, glHeader);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GeneralLedgerService PreviewGLFromAdvances Error: {Message}", ex.Message);
            return (false, null);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromPettyCash(int pettyCashId, int userId, string clientInfo)
    {
        try
        {
            using var connection = new SqlConnection(new Config().DefaultConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get petty cash data
                string SQLPettyCash = $@"SELECT * FROM PettyCash WHERE Id = {pettyCashId} AND IsSoftDeleted = 0";
                var pettyCash = (await connection.QueryAsync<PettyCashModel>(SQLPettyCash, transaction: transaction))?.FirstOrDefault();
                
                if (pettyCash == null || pettyCash.Id == 0)
                {
                    transaction.Rollback();
                    return (false, null!, "Petty cash entry not found.");
                }

                // Check if already posted
                if (pettyCash.IsPostedToGL == 1)
                {
                    transaction.Rollback();
                    return (false, null!, "Petty cash entry is already posted to General Ledger.");
                }

                // Validate period for PETTYCASH module
                var periodCheck = await ValidatePeriod(pettyCash.OrganizationId, pettyCash.TranDate ?? DateTime.Now, "PETTYCASH");
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
                    Description = $"Petty Cash {pettyCash.SeqNo}",
                    ReferenceNo = pettyCash.SeqNo,
                    ReferenceType = "PETTYCASH",
                    ReferenceId = pettyCash.Id,
                    PartyId = pettyCash.PartyId,
                    LocationId = pettyCash.LocationId,
                    BaseCurrencyId = pettyCash.BaseCurrencyId,
                    EnteredCurrencyId = pettyCash.EnteredCurrencyId,
                    CreatedBy = userId,
                    CreatedFrom = clientInfo,
                    Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
                };

                // Create GL entries based on transaction type (same pattern as CashBook)
                if (pettyCash.TranType?.ToUpper() == "RECEIPT")
                {
                    glHeader.Details.Add(new GeneralLedgerDetailModel
                    {
                        AccountId = pettyCash.AccountId,
                        DebitAmount = pettyCash.Amount,
                        CreditAmount = 0,
                        Description = pettyCash.Description,
                        PartyId = pettyCash.PartyId
                    });
                }
                else if (pettyCash.TranType?.ToUpper() == "PAYMENT")
                {
                    glHeader.Details.Add(new GeneralLedgerDetailModel
                    {
                        AccountId = pettyCash.AccountId,
                        DebitAmount = pettyCash.Amount,
                        CreditAmount = 0,
                        Description = pettyCash.Description,
                        PartyId = pettyCash.PartyId
                    });
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
                string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{entryNo.ToUpper()}';";
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

                // Validate all account IDs before inserting details
                var invalidDetails = glHeader.Details.Where(d => d.AccountId == 0).ToList();
                if (invalidDetails.Any())
                {
                    transaction.Rollback();
                    var invalidDescriptions = string.Join(", ", invalidDetails.Select(d => d.Description ?? "Unknown"));
                    return (false, null!, $"Invalid Account ID found in GL entries. One or more accounts have AccountId = 0. Details: {invalidDescriptions}. Please ensure all accounts are properly configured in Chart of Accounts.");
                }

                // Insert Details
                int seqNo = 1;
                foreach (var detail in glHeader.Details)
                {
                    if (detail.AccountId == 0)
                    {
                        transaction.Rollback();
                        return (false, null!, $"Invalid Account ID (0) found in GL detail entry: {detail.Description ?? "Unknown"}. AccountId must be a valid account from Chart of Accounts.");
                    }

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

                // Update petty cash with GL posting info
                string SQLUpdatePettyCash = $@"UPDATE PettyCash SET 
                    IsPostedToGL = 1,
                    PostedToGLDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    PostedToGLBy = {userId},
                    GLEntryNo = '{entryNo}'
                WHERE Id = {pettyCashId}";

                await connection.ExecuteAsync(SQLUpdatePettyCash, transaction: transaction);
                transaction.Commit();

                // Get the created GL entry
                var result = await Get(headerId);
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
            Log.Error(ex, "GeneralLedgerService CreateGLFromPettyCash Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromAdvances(int advancesId, int userId, string clientInfo)
    {
        try
        {
            using var connection = new SqlConnection(new Config().DefaultConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get advances data
                string SQLAdvances = $@"SELECT * FROM Advances WHERE Id = {advancesId} AND IsSoftDeleted = 0";
                var advances = (await connection.QueryAsync<AdvancesModel>(SQLAdvances, transaction: transaction))?.FirstOrDefault();
                
                if (advances == null || advances.Id == 0)
                {
                    transaction.Rollback();
                    return (false, null!, "Advances entry not found.");
                }

                // Check if already posted
                if (advances.IsPostedToGL == 1)
                {
                    transaction.Rollback();
                    return (false, null!, "Advances entry is already posted to General Ledger.");
                }

                // Validate period for ADVANCES module
                var periodCheck = await ValidatePeriod(advances.OrganizationId, advances.TranDate ?? DateTime.Now, "ADVANCES");
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
                    OrganizationId = advances.OrganizationId,
                    EntryNo = entryNo,
                    EntryDate = advances.TranDate ?? DateTime.Now,
                    Source = "ADVANCES",
                    Description = $"Advance {advances.SeqNo}",
                    ReferenceNo = advances.SeqNo,
                    ReferenceType = "ADVANCES",
                    ReferenceId = advances.Id,
                    PartyId = advances.PartyId,
                    LocationId = advances.LocationId,
                    BaseCurrencyId = advances.BaseCurrencyId,
                    EnteredCurrencyId = advances.EnteredCurrencyId,
                    CreatedBy = userId,
                    CreatedFrom = clientInfo,
                    Details = new System.Collections.ObjectModel.ObservableCollection<GeneralLedgerDetailModel>()
                };

                // Create GL entries based on transaction type (same pattern as CashBook)
                if (advances.TranType?.ToUpper() == "RECEIPT")
                {
                    glHeader.Details.Add(new GeneralLedgerDetailModel
                    {
                        AccountId = advances.AccountId,
                        DebitAmount = advances.Amount,
                        CreditAmount = 0,
                        Description = advances.Description,
                        PartyId = advances.PartyId
                    });
                }
                else if (advances.TranType?.ToUpper() == "PAYMENT")
                {
                    glHeader.Details.Add(new GeneralLedgerDetailModel
                    {
                        AccountId = advances.AccountId,
                        DebitAmount = advances.Amount,
                        CreditAmount = 0,
                        Description = advances.Description,
                        PartyId = advances.PartyId
                    });
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
                string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{entryNo.ToUpper()}';";
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
                        '{glHeader.Source?.ToUpper().Replace("'", "''") ?? "ADVANCES"}', '{glHeader.Description!.ToUpper().Replace("'", "''")}',
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

                // Validate all account IDs before inserting details
                var invalidDetails = glHeader.Details.Where(d => d.AccountId == 0).ToList();
                if (invalidDetails.Any())
                {
                    transaction.Rollback();
                    var invalidDescriptions = string.Join(", ", invalidDetails.Select(d => d.Description ?? "Unknown"));
                    return (false, null!, $"Invalid Account ID found in GL entries. One or more accounts have AccountId = 0. Details: {invalidDescriptions}. Please ensure all accounts are properly configured in Chart of Accounts.");
                }

                // Insert Details
                int seqNo = 1;
                foreach (var detail in glHeader.Details)
                {
                    if (detail.AccountId == 0)
                    {
                        transaction.Rollback();
                        return (false, null!, $"Invalid Account ID (0) found in GL detail entry: {detail.Description ?? "Unknown"}. AccountId must be a valid account from Chart of Accounts.");
                    }

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

                // Update advances with GL posting info
                string SQLUpdateAdvances = $@"UPDATE Advances SET 
                    IsPostedToGL = 1,
                    PostedToGLDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    PostedToGLBy = {userId},
                    GLEntryNo = '{entryNo}'
                WHERE Id = {advancesId}";

                await connection.ExecuteAsync(SQLUpdateAdvances, transaction: transaction);
                transaction.Commit();

                // Get the created GL entry
                var result = await Get(headerId);
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
            Log.Error(ex, "GeneralLedgerService CreateGLFromAdvances Error");
            return (false, null!, ex.Message);
        }
    }
}

using Dapper;
using Microsoft.Data.SqlClient;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Services.GeneralLedger;
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
    Task<(bool, GeneralLedgerHeaderModel?, string)> PreviewGLFromInvoice(InvoicesModel invoice);
    Task<(bool, GeneralLedgerHeaderModel?, string)> PreviewGLFromCashBook(CashBookModel cashBook);
    Task<(bool, GeneralLedgerHeaderModel?, string)> PreviewGLFromPettyCash(PettyCashModel pettyCash);
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

            string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{entryNo.ToUpper()}' AND IsSoftDeleted = 0;";
            
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

            string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{obj.EntryNo!.ToUpper()}' AND Id != {obj.Id} AND IsSoftDeleted = 0;";
            
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
					ExchangeRate = {(obj.ExchangeRate > 0 ? obj.ExchangeRate.ToString(System.Globalization.CultureInfo.InvariantCulture) : "1.000000")},
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
        return await CreateGLFromInvoiceHelper.CreateGLFromInvoice(this, invoiceId, userId, clientInfo);
    }

    public async Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromCashBook(int cashBookId, int userId, string clientInfo)
    {
        return await CreateGLFromCashBookHelper.CreateGLFromCashBook(this, cashBookId, userId, clientInfo);
    }

    public async Task<(bool, GeneralLedgerHeaderModel?, string)> PreviewGLFromInvoice(InvoicesModel invoice)
    {
        return await PreviewGLFromInvoiceHelper.PreviewGLFromInvoice(invoice);
    }

    public async Task<(bool, GeneralLedgerHeaderModel?, string)> PreviewGLFromCashBook(CashBookModel cashBook)
    {
        return await PreviewGLFromCashBookHelper.PreviewGLFromCashBook(cashBook);
    }

    public async Task<(bool, GeneralLedgerHeaderModel?, string)> PreviewGLFromPettyCash(PettyCashModel pettyCash)
    {
        return await PreviewGLFromPettyCashHelper.PreviewGLFromPettyCash(pettyCash);
    }

    public async Task<(bool, GeneralLedgerHeaderModel, string)> CreateGLFromPettyCash(int pettyCashId, int userId, string clientInfo)
    {
        return await CreateGLFromPettyCashHelper.CreateGLFromPettyCash(this, pettyCashId, userId, clientInfo);
    }
}
                
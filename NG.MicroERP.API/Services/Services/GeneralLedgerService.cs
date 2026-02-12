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
        using var connection = new SqlConnection(new Config().DefaultConnectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

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
                // We must use the same connection/transaction if we want to be safe, but GetCode makes its own connection.
                // For now, let's just generate it. 
                // ideally GetCode should accept a connection/transaction, but let's replicate the logic or use dapper directly here for safety in transaction.
                // Replicating GetCode logic for safety within transaction:
                string prefix = "GL";
                string sqlGetCode = $@"SELECT MAX(CAST(SUBSTRING(EntryNo, {prefix.Length + 1}, LEN(EntryNo) - {prefix.Length}) AS INT))
                                     FROM GeneralLedgerHeader WHERE LEFT(EntryNo, {prefix.Length}) = '{prefix}'";
                var maxCode = await connection.QueryFirstOrDefaultAsync<int?>(sqlGetCode, transaction: transaction);
                entryNo = prefix + ((maxCode ?? 0) + 1).ToString("000000");
                obj.EntryNo = entryNo;
            }

            string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{entryNo.ToUpper()}' AND IsSoftDeleted = 0;";
            var duplicate = await connection.QueryFirstOrDefaultAsync<GeneralLedgerHeaderModel>(SQLDuplicate, transaction: transaction);
            if (duplicate != null)
            {
                return (false, null!, "Duplicate Entry No found.");
            }
            
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
				@OrganizationId,
				@EntryNo, 
				@EntryDate,
				@Source,
				@Description, 
				@ReferenceNo,
				@ReferenceType,
				@ReferenceId,
				@PartyId,
				@LocationId,
				@BaseCurrencyId,
				@EnteredCurrencyId,
				@TotalDebit,
				@TotalCredit,
				@IsReversed,
				@ReversedEntryNo,
				@IsPosted,
				@PostedDate,
				@PostedBy,
				@IsAdjusted,
				@AdjustmentEntryNo,
				@FileAttachment,
				@Notes,
				@CreatedBy,
				@CreatedOn,
				@CreatedFrom, 
				@IsSoftDeleted
			);
			SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var headerIdDecimal = await connection.ExecuteScalarAsync<decimal>(SQLInsertHeader, new {
                obj.OrganizationId,
                EntryNo = entryNo.ToUpper(),
                obj.EntryDate,
                Source = obj.Source?.ToUpper() ?? "MANUAL",
                Description = obj.Description?.ToUpper() ?? "",
                ReferenceNo = obj.ReferenceNo?.ToUpper() ?? "",
                ReferenceType = obj.ReferenceType?.ToUpper() ?? "",
                obj.ReferenceId,
                PartyId = obj.PartyId > 0 ? obj.PartyId : (int?)null,
                LocationId = obj.LocationId > 0 ? obj.LocationId : (int?)null,
                BaseCurrencyId = obj.BaseCurrencyId > 0 ? obj.BaseCurrencyId : (int?)null,
                EnteredCurrencyId = obj.EnteredCurrencyId > 0 ? obj.EnteredCurrencyId : (int?)null,
                totalDebit,
                totalCredit,
                obj.IsReversed,
                ReversedEntryNo = obj.ReversedEntryNo?.ToUpper() ?? "",
                obj.IsPosted,
                obj.PostedDate,
                PostedBy = obj.PostedBy > 0 ? obj.PostedBy : (int?)null,
                obj.IsAdjusted,
                AdjustmentEntryNo = obj.AdjustmentEntryNo?.ToUpper() ?? "",
                FileAttachment = obj.FileAttachment ?? "",
                Notes = obj.Notes?.ToUpper() ?? "",
                obj.CreatedBy,
                CreatedOn = DateTime.Now,
                CreatedFrom = obj.CreatedFrom?.ToUpper() ?? "",
                obj.IsSoftDeleted
            }, transaction: transaction);
            
            int headerId = Convert.ToInt32(headerIdDecimal);

            // Insert Details
            int seqNo = 1;
            foreach (var detail in obj.Details)
            {
                // Auto-resolve AccountId from Party if missing
                if (detail.AccountId == 0 && detail.PartyId > 0)
                {
                     var partyAccount = await connection.QueryFirstOrDefaultAsync<int?>("SELECT AccountId FROM Parties WHERE Id = @Id", new { Id = detail.PartyId }, transaction: transaction);
                     if (partyAccount.HasValue && partyAccount.Value > 0)
                     {
                         detail.AccountId = partyAccount.Value;
                     }
                }

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
					@HeaderId,
					@AccountId,
					@Description,
					@DebitAmount,
					@CreditAmount,
					@PartyId,
					@LocationId,
					@CostCenterId,
					@ProjectId,
					@CurrencyId,
					@ExchangeRate,
					@SeqNo,
					@CreatedBy, 
					@CreatedOn, 
					@CreatedFrom, 
					@IsSoftDeleted
				);";

                await connection.ExecuteAsync(SQLInsertDetail, new {
                    HeaderId = headerId,
                    detail.AccountId,
                    Description = detail.Description?.ToUpper() ?? "",
                    detail.DebitAmount,
                    detail.CreditAmount,
                    PartyId = detail.PartyId > 0 ? detail.PartyId : (int?)null,
                    LocationId = detail.LocationId > 0 ? detail.LocationId : (int?)null,
                    CostCenterId = detail.CostCenterId > 0 ? detail.CostCenterId : (int?)null,
                    ProjectId = detail.ProjectId > 0 ? detail.ProjectId : (int?)null,
                    CurrencyId = detail.CurrencyId > 0 ? detail.CurrencyId : (int?)null,
                    detail.ExchangeRate,
                    SeqNo = seqNo++,
                    obj.CreatedBy,
                    CreatedOn = DateTime.Now,
                    CreatedFrom = obj.CreatedFrom?.ToUpper() ?? "",
                    detail.IsSoftDeleted
                }, transaction: transaction);
            }

            transaction.Commit();

            // Return the complete header with details
            var result = await Get(headerId);
            return (true, result!.Item2!, "");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, GeneralLedgerHeaderModel, string)> Put(GeneralLedgerHeaderModel obj)
    {
        using var connection = new SqlConnection(new Config().DefaultConnectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

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
            // Important: also check if we are changing the date, because if we move from open to closed period it should fail.
            // But we should always validate the date we are trying to save.
            var periodCheck = await ValidatePeriod(obj.OrganizationId, obj.EntryDate ?? DateTime.Now, "GENERALLEDGER");
            if (!periodCheck.Item1)
                return (false, null!, periodCheck.Item2);

            string SQLDuplicate = $@"SELECT * FROM GeneralLedgerHeader WHERE UPPER(EntryNo) = '{obj.EntryNo!.ToUpper()}' AND Id != {obj.Id} AND IsSoftDeleted = 0;";
            var duplicate = await connection.QueryFirstOrDefaultAsync<GeneralLedgerHeaderModel>(SQLDuplicate, transaction: transaction);
            if (duplicate != null)
            {
                 return (false, null!, "Duplicate Entry No found.");
            }

            // Update Header
            string SQLUpdateHeader = $@"UPDATE GeneralLedgerHeader SET 
					OrganizationId = @OrganizationId, 
					EntryNo = @EntryNo,
					EntryDate = @EntryDate,
					Source = @Source,
					Description = @Description, 
					ReferenceNo = @ReferenceNo,
					ReferenceType = @ReferenceType,
					ReferenceId = @ReferenceId,
					PartyId = @PartyId,
					LocationId = @LocationId,
					BaseCurrencyId = @BaseCurrencyId,
					EnteredCurrencyId = @EnteredCurrencyId,
					ExchangeRate = @ExchangeRate,
					TotalDebit = @TotalDebit,
					TotalCredit = @TotalCredit,
					IsReversed = @IsReversed,
					ReversedEntryNo = @ReversedEntryNo,
					IsPosted = @IsPosted,
					PostedDate = @PostedDate,
					PostedBy = @PostedBy,
					IsAdjusted = @IsAdjusted,
					AdjustmentEntryNo = @AdjustmentEntryNo,
					FileAttachment = @FileAttachment,
					Notes = @Notes,
					UpdatedBy = @UpdatedBy, 
					UpdatedOn = @UpdatedOn, 
					UpdatedFrom = @UpdatedFrom, 
					IsSoftDeleted = @IsSoftDeleted 
				WHERE Id = @Id;";

            int affected = await connection.ExecuteAsync(SQLUpdateHeader, new {
                obj.OrganizationId,
                EntryNo = obj.EntryNo?.ToUpper(),
                obj.EntryDate,
                Source = obj.Source?.ToUpper() ?? "MANUAL",
                Description = obj.Description?.ToUpper() ?? "",
                ReferenceNo = obj.ReferenceNo?.ToUpper() ?? "",
                ReferenceType = obj.ReferenceType?.ToUpper() ?? "",
                obj.ReferenceId,
                PartyId = obj.PartyId > 0 ? obj.PartyId : (int?)null,
                LocationId = obj.LocationId > 0 ? obj.LocationId : (int?)null,
                BaseCurrencyId = obj.BaseCurrencyId > 0 ? obj.BaseCurrencyId : (int?)null,
                EnteredCurrencyId = obj.EnteredCurrencyId > 0 ? obj.EnteredCurrencyId : (int?)null,
                obj.ExchangeRate,
                totalDebit,
                totalCredit,
                obj.IsReversed,
                ReversedEntryNo = obj.ReversedEntryNo?.ToUpper() ?? "",
                obj.IsPosted,
                obj.PostedDate,
                PostedBy = obj.PostedBy > 0 ? obj.PostedBy : (int?)null,
                obj.IsAdjusted,
                AdjustmentEntryNo = obj.AdjustmentEntryNo?.ToUpper() ?? "",
                FileAttachment = obj.FileAttachment ?? "",
                Notes = obj.Notes?.ToUpper() ?? "",
                obj.UpdatedBy,
                UpdatedOn = DateTime.Now,
                CreatedFrom = obj.UpdatedFrom?.ToUpper() ?? "",
                obj.IsSoftDeleted,
                obj.Id
            }, transaction: transaction);

            if (affected == 0)
            {
                 transaction.Rollback();
                 return (false, null!, "Update failed. Record may not exist.");
            }

            // Delete existing details (Soft Delete)
            string SQLDeleteDetails = $@"UPDATE GeneralLedgerDetail SET IsSoftDeleted = 1 WHERE HeaderId = @Id";
            await connection.ExecuteAsync(SQLDeleteDetails, new { Id = obj.Id }, transaction: transaction);

            // Insert new details
            int seqNo = 1;
            foreach (var detail in obj.Details)
            {
                // Auto-resolve AccountId from Party if missing
                if (detail.AccountId == 0 && detail.PartyId > 0)
                {
                     var partyAccount = await connection.QueryFirstOrDefaultAsync<int?>("SELECT AccountId FROM Parties WHERE Id = @Id", new { Id = detail.PartyId }, transaction: transaction);
                     if (partyAccount.HasValue && partyAccount.Value > 0)
                     {
                         detail.AccountId = partyAccount.Value;
                     }
                }

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
					@HeaderId,
					@AccountId,
					@Description,
					@DebitAmount,
					@CreditAmount,
					@PartyId,
					@LocationId,
					@CostCenterId,
					@ProjectId,
					@CurrencyId,
					@ExchangeRate,
					@SeqNo,
					@CreatedBy, 
					@CreatedOn, 
					@CreatedFrom, 
					@IsSoftDeleted
				);";

                await connection.ExecuteAsync(SQLInsertDetail, new {
                    HeaderId = obj.Id,
                    detail.AccountId,
                    Description = detail.Description?.ToUpper() ?? "",
                    detail.DebitAmount,
                    detail.CreditAmount,
                    PartyId = detail.PartyId > 0 ? detail.PartyId : (int?)null,
                    LocationId = detail.LocationId > 0 ? detail.LocationId : (int?)null,
                    CostCenterId = detail.CostCenterId > 0 ? detail.CostCenterId : (int?)null,
                    ProjectId = detail.ProjectId > 0 ? detail.ProjectId : (int?)null,
                    CurrencyId = detail.CurrencyId > 0 ? detail.CurrencyId : (int?)null,
                    detail.ExchangeRate,
                    SeqNo = seqNo++,
                    obj.UpdatedBy,
                    CreatedOn = DateTime.Now,
                    CreatedFrom = obj.UpdatedFrom?.ToUpper() ?? "",
                    detail.IsSoftDeleted
                }, transaction: transaction);
            }

            transaction.Commit();

            // Return the complete header with details
            var result = await Get(obj.Id);
            return (true, result!.Item2!, "");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
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
                
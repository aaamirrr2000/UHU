using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;


public interface IPettyCashService
{
    Task<(bool, List<PettyCashModel>)>? Search(string Criteria = "");
    Task<(bool, PettyCashModel?)>? Get(int id);
    Task<(bool, PettyCashReportModel?)>? GetPettyCashReport(int id);
    Task<(bool, PettyCashModel, string)> Post(PettyCashModel obj);
    Task<(bool, PettyCashModel, string)> Put(PettyCashModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PettyCashModel obj);
}


public class PettyCashService : IPettyCashService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PettyCashModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT 'PETTY CASH' as Source, a.*, b.Name as LocationName FROM PettyCash as a
                        LEFT JOIN Locations as b on b.id=a.LocationId
                        Where a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PettyCashModel> result = (await dapper.SearchByQuery<PettyCashModel>(SQL)) ?? new List<PettyCashModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PettyCashModel?)>? Get(int id)
    {
        PettyCashModel result = (await dapper.SearchByID<PettyCashModel>("PettyCash", id)) ?? new PettyCashModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, PettyCashReportModel?)>? GetPettyCashReport(int id)
    {
        PettyCashReportModel result = (await dapper.SearchByID<PettyCashReportModel>("vw_PettyCashReport", id)) ?? new PettyCashReportModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, PettyCashModel, string)> Post(PettyCashModel obj)
    {
        try
        {
            // Validate period for petty cash creation
            if (obj.TranDate.HasValue)
            {
                var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.OrganizationId, obj.TranDate.Value, "PETTYCASH");
                if (!periodCheck.Item1)
                {
                    return (false, null!, periodCheck.Item2);
                }
            }

            string Code = dapper.GetCode("PCP", "PettyCash", "SeqNo")!;
            string SQLDuplicate = $@"SELECT * FROM PettyCash WHERE UPPER(SeqNo) = '{obj.SeqNo!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO PettyCash 
			(
				OrganizationId, 
				SeqNo,
                FileAttachment,
				LocationId, 
				PartyId, 
				TranDate, 
				Description, 
				Amount, 
				AccountId, 
				TranType, 
				PaymentMethod, 
				RefNo, 
				TranRef, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{Code!}', 
                '{obj.FileAttachment}',
				{obj.LocationId},
				{obj.PartyId},
				'{obj.TranDate!.Value.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.Description!.ToUpper()}', 
				{obj.Amount},
				{obj.AccountId},
				'{obj.TranType!.ToUpper()}', 
				'{obj.PaymentMethod!.ToUpper()}', 
				'{obj.RefNo!.ToUpper()}', 
				'{obj.TranRef!.ToUpper()}', 
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<PettyCashModel> Output = new List<PettyCashModel>();
                var result = await Search($"a.id={res.Item2}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                return (false, null!, "Duplicate Record Found.");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, PettyCashModel, string)> Put(PettyCashModel obj)
    {
        try
        {
            // Check if petty cash is posted to GL - prevent updates
            var existingPettyCash = await dapper.SearchByQuery<PettyCashModel>($"SELECT * FROM PettyCash WHERE Id = {obj.Id}");
            if (existingPettyCash != null && existingPettyCash.Any() && existingPettyCash.First().IsPostedToGL == 1)
            {
                return (false, null!, "Cannot update petty cash entry that is posted to General Ledger.");
            }

            // Validate period for petty cash update
            if (obj.TranDate.HasValue)
            {
                var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.OrganizationId, obj.TranDate.Value, "PETTYCASH");
                if (!periodCheck.Item1)
                {
                    return (false, null!, periodCheck.Item2);
                }
            }

            string SQLDuplicate = $@"SELECT * FROM PettyCash WHERE UPPER(SeqNo) = '{obj.SeqNo!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE PettyCash SET 
					OrganizationId = {obj.OrganizationId}, 
					SeqNo = '{obj.SeqNo!.ToUpper()}', 
                    FileAttachment = '{obj.FileAttachment}', 
					LocationId = {obj.LocationId}, 
					PartyId = {obj.PartyId}, 
					TranDate = '{obj.TranDate!.Value.ToString("yyyy-MM-dd HH:mm:ss")}', 
					Description = '{obj.Description!.ToUpper()}', 
					Amount = {obj.Amount}, 
					AccountId = {obj.AccountId}, 
					TranType = '{obj.TranType!.ToUpper()}', 
					PaymentMethod = '{obj.PaymentMethod!.ToUpper()}', 
					RefNo = '{obj.RefNo!.ToUpper()}', 
					TranRef = '{obj.TranRef!.ToUpper()}',  
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<PettyCashModel> Output = new List<PettyCashModel>();
                var result = await Search($"a.id={obj.Id}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                return (false, null!, "Duplicate Record Found.");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }

    }

    public async Task<(bool, string)> Delete(int id)
    {
        // Check if petty cash is posted to GL - prevent deletion
        var pettyCash = await dapper.SearchByQuery<PettyCashModel>($"SELECT * FROM PettyCash WHERE Id = {id}");
        if (pettyCash != null && pettyCash.Any() && pettyCash.First().IsPostedToGL == 1)
        {
            return (false, "Cannot delete petty cash entry that is posted to General Ledger.");
        }
        return await dapper.Delete("PettyCash", id);
    }

    public async Task<(bool, string)> SoftDelete(PettyCashModel obj)
    {
        // Check if petty cash is posted to GL - prevent deletion
        var pettyCash = await dapper.SearchByQuery<PettyCashModel>($"SELECT * FROM PettyCash WHERE Id = {obj.Id}");
        if (pettyCash != null && pettyCash.Any() && pettyCash.First().IsPostedToGL == 1)
        {
            return (false, "Cannot delete petty cash entry that is posted to General Ledger.");
        }
        string SQLUpdate = $@"UPDATE PettyCash SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}


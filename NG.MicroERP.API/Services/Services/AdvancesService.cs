using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;


public interface IAdvancesService
{
    Task<(bool, List<AdvancesModel>)>? Search(string Criteria = "");
    Task<(bool, AdvancesModel?)>? Get(int id);
    Task<(bool, AdvancesReportModel?)>? GetAdvancesReport(int id);
    Task<(bool, AdvancesModel, string)> Post(AdvancesModel obj);
    Task<(bool, AdvancesModel, string)> Put(AdvancesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(AdvancesModel obj);
}


public class AdvancesService : IAdvancesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<AdvancesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT 'ADVANCES' as Source, a.*, b.Name as LocationName FROM Advances as a
                        LEFT JOIN Locations as b on b.id=a.LocationId
                        Where a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<AdvancesModel> result = (await dapper.SearchByQuery<AdvancesModel>(SQL)) ?? new List<AdvancesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, AdvancesModel?)>? Get(int id)
    {
        AdvancesModel result = (await dapper.SearchByID<AdvancesModel>("Advances", id)) ?? new AdvancesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, AdvancesReportModel?)>? GetAdvancesReport(int id)
    {
        AdvancesReportModel result = (await dapper.SearchByID<AdvancesReportModel>("vw_AdvancesReport", id)) ?? new AdvancesReportModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, AdvancesModel, string)> Post(AdvancesModel obj)
    {
        try
        {
            // Validate period for advances creation
            if (obj.TranDate.HasValue)
            {
                var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.OrganizationId, obj.TranDate.Value, "ADVANCES");
                if (!periodCheck.Item1)
                {
                    return (false, null!, periodCheck.Item2);
                }
            }

            string Code = dapper.GetCode("ADP", "Advances", "SeqNo")!;
            string SQLDuplicate = $@"SELECT * FROM Advances WHERE UPPER(SeqNo) = '{obj.SeqNo!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Advances 
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
                List<AdvancesModel> Output = new List<AdvancesModel>();
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

    public async Task<(bool, AdvancesModel, string)> Put(AdvancesModel obj)
    {
        try
        {
            // Check if advances is posted to GL - prevent updates
            var existingAdvances = await dapper.SearchByQuery<AdvancesModel>($"SELECT * FROM Advances WHERE Id = {obj.Id}");
            if (existingAdvances != null && existingAdvances.Any() && existingAdvances.First().IsPostedToGL == 1)
            {
                return (false, null!, "Cannot update advance entry that is posted to General Ledger.");
            }

            // Validate period for advances update
            if (obj.TranDate.HasValue)
            {
                var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.OrganizationId, obj.TranDate.Value, "ADVANCES");
                if (!periodCheck.Item1)
                {
                    return (false, null!, periodCheck.Item2);
                }
            }

            string SQLDuplicate = $@"SELECT * FROM Advances WHERE UPPER(SeqNo) = '{obj.SeqNo!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE Advances SET 
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
                List<AdvancesModel> Output = new List<AdvancesModel>();
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
        // Check if advances is posted to GL - prevent deletion
        var advances = await dapper.SearchByQuery<AdvancesModel>($"SELECT * FROM Advances WHERE Id = {id}");
        if (advances != null && advances.Any() && advances.First().IsPostedToGL == 1)
        {
            return (false, "Cannot delete advance entry that is posted to General Ledger.");
        }
        return await dapper.Delete("Advances", id);
    }

    public async Task<(bool, string)> SoftDelete(AdvancesModel obj)
    {
        // Check if advances is posted to GL - prevent deletion
        var advances = await dapper.SearchByQuery<AdvancesModel>($"SELECT * FROM Advances WHERE Id = {obj.Id}");
        if (advances != null && advances.Any() && advances.First().IsPostedToGL == 1)
        {
            return (false, "Cannot delete advance entry that is posted to General Ledger.");
        }
        string SQLUpdate = $@"UPDATE Advances SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}


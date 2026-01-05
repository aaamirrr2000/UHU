using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Services;

public interface ILeavesService
{
    Task<(bool, List<LeavesModel>)>? Search(string Criteria = "");
    Task<(bool, LeavesModel?)>? Get(int id);
    Task<(bool, LeavesModel, string)> Post(LeavesModel obj);
    Task<(bool, LeavesModel, string)> Put(LeavesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(LeavesModel obj);
}

public class LeavesService : ILeavesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<LeavesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Leaves Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<LeavesModel> result = (await dapper.SearchByQuery<LeavesModel>(SQL)) ?? new List<LeavesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, LeavesModel?)>? Get(int id)
    {
        LeavesModel result = (await dapper.SearchByID<LeavesModel>("Leaves", id)) ?? new LeavesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, LeavesModel, string)> Post(LeavesModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO Leaves 
			(
				OrganizationId, 
				EmpId, 
				LeaveType, 
				FromDate, 
				ToDate, 
				Description, 
				Approved, 
				ApprovedBy, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				{obj.EmpId},
				'{obj.LeaveType!.ToUpper()}', 
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.Description!.ToUpper()}', 
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.ApprovedBy},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<LeavesModel> Output = new List<LeavesModel>();
                var result = await Search($"id={res.Item2}")!;
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

    public async Task<(bool, LeavesModel, string)> Put(LeavesModel obj)
    {
        try
        {   string SQLUpdate = $@"UPDATE Leaves SET 
					OrganizationId = {obj.OrganizationId}, 
					EmpId = {obj.EmpId}, 
					LeaveType = '{obj.LeaveType!.ToUpper()}', 
					FromDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					ToDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					Description = '{obj.Description!.ToUpper()}', 
					Approved = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					ApprovedBy = {obj.ApprovedBy}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<LeavesModel> Output = new List<LeavesModel>();
                var result = await Search($"id={obj.Id}")!;
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
        return await dapper.Delete("Leaves", id);
    }

    public async Task<(bool, string)> SoftDelete(LeavesModel obj)
    {
        string SQLUpdate = $@"UPDATE Leaves SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
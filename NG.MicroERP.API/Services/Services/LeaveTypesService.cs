using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface ILeaveTypesService
{
    Task<(bool, List<LeaveTypesModel>)>? Search(string Criteria = "");
    Task<(bool, LeaveTypesModel?)>? Get(int id);
    Task<(bool, LeaveTypesModel, string)> Post(LeaveTypesModel obj);
    Task<(bool, LeaveTypesModel, string)> Put(LeaveTypesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(LeaveTypesModel obj);
}


public class LeaveTypesService : ILeaveTypesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<LeaveTypesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM LeaveTypes Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id";

        List<LeaveTypesModel> result = (await dapper.SearchByQuery<LeaveTypesModel>(SQL)) ?? new List<LeaveTypesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, LeaveTypesModel?)>? Get(int id)
    {
        LeaveTypesModel result = (await dapper.SearchByID<LeaveTypesModel>("LeaveTypes", id)) ?? new LeaveTypesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, LeaveTypesModel, string)> Post(LeaveTypesModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO LeaveTypes 
			(
				OrganizationId, 
				LeaveName, 
				Description, 
				IsPaid, 
				MaxDaysPerYear, 
				CarryForward, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.LeaveName!.ToUpper()}', 
				'{obj.Description!.ToUpper()}', 
				{obj.IsPaid},
				{obj.MaxDaysPerYear},
				{obj.CarryForward},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<LeaveTypesModel> Output = new List<LeaveTypesModel>();
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
            return (true, null!, ex.Message);
        }
    }

    public async Task<(bool, LeaveTypesModel, string)> Put(LeaveTypesModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE LeaveTypes SET 
					OrganizationId = {obj.OrganizationId}, 
					LeaveName = '{obj.LeaveName!.ToUpper()}', 
					Description = '{obj.Description!.ToUpper()}', 
					IsPaid = {obj.IsPaid}, 
					MaxDaysPerYear = {obj.MaxDaysPerYear}, 
					CarryForward = {obj.CarryForward}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<LeaveTypesModel> Output = new List<LeaveTypesModel>();
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
            return (true, null!, ex.Message);
        }

    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("LeaveTypes", id);
    }

    public async Task<(bool, string)> SoftDelete(LeaveTypesModel obj)
    {
        string SQLUpdate = $@"UPDATE LeaveTypes SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



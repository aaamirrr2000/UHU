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

public interface IGroupsService
{
    Task<(bool, List<GroupsModel>)>? Search(string Criteria = "");
    Task<(bool, GroupsModel?)>? Get(int id);
    Task<(bool, GroupsModel, string)> Post(GroupsModel obj);
    Task<(bool, string)> Put(GroupsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(GroupsModel obj);
}


public class GroupsService : IGroupsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<GroupsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Groups Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<GroupsModel> result = (await dapper.SearchByQuery<GroupsModel>(SQL)) ?? new List<GroupsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, GroupsModel?)>? Get(int id)
    {
        GroupsModel result = (await dapper.SearchByID<GroupsModel>("Groups", id)) ?? new GroupsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, GroupsModel, string)> Post(GroupsModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO Groups 
			(
				OrganizationId, 
				Name, 
				Dashboard, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.Name!.ToUpper()}', 
				'{obj.Dashboard!}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<GroupsModel> Output = new List<GroupsModel>();
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

    public async Task<(bool, string)> Put(GroupsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Groups SET 
					OrganizationId = {obj.OrganizationId}, 
					Name = '{obj.Name!.ToUpper()}', 
					Dashboard = '{obj.Dashboard!}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            return await dapper.Update(SQLUpdate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Groups", id);
    }

    public async Task<(bool, string)> SoftDelete(GroupsModel obj)
    {
        string SQLUpdate = $@"UPDATE Groups SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

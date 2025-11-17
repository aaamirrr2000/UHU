using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;
using Serilog;

namespace NG.MicroERP.API.Services;

public interface IOrganizationsService
{
    Task<(bool, List<OrganizationsModel>)>? Search(string Criteria = "");
    Task<(bool, OrganizationsModel?)>? Get(int id);
    Task<(bool, OrganizationsModel, string)> Post(OrganizationsModel obj);
	Task<(bool, OrganizationsModel, string)> Put(OrganizationsModel obj);
    Task<(bool, string)> SetParent(OrganizationsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(OrganizationsModel obj);
}

public class OrganizationsService : IOrganizationsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<OrganizationsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"
					SELECT
					  a.ID,
					  a.Name,
					  a.Code,
					  a.EntraId,
					  a.Logo,
					  a.Wallpaper,
					  a.ThemeColor,
					  a.MenuColor,
					  a.Name,
					  a.Description,
					  a.ParentId,  
					  b.Name as ParentOrganizationName,
					  a.Phone,
					  a.Email,
					  a.Address,
					  a.MaxUsers,
					  a.DbSize,
					  a.LoginPic,
					  a.Industry,
					  a.Website,
					  a.TimeZone,
					  a.GMT,
					  a.IsVerified,
					  a.Expiry,
					  a.ParentId,
					  a.IsActive,
					  a.CreatedBy,
					  a.CreatedOn,
					  a.CreatedFrom,
					  a.UpdatedBy,
					  a.UpdatedOn,
					  a.UpdatedFrom,
					  a.IsSoftDeleted
					FROM Organizations as a
					LEFT JOIN Organizations as b on b.ID = a.ParentId
					Where a.IsSoftDeleted = 0
					";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by a.Id";

        List<OrganizationsModel> result = (await dapper.SearchByQuery<OrganizationsModel>(SQL)) ?? new List<OrganizationsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, OrganizationsModel?)>? Get(int id)
    {
        OrganizationsModel result = (await dapper.SearchByID<OrganizationsModel>("Organizations", id)) ?? new OrganizationsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, OrganizationsModel, string)> Post(OrganizationsModel obj)
    {

        try
        {
            string Code = dapper.GetCode("ORG", "Organizations", "Code", 4)!;
            string SQLInsert = $@"INSERT INTO Organizations 
			(
				Code, 
				EntraId, 
				Logo, 
				Wallpaper,
				ThemeColor, 
				MenuColor, 
				Name, 
				Description, 
				Phone, 
				Email, 
				Address, 
				MaxUsers, 
				DbSize, 
				LoginPic, 
				Industry, 
				Website, 
				TimeZone,
				GMT,
				IsVerified, 
				Expiry, 
				ParentId, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				'{obj.Code!.ToUpper()}', 
				'{obj.EntraId!.ToUpper()}', 
				'{obj.Logo!}', 
				'{obj.Wallpaper!}', 
				'{obj.ThemeColor!.ToUpper()}', 
				'{obj.MenuColor!.ToUpper()}', 
				'{obj.Name!.ToUpper()}', 
				'{obj.Description!.ToUpper()}', 
				'{obj.Phone!.ToUpper()}', 
				'{obj.Email!}', 
				'{obj.Address!.ToUpper()}', 
				{obj.MaxUsers},
				{obj.DbSize},
				'{obj.LoginPic!.ToUpper()}', 
				'{obj.Industry!.ToUpper()}', 
				'{obj.Website!}', 
				'{obj.TimeZone!.ToUpper()}', 
				{obj.GMT},
				{obj.IsVerified},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.ParentId},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<OrganizationsModel> Output = new List<OrganizationsModel>();
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

    public async Task<(bool, OrganizationsModel, string)> Put(OrganizationsModel obj)
    {
        try
        {
			if (obj.ParentId == 0)
			{
				// Update all child organizations under the old parent to the new parent
				string SQL1 = $"UPDATE Organizations SET ParentId = {obj.Id} WHERE ParentId = {obj.ParentId}";
				var res1 = await dapper.Update(SQL1);

				// Update the organization that had ParentId = 0 (the root) to the new parent
				string SQL2 = $"UPDATE Organizations SET ParentId = {obj.Id} WHERE Id = {obj.ParentId} AND ParentId = 0";
				var res2 = await dapper.Update(SQL2);

			}

			string SQLDuplicate = $@"SELECT * FROM Organizations WHERE UPPER(Code) = '{obj.Code!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE Organizations SET 
					Code = '{obj.Code!.ToUpper()}', 
					EntraId = '{obj.EntraId!.ToUpper()}', 
					Logo = '{obj.Logo!.ToUpper()}',		
					Wallpaper = '{obj.Wallpaper!.ToUpper()}', 
					ThemeColor = '{obj.ThemeColor!.ToUpper()}', 
					MenuColor = '{obj.MenuColor!.ToUpper()}', 
					Name = '{obj.Name!.ToUpper()}', 
					Description = '{obj.Description!.ToUpper()}', 
					Phone = '{obj.Phone!.ToUpper()}', 
					Email = '{obj.Email!}', 
					Address = '{obj.Address!.ToUpper()}', 
					MaxUsers = {obj.MaxUsers}, 
					DbSize = {obj.DbSize}, 
					LoginPic = '{obj.LoginPic!.ToUpper()}', 
					Industry = '{obj.Industry!.ToUpper()}', 
					Website = '{obj.Website!}', 
					TimeZone = '{obj.TimeZone!.ToUpper()}', 
					GMT = {obj.GMT},
					IsVerified = {obj.IsVerified}, 
					ParentId = {obj.ParentId}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<OrganizationsModel> Output = new List<OrganizationsModel>();
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

    public async Task<(bool, string)> SetParent(OrganizationsModel obj)
    {
        try
        {
            int newRootId = obj.Id;

            // 1. Promote selected organization to root
            string sqlMakeRoot = $"UPDATE Organizations SET ParentId = 0 WHERE Id = {newRootId};";
            Log.Warning("1. " + sqlMakeRoot);
            var res1 = await dapper.Update(sqlMakeRoot);

            // 2. Make all other organizations children of the new root
            string sqlReassignAll = $"UPDATE Organizations SET ParentId = {newRootId} WHERE Id != {newRootId};";
            Log.Warning("2. " + sqlReassignAll);
            var res2 = await dapper.Update(sqlReassignAll);

            if (res1.Item1 && res2.Item1)
                return (true, "New parent set successfully.");
            else
                return (false, "One or more SQL updates failed.");
        }
        catch (Exception ex)
        {
            Log.Warning(ex.Message);
            return (false, $"Exception: {ex.Message}");
        }
    }



    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Organizations", id);
    }

    public async Task<(bool, string)> SoftDelete(OrganizationsModel obj)
    {
        string SQLUpdate = $@"UPDATE Organizations SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
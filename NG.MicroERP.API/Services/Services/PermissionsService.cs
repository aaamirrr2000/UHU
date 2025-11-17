using Dapper;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services;

public interface IPermissionsService
{
    Task<(bool, List<PermissionsModel>)>? Search(string Criteria = "");
    Task<(bool, PermissionsModel?)>? Get(int id);
    Task<(bool, PermissionsModel, string)> Post(PermissionsModel obj);
    Task<(bool, PermissionsModel, string)> Put(PermissionsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PermissionsModel obj);
    Task<List<GroupMenuModel>>? SearchGroupMenu(int OrganizationId, string Additional_Info = "");
}


public class PermissionsService : IPermissionsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PermissionsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Permissions Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PermissionsModel> result = (await dapper.SearchByQuery<PermissionsModel>(SQL)) ?? new List<PermissionsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PermissionsModel?)>? Get(int id)
    {
        PermissionsModel result = (await dapper.SearchByID<PermissionsModel>("Permissions", id)) ?? new PermissionsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<bool> Save(PermissionsModel obj)
    {
        try
        {
            Config cfg = new Config();
            string SQLExistYN = $"SELECT * FROM Permissions where MenuId={obj.MenuId} and GroupId={obj.GroupId} and OrganizationId={obj.OrganizationId}";
            string DBConnection = cfg.DefaultDB();

            using IDbConnection cnn = new MySqlConnection(DBConnection);

            int count = await cnn.ExecuteScalarAsync<int>(SQLExistYN);

            if (count > 0)
            {
                var res = await Put(obj);
                return res.Item1;
            }
            else
            {
                var res = await Post(obj);
                return res.Item1;
            }


            /*
            IEnumerable<PermissionsModel> result = await cnn.QueryAsync<PermissionsModel>(SQLExistYN, new DynamicParameters());

            if (result.FirstOrDefault() != null && Convert.ToInt32(result.FirstOrDefault()) > 0)
            {
                var res = await Put(obj);
                return res.Item1;
            }
            else
            {
                var res = await Post(obj);
                return res.Item1;
            }

            */

        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<(bool, PermissionsModel, string)> Post(PermissionsModel obj)
    {

        try
        {
 
            string SQLInsert = $@"INSERT INTO Permissions 
			(
				OrganizationId, 
				GroupId, 
				MenuId, 
				Privilege, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				{obj.GroupId},
				{obj.MenuId},
				'{obj.Privilege!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PermissionsModel> Output = new List<PermissionsModel>();
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

    public async Task<(bool, PermissionsModel, string)> Put(PermissionsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Permissions SET 
					OrganizationId = {obj.OrganizationId}, 
					GroupId = {obj.GroupId}, 
					MenuId = {obj.MenuId}, 
					Privilege = '{obj.Privilege!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE MenuId={obj.MenuId} and GroupId={obj.GroupId} and OrganizationId={obj.OrganizationId};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<PermissionsModel> Output = new List<PermissionsModel>();
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
        return await dapper.Delete("Permissions", id);
    }

    public async Task<(bool, string)> SoftDelete(PermissionsModel obj)
    {
        string SQLUpdate = $@"UPDATE Permissions SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }

    public async Task<List<GroupMenuModel>>? SearchGroupMenu(int OrganizationId, string Criteria = "")
    {
        string SQL = $"Select * from GroupMenu where OrganizationId = {OrganizationId}";

        if (Debugger.IsAttached == true)
        {
            SQL += " and live>=0";
        }
        else
        {
            SQL += " and live=1";
        }

        if (Criteria.Length > 0)
        {
            SQL += " and " + Criteria;
        }

        SQL += " Order by SeqNo, ParentId, MenuId";

        List<GroupMenuModel> result = (await dapper.SearchByQuery<GroupMenuModel>(SQL)) ?? [];
        return result;
    }

}
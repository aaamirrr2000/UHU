using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Services.Services;

public interface IRestaurantTablesService
{
    Task<(bool, List<RestaurantTablesModel>)>? Search(string Criteria = "");
    Task<(bool, RestaurantTablesModel?)>? Get(int id);
    Task<(bool, RestaurantTablesModel, string)> Post(RestaurantTablesModel obj);
    Task<(bool, RestaurantTablesModel, string)> Put(RestaurantTablesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(RestaurantTablesModel obj);
}

public class RestaurantTablesService : IRestaurantTablesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<RestaurantTablesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM RestaurantTables WHERE IsSoftDeleted=0 AND IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " AND " + Criteria;

        SQL += " ORDER BY TableNumber ASC";

        List<RestaurantTablesModel> result = (await dapper.SearchByQuery<RestaurantTablesModel>(SQL)) ?? new List<RestaurantTablesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, RestaurantTablesModel?)>? Get(int id)
    {
        RestaurantTablesModel result = (await dapper.SearchByID<RestaurantTablesModel>("RestaurantTables", id)) ?? new RestaurantTablesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, RestaurantTablesModel, string)> Post(RestaurantTablesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM RestaurantTables WHERE UPPER(TableNumber) = '{obj.TableNumber!.ToUpper().Replace("'", "''")}' AND IsSoftDeleted=0;";
            string SQLInsert = $@"INSERT INTO RestaurantTables 
                (
                    OrganizationId, 
                    TableNumber, 
                    Capacity, 
                    IsAvailable, 
                    TableLocation, 
                    Notes, 
                    IsActive, 
                    CreatedBy, 
                    CreatedOn, 
                    CreatedFrom, 
                    IsSoftDeleted
                ) 
                VALUES 
                (
                    {obj.OrganizationId},
                    '{obj.TableNumber!.ToUpper().Replace("'", "''")}', 
                    {obj.Capacity}, 
                    {obj.IsAvailable}, 
                    '{obj.TableLocation!.ToUpper().Replace("'", "''")}', 
                    '{obj.Notes!.ToUpper().Replace("'", "''")}', 
                    {obj.IsActive},
                    {obj.CreatedBy},
                    '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
                    '{obj.CreatedFrom!.ToUpper().Replace("'", "''")}', 
                    {obj.IsSoftDeleted}
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<RestaurantTablesModel> Output = new List<RestaurantTablesModel>();
                var result = await Search($"Id={res.Item2}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                return (false, null!, res.Item3 ?? "Duplicate Record Found.");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, RestaurantTablesModel, string)> Put(RestaurantTablesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM RestaurantTables WHERE UPPER(TableNumber) = '{obj.TableNumber!.ToUpper().Replace("'", "''")}' AND Id != {obj.Id} AND IsSoftDeleted=0;";
            string SQLUpdate = $@"UPDATE RestaurantTables SET 
                    OrganizationId = {obj.OrganizationId}, 
                    TableNumber = '{obj.TableNumber!.ToUpper().Replace("'", "''")}', 
                    Capacity = {obj.Capacity}, 
                    IsAvailable = {obj.IsAvailable}, 
                    TableLocation = '{obj.TableLocation!.ToUpper().Replace("'", "''")}', 
                    Notes = '{obj.Notes!.ToUpper().Replace("'", "''")}', 
                    IsActive = {obj.IsActive}, 
                    UpdatedBy = {obj.UpdatedBy}, 
                    UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
                    UpdatedFrom = '{obj.UpdatedFrom!.ToUpper().Replace("'", "''")}', 
                    IsSoftDeleted = {obj.IsSoftDeleted} 
                WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<RestaurantTablesModel> Output = new List<RestaurantTablesModel>();
                var result = await Search($"Id={obj.Id}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                return (false, null!, res.Item2 ?? "Duplicate Record Found.");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("RestaurantTables", id);
    }

    public async Task<(bool, string)> SoftDelete(RestaurantTablesModel obj)
    {
        string SQLUpdate = $@"UPDATE RestaurantTables SET 
                    UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
                    UpdatedBy = {obj.UpdatedBy},
                    IsSoftDeleted = 1 
                WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

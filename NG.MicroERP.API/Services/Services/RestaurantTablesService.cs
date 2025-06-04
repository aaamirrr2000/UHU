using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services;

public interface IRestaurantTablesService
{
    Task<(bool, List<RestaurantTablesModel>)>? Search(string Criteria = "");
    Task<(bool, RestaurantTablesModel?)>? Get(int id);
    Task<(bool, RestaurantTablesModel, string)> Post(RestaurantTablesModel obj);
    Task<(bool, string)> Put(RestaurantTablesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(RestaurantTablesModel obj);
}

public class RestaurantTablesService : IRestaurantTablesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<RestaurantTablesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM RestaurantTables Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

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
            string SQLDuplicate = $@"SELECT * FROM RestaurantTables WHERE UPPER(TableNumber) = '{obj.TableNumber!.ToUpper()}';";
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
				'{obj.TableNumber!.ToUpper()}', 
				{obj.Capacity},
				{obj.IsAvailable},
				'{obj.TableLocation!.ToUpper()}', 
				'{obj.Notes!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<RestaurantTablesModel> Output = new List<RestaurantTablesModel>();
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

    public async Task<(bool, string)> Put(RestaurantTablesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM RestaurantTables WHERE UPPER(TableNumber) = '{obj.TableNumber!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE RestaurantTables SET 
                    OrganizationId = {obj.OrganizationId},
					TableNumber = '{obj.TableNumber!.ToUpper()}', 
					Capacity = {obj.Capacity}, 
					IsAvailable = {obj.IsAvailable}, 
					TableLocation = '{obj.TableLocation!.ToUpper()}', 
					Notes = '{obj.Notes!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            return await dapper.Update(SQLUpdate, SQLDuplicate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("RestaurantTables", id);
    }

    public async Task<(bool, string)> SoftDelete(RestaurantTablesModel obj)
    {
        string SQLUpdate = $@"UPDATE RestaurantTables SET 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
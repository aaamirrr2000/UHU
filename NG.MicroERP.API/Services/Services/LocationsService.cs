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

public interface ILocationsService
{
    Task<(bool, List<LocationsModel>)>? Search(string Criteria = "");
    Task<(bool, LocationsModel?)>? Get(int id);
    Task<(bool, LocationsModel, string)> Post(LocationsModel obj);
    Task<(bool, LocationsModel, string)> Put(LocationsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(LocationsModel obj);
}

public class LocationsService : ILocationsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<LocationsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Locations Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<LocationsModel> result = (await dapper.SearchByQuery<LocationsModel>(SQL)) ?? new List<LocationsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, LocationsModel?)>? Get(int id)
    {
        LocationsModel result = (await dapper.SearchByID<LocationsModel>("Locations", id)) ?? new LocationsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, LocationsModel, string)> Post(LocationsModel obj)
    {

        try
        {

            string Code = dapper.GetCode("", "Locations", "Code")!;
            string SQLDuplicate = $@"SELECT * FROM Locations WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Locations 
			(
				OrganizationId, 
				Code, 
				Name, 
				Address, 
				PocName, 
				PocEmail, 
				PocPhone, 
				LocationType, 
				Latitude, 
				Longitude, 
				Radius, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{Code!}', 
				'{obj.Name!.ToUpper()}', 
				'{obj.Address!.ToUpper()}', 
				'{obj.PocName!.ToUpper()}', 
				'{obj.PocEmail!.ToUpper()}', 
				'{obj.PocPhone!.ToUpper()}', 
				'{obj.LocationType!.ToUpper()}', 
				'{obj.Latitude!.ToUpper()}', 
				'{obj.Longitude!.ToUpper()}', 
				{obj.Radius},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<LocationsModel> Output = new List<LocationsModel>();
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

    public async Task<(bool, LocationsModel, string)> Put(LocationsModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Locations WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE Locations SET 
					OrganizationId = {obj.OrganizationId}, 
					Code = '{obj.Code!.ToUpper()}', 
					Name = '{obj.Name!.ToUpper()}', 
					Address = '{obj.Address!.ToUpper()}', 
					PocName = '{obj.PocName!.ToUpper()}', 
					PocEmail = '{obj.PocEmail!.ToUpper()}', 
					PocPhone = '{obj.PocPhone!.ToUpper()}', 
					LocationType = '{obj.LocationType!.ToUpper()}', 
					Latitude = '{obj.Latitude!.ToUpper()}', 
					Longitude = '{obj.Longitude!.ToUpper()}', 
					Radius = {obj.Radius}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<LocationsModel> Output = new List<LocationsModel>();
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
        return await dapper.Delete("Locations", id);
    }

    public async Task<(bool, string)> SoftDelete(LocationsModel obj)
    {
        string SQLUpdate = $@"UPDATE Locations SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
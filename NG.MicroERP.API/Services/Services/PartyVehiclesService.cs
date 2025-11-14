using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;

public interface IPartyVehiclesService
{
    Task<(bool, List<PartyVehiclesModel>)>? Search(string Criteria = "");
    Task<(bool, PartyVehiclesModel?)>? Get(int id);
    Task<(bool, PartyVehiclesModel, string)> Post(PartyVehiclesModel obj);
    Task<(bool, string)> Put(PartyVehiclesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PartyVehiclesModel obj);
}


public class PartyVehiclesService : IPartyVehiclesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PartyVehiclesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM PartyVehicles Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PartyVehiclesModel> result = (await dapper.SearchByQuery<PartyVehiclesModel>(SQL)) ?? new List<PartyVehiclesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PartyVehiclesModel?)>? Get(int id)
    {
        PartyVehiclesModel result = (await dapper.SearchByID<PartyVehiclesModel>("PartyVehicles", id)) ?? new PartyVehiclesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PartyVehiclesModel, string)> Post(PartyVehiclesModel obj)
    {

        try
        {
            string SQLDuplicate = $@"SELECT * FROM PartyVehicles WHERE UPPER(VehicleRegNo) = '{obj.VehicleRegNo!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO PartyVehicles 
			(
				PartyId, 
				VehicleRegNo, 
				EngineNo, 
				ChasisNo, 
				VehicleType, 
				MakeType,
				Model, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.PartyId},
				'{obj.VehicleRegNo!.ToUpper()}', 
				'{obj.EngineNo!.ToUpper()}', 
				'{obj.ChasisNo!.ToUpper()}', 
				'{obj.VehicleType!.ToUpper()}', 
				'{obj.MakeType!.ToUpper()}',
				'{obj.Model!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<PartyVehiclesModel> Output = new List<PartyVehiclesModel>();
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

    public async Task<(bool, string)> Put(PartyVehiclesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM PartyVehicles WHERE UPPER(code) = '{obj.VehicleRegNo!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE PartyVehicles SET 
					PartyId = {obj.PartyId}, 
					VehicleRegNo = '{obj.VehicleRegNo!.ToUpper()}', 
					EngineNo = '{obj.EngineNo!.ToUpper()}', 
					ChasisNo = '{obj.ChasisNo!.ToUpper()}', 
					VehicleType = '{obj.VehicleType!.ToUpper()}', 
					MakeType = '{obj.MakeType!.ToUpper()}',
					Model = '{obj.Model!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					CreatedBy = {obj.CreatedBy}, 
					CreatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					CreatedFrom = '{obj.CreatedFrom!.ToUpper()}', 
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
        return await dapper.Delete("PartyVehicles", id);
    }

    public async Task<(bool, string)> SoftDelete(PartyVehiclesModel obj)
    {
        string SQLUpdate = $@"UPDATE PartyVehicles SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

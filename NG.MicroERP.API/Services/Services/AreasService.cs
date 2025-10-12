using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IAreasService
{
    Task<(bool, List<AreasModel>)>? Search(string Criteria = "");
    Task<(bool, AreasModel?)>? Get(int id);
    Task<(bool, AreasModel, string)> Post(AreasModel obj);
    Task<(bool, string)> Put(AreasModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(AreasModel obj);
}


public class AreasService : IAreasService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<AreasModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Areas Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<AreasModel> result = (await dapper.SearchByQuery<AreasModel>(SQL)) ?? new List<AreasModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, AreasModel?)>? Get(int id)
    {
        AreasModel result = (await dapper.SearchByID<AreasModel>("Areas", id)) ?? new AreasModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, AreasModel, string)> Post(AreasModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO Areas 
			(
				OrganizationId, 
				AreaName, 
				AreaType, 
				ParentId, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				UpdatedBy, 
				UpdatedOn, 
				UpdatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.AreaName!.ToUpper()}', 
				'{obj.AreaType!.ToUpper()}', 
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
                List<AreasModel> Output = new List<AreasModel>();
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

    public async Task<(bool, string)> Put(AreasModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Areas SET 
					OrganizationId = {obj.OrganizationId}, 
					AreaName = '{obj.AreaName!.ToUpper()}', 
					AreaType = '{obj.AreaType!.ToUpper()}', 
					ParentId = {obj.ParentId}, 
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
        return await dapper.Delete("Areas", id);
    }

    public async Task<(bool, string)> SoftDelete(AreasModel obj)
    {
        string SQLUpdate = $@"UPDATE Areas SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



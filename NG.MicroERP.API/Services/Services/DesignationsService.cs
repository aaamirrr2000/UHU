using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IDesignationsService
{
    Task<(bool, List<DesignationsModel>)>? Search(string Criteria = "");
    Task<(bool, DesignationsModel?)>? Get(int id);
    Task<(bool, DesignationsModel, string)> Post(DesignationsModel obj);
    Task<(bool, DesignationsModel, string)> Put(DesignationsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(DesignationsModel obj);
}


public class DesignationsService : IDesignationsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<DesignationsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                          a.*,
                          b.DesignationName as ReportTo,
                          c.DepartmentName
                        FROM designations as a
                        LEFT JOIN designations as b on b.id=a.ParentId
                        LEFT JOIN departments as c on c.id=a.DepartmentId
                    WHERE a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<DesignationsModel> result = (await dapper.SearchByQuery<DesignationsModel>(SQL)) ?? new List<DesignationsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, DesignationsModel?)>? Get(int id)
    {
        DesignationsModel result = (await dapper.SearchByID<DesignationsModel>("designations", id)) ?? new DesignationsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, DesignationsModel, string)> Post(DesignationsModel obj)
    {

        try
        {
            string SQLDuplicate = $@"SELECT * FROM designations WHERE UPPER(DesignationName) = '{obj.DesignationName!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO designations 
			(
				OrganizationId, 
				DesignationName, 
				ParentId, 
				DepartmentId, 
				Description, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.DesignationName!.ToUpper()}', 
				{obj.ParentId},
				{obj.DepartmentId},
				'{obj.Description!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}'
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<DesignationsModel> Output = new List<DesignationsModel>();
                var result = await Search($"a.id={res.Item2}")!;
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

    public async Task<(bool, DesignationsModel, string)> Put(DesignationsModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM designations WHERE UPPER(DesignationName) = '{obj.DesignationName!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE designations SET 
					OrganizationId = {obj.OrganizationId}, 
					DesignationName = '{obj.DesignationName!.ToUpper()}', 
					ParentId = {obj.ParentId}, 
					DepartmentId = {obj.DepartmentId}, 
					Description = '{obj.Description!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}'
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<DesignationsModel> Output = new List<DesignationsModel>();
                var result = await Search($"a.id={obj.Id}")!;
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
        return await dapper.Delete("designations", id);
    }

    public async Task<(bool, string)> SoftDelete(DesignationsModel obj)
    {
        string SQLUpdate = $@"UPDATE designations SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IDepartmentsService
{
    Task<(bool, List<DepartmentsModel>)>? Search(string Criteria = "");
    Task<(bool, DepartmentsModel?)>? Get(int id);
    Task<(bool, DepartmentsModel, string)> Post(DepartmentsModel obj);
    Task<(bool, string)> Put(DepartmentsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(DepartmentsModel obj);
}


public class DepartmentsService : IDepartmentsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<DepartmentsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                          a.*,
                          b.DepartmentName as ParentDepartment
                        FROM departments as a
                        LEFT JOIN departments as b on b.Id=a.ParentId
                        WHERE a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by a.Id Desc";

        List<DepartmentsModel> result = (await dapper.SearchByQuery<DepartmentsModel>(SQL)) ?? new List<DepartmentsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, DepartmentsModel?)>? Get(int id)
    {
        DepartmentsModel result = (await dapper.SearchByID<DepartmentsModel>("departments", id)) ?? new DepartmentsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, DepartmentsModel, string)> Post(DepartmentsModel obj)
    {

        try
        {
            string SQLDuplicate = $@"SELECT * FROM departments WHERE UPPER(DepartmentName) = '{obj.DepartmentName!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO departments 
			(
				OrganizationId, 
				DepartmentName, 
				ParentId, 
				Description, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.DepartmentName!.ToUpper()}', 
				{obj.ParentId},
				'{obj.Description!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<DepartmentsModel> Output = new List<DepartmentsModel>();
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

    public async Task<(bool, string)> Put(DepartmentsModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM departments WHERE UPPER(DepartmentName) = '{obj.DepartmentName!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE departments SET 
					OrganizationId = {obj.OrganizationId}, 
					DepartmentName = '{obj.DepartmentName!.ToUpper()}', 
					ParentId = {obj.ParentId}, 
					Description = '{obj.Description!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE  Id = {obj.Id};";

            return await dapper.Update(SQLUpdate, SQLDuplicate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("departments", id);
    }

    public async Task<(bool, string)> SoftDelete(DepartmentsModel obj)
    {
        string SQLUpdate = $@"UPDATE departments SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



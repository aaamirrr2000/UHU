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

public interface ICategoriesService
{
    Task<(bool, List<CategoriesModel>)>? Search(string Criteria = "");
    Task<(bool, CategoriesModel?)>? Get(int id);
    Task<(bool, CategoriesModel, string)> Post(CategoriesModel obj);
    Task<(bool, string)> Put(CategoriesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(CategoriesModel obj);
}
public class CategoriesService : ICategoriesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<CategoriesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Categories Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id";

        List<CategoriesModel> result = (await dapper.SearchByQuery<CategoriesModel>(SQL)) ?? new List<CategoriesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, CategoriesModel?)>? Get(int id)
    {
        CategoriesModel result = (await dapper.SearchByID<CategoriesModel>("Categories", id)) ?? new CategoriesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, List<CategoriesModel>)>? SearchCategoriesHavingSomeItems(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM CategoriesHavingSomeItems";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " where " + Criteria;

        SQL += " Order by Name";

        List<CategoriesModel> result = (await dapper.SearchByQuery<CategoriesModel>(SQL)) ?? new List<CategoriesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, CategoriesModel, string)> Post(CategoriesModel obj)
    {

        try
        {

            string Code = dapper.GetCode("", "Categories", "Code")!;
            string SQLDuplicate = $@"SELECT * FROM Categories WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Categories 
			(
				OrganizationId, 
				Code,
 				Name, 
                CategoryType,
				ParentId, 
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
				'{obj.CategoryType!.ToUpper()}', 
				{obj.ParentId},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<CategoriesModel> Output = new List<CategoriesModel>();
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

    public async Task<(bool, string)> Put(CategoriesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Categories WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE Categories SET 
					OrganizationId = {obj.OrganizationId}, 
					Code = '{obj.Code!.ToUpper()}', 
 					Name = '{obj.Name!.ToUpper()}',
                    CategoryType = '{obj.CategoryType!.ToUpper()}',
					ParentId = {obj.ParentId}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
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
        return await dapper.Delete("Categories", id);
    }

    public async Task<(bool, string)> SoftDelete(CategoriesModel obj)
    {
        string SQLUpdate = $@"UPDATE Categories SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

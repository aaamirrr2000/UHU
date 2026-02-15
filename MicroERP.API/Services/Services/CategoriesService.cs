using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroERP.Shared.Helper;
using MicroERP.Shared.Models;
using MicroERP.API.Services;
using MicroERP.API.Helper;

namespace MicroERP.API.Services;

public interface ICategoriesService
{
    Task<(bool, List<CategoriesModel>)>? Search(string Criteria = "");
    Task<(bool, CategoriesModel?)>? Get(int id);
    Task<(bool, CategoriesModel, string)> Post(CategoriesModel obj);
    Task<(bool, CategoriesModel, string)> Put(CategoriesModel obj);
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
        string SQL = $@"SELECT * FROM vw_CategoriesHavingSomeItems";

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
            string SQLDuplicate = $@"SELECT * FROM Categories WHERE UPPER(code) = '{obj.Code!.ToUpper()}' AND IsSoftDeleted = 0;";
            string taxRuleIdValue = obj.TaxRuleId.HasValue && obj.TaxRuleId.Value > 0 ? obj.TaxRuleId.Value.ToString() : "NULL";
            string SQLInsert = $@"INSERT INTO Categories 
			(
				OrganizationId, 
				Code,
 				Name, 
                CategoryType,
				ParentId, 
				TaxRuleId,
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
				{taxRuleIdValue},
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
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, CategoriesModel, string)> Put(CategoriesModel obj)

    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Categories WHERE UPPER(code) = '{obj.Code!.ToUpper()}' AND ID != {obj.Id} AND IsSoftDeleted = 0;";
            string taxRuleIdValue = obj.TaxRuleId.HasValue && obj.TaxRuleId.Value > 0 ? obj.TaxRuleId.Value.ToString() : "NULL";
            string SQLUpdate = $@"UPDATE Categories SET 
					OrganizationId = {obj.OrganizationId}, 
					Code = '{obj.Code!.ToUpper()}', 
 					Name = '{obj.Name!.ToUpper()}',
                    CategoryType = '{obj.CategoryType!.ToUpper()}',
					ParentId = {obj.ParentId}, 
					TaxRuleId = {taxRuleIdValue},
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<CategoriesModel> Output = new List<CategoriesModel>();
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
            return (false, null!, ex.Message);
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


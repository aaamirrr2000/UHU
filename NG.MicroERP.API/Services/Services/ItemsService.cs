using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;
using System.Text.Json;

namespace NG.MicroERP.API.Services;


public interface IItemsService
{
    Task<(bool, List<ItemsModel>)>? Search(string Criteria = "");
    Task<(bool, ItemsModel?)>? Get(int id);
    Task<(bool, ItemsModel, string)> Post(ItemsModel obj);
    Task<(bool, string)> Put(ItemsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ItemsModel obj);
    Task<(bool, string)> ServingSize(ItemsModel obj);
}

public class ItemsService : IItemsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ItemsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Items Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<ItemsModel> result = (await dapper.SearchByQuery<ItemsModel>(SQL)) ?? new List<ItemsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, ItemsModel?)>? Get(int id)
    {
        ItemsModel result = (await dapper.SearchByID<ItemsModel>("Items", id)) ?? new ItemsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, ItemsModel, string)> Post(ItemsModel obj)
    {

        try
        {

            string Code = dapper.GetCode("LOC", "Items", "Code")!;
            string SQLDuplicate = $@"SELECT * FROM Items WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Items 
			(
				OrganizationId, 
				Pic, 
				Code, 
				Name, 
				Description, 
				MinQty, 
				MaxQty, 
				Discount, 
				CostPrice, 
				RetailPrice, 
				CategoriesId, 
				StockType, 
				Unit, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.Pic!.ToUpper()}', 
				'{obj.Code!.ToUpper()}', 
				'{obj.Name!.ToUpper()}', 
				'{obj.Description!.ToUpper()}', 
				{obj.MinQty},
				{obj.MaxQty},
				{obj.Discount},
				{obj.CostPrice},
				{obj.RetailPrice},
				{obj.CategoriesId},
				'{obj.StockType!.ToUpper()}', 
				'{obj.Unit!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ItemsModel> Output = new List<ItemsModel>();
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

    public async Task<(bool, string)> Put(ItemsModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Items WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE Items SET 
					OrganizationId = {obj.OrganizationId}, 
					Pic = '{obj.Pic!.ToUpper()}', 
					Code = '{obj.Code!.ToUpper()}', 
					Name = '{obj.Name!.ToUpper()}', 
					Description = '{obj.Description!.ToUpper()}', 
					MinQty = {obj.MinQty}, 
					MaxQty = {obj.MaxQty}, 
					Discount = {obj.Discount}, 
					CostPrice = {obj.CostPrice}, 
					RetailPrice = {obj.RetailPrice}, 
					CategoriesId = {obj.CategoriesId}, 
					StockType = '{obj.StockType!.ToUpper()}', 
					Unit = '{obj.Unit!.ToUpper()}', 
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

    public async Task<(bool, string)> ServingSize(ItemsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Items SET 
					ServingSize = '{obj.ServingSize}', 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
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
        return await dapper.Delete("Items", id);
    }

    public async Task<(bool, string)> SoftDelete(ItemsModel obj)
    {
        string SQLUpdate = $@"UPDATE Items SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
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
    Task<(bool, List<ItemsModel>)>? Search(string Criteria = "", string TopN = "");
    Task<(bool, List<ItemsModel>)>? SearchRecent(string TopN = "");
    Task<(bool, ItemsModel?)>? Get(int id);
    Task<(bool, ItemsModel, string)> Post(ItemsModel obj);
    Task<(bool, ItemsModel, string)> Put(ItemsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ItemsModel obj);
    Task<(bool, ItemsModel, string)> ServingSize(ItemsModel obj);
}

public class ItemsService : IItemsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ItemsModel>)>? Search(string Criteria = "", string TopN="")
    {
        string SQL = string.Empty;
        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL = $@"SELECT
                    Items.Id,
                    Items.OrganizationId,
                    Items.Pic,
                    Items.Code,
                    Items.HsCode,
                    Items.Name,
                    Items.Description,
                    Items.MinQty,
                    Items.MaxQty,
                    Items.ReorderQty,
                    Items.Discount,
                    Items.Tax,
                    Items.CostPrice,
                    Items.RetailPrice,
                    Items.CategoriesId,
                    Categories.Code AS CategoryCode,
                    Categories.Name AS CategoryName,
                    Items.StockType,
                    Items.Unit,
                    Items.ServingSize,
                    Items.IsFavItem,
                    Items.IsActive,
                    Items.CreatedBy,
                    Items.CreatedOn,
                    Items.CreatedFrom,
                    Items.UpdatedBy,
                    Items.UpdatedOn,
                    Items.UpdatedFrom,
                    Items.IsInventoryItem,
                    AVG(BillDetail.Rating) AS Rating
                FROM Items
                LEFT JOIN Categories ON Categories.Id = Items.CategoriesId
                LEFT JOIN BillDetail ON Items.Id = BillDetail.ItemId
                WHERE Items.IsSoftDeleted = 0 {SQL}
                GROUP BY
                    Items.Id,
                    Items.OrganizationId,
                    Items.Pic,
                    Items.Code,
                    Items.HsCode,
                    Items.Name,
                    Items.Description,
                    Items.MinQty,
                    Items.MaxQty,
                    Items.ReorderQty,
                    Items.Discount,
                    Items.Tax,
                    Items.CostPrice,
                    Items.RetailPrice,
                    Items.CategoriesId,
                    Categories.Code,
                    Categories.Name,
                    Items.StockType,
                    Items.Unit,
                    Items.ServingSize,
                    Items.IsFavItem,
                    Items.IsActive,
                    Items.CreatedBy,
                    Items.CreatedOn,
                    Items.CreatedFrom,
                    Items.UpdatedBy,
                    Items.UpdatedOn,
                    Items.UpdatedFrom,
                    Items.IsInventoryItem
            ";


        SQL += " Order by Items.IsFavItem desc, Items.Name";

        List<ItemsModel> result = (await dapper.SearchByQuery<ItemsModel>(SQL)) ?? new List<ItemsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, List<ItemsModel>)>? SearchRecent(string TopN = "")
    {
        string SQL = $@"SELECT 
                          {TopN}
                          Items.Id,
                          Items.OrganizationId,
                          Items.Pic,
                          Items.Code,
                          Items.HsCode,
                          Items.Name,
                          Items.Description,
                          Items.MinQty,
                          Items.MaxQty,
                          Items.ReorderQty,
                          Items.Discount,
                          Items.CostPrice,
                          Items.RetailPrice,
                          Items.CategoriesId,
                          Categories.Code as CategoryCode,
                          Categories.Name as CategoryName,
                          Items.StockType,
                          Items.SaleType,
                          Items.Unit,
                          Items.ServingSize,
                          Items.IsFavItem,
                          Items.IsActive,
                          Items.CreatedBy,
                          Items.CreatedOn,
                          Items.CreatedFrom,
                          Items.UpdatedBy,
                          Items.UpdatedOn,
                          Items.UpdatedFrom,
                          Items.IsInventoryItem,
                          ISNULL(Itm.Quantity, 0) as Quantity
                    FROM Items
                    LEFT JOIN Categories ON Categories.Id = Items.CategoriesId
                    LEFT JOIN (
                        SELECT ItemId, SUM(qty) as Quantity 
                        FROM BillDetail 
                        GROUP BY ItemId
                    ) as Itm ON Itm.ItemId = Items.Id
                    WHERE Items.IsSoftDeleted = 0
                    ORDER BY ISNULL(Itm.Quantity, 0) DESC, Items.IsFavItem DESC, Items.Id;

                ";

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
            string Code = dapper.GetCode("", "Items", "Code", 12)!;
            string SQLDuplicate = $@"SELECT * FROM Items WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Items 
			(
				OrganizationId, 
				Pic, 
				Code, 
				HsCode, 
				Name, 
				Description, 
				MinQty, 
				MaxQty, 
				ReorderQty, 
				Discount, 
				Tax, 
				CostPrice, 
				RetailPrice, 
				CategoriesId, 
				StockType, 
				Unit, 
				IsInventoryItem, 
				IsFavItem, 
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
				'{Code}', 
				'{obj.HsCode!.ToUpper()}', 
				'{obj.Name!.ToUpper()}', 
				'{obj.Description!.ToUpper()}', 
				{obj.MinQty},
				{obj.MaxQty},
				{obj.ReorderQty},
				{obj.Discount},
				{obj.Tax},
				{obj.CostPrice},
				{obj.RetailPrice},
				{obj.CategoriesId},
				'{obj.StockType!.ToUpper()}', 
				'{obj.Unit!.ToUpper()}', 
				{obj.IsInventoryItem},
				{obj.IsFavItem},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
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

    public async Task<(bool, ItemsModel, string)> Put(ItemsModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Items WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE Items SET 
					OrganizationId = {obj.OrganizationId}, 
					Pic = '{obj.Pic!.ToUpper()}', 
					Code = '{obj.Code!.ToUpper()}', 
					HsCode = '{obj.HsCode!.ToUpper()}', 
					Name = '{obj.Name!.ToUpper()}', 
					Description = '{obj.Description!.ToUpper()}', 
					MinQty = {obj.MinQty}, 
					MaxQty = {obj.MaxQty}, 
					ReorderQty = {obj.ReorderQty}, 
					Discount = {obj.Discount}, 
					Tax = {obj.Tax}, 
					CostPrice = {obj.CostPrice}, 
					RetailPrice = {obj.RetailPrice}, 
					CategoriesId = {obj.CategoriesId}, 
					StockType = '{obj.StockType!.ToUpper()}', 
					Unit = '{obj.Unit!.ToUpper()}', 
					ServingSize = '{obj.ServingSize!.ToUpper()}', 
					IsInventoryItem = {obj.IsInventoryItem}, 
					IsFavItem = {obj.IsFavItem}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ItemsModel> Output = new List<ItemsModel>();
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

    public async Task<(bool, ItemsModel, string)> ServingSize(ItemsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Items SET 
					ServingSize = '{obj.ServingSize}', 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<ItemsModel> Output = new List<ItemsModel>();
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
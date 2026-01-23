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
using Serilog;

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
        try
        {
            string SQL = string.Empty;
            if (!string.IsNullOrWhiteSpace(Criteria))
                SQL += " and " + Criteria;

            // Removed GROUP BY as it's not needed without aggregate functions
            // This should improve query performance
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
                        Items.DefaultDiscount,
                        Items.CostPrice,
                        Items.BasePrice,
                        Items.CategoryId,
                        Categories.Code AS CategoryCode,
                        Categories.Name AS CategoryName,
                        Items.StockType,
                        Items.Unit,
                        Items.TaxRuleId,
                        Items.ExpenseAccountId,
                        Items.RevenueAccountId,
                        Items.ServingSize,
                        Items.IsFavorite,
                        Items.IsActive,
                        Items.CreatedBy,
                        Items.CreatedOn,
                        Items.CreatedFrom,
                        Items.UpdatedBy,
                        Items.UpdatedOn,
                        Items.UpdatedFrom
                    FROM Items
                    LEFT JOIN Categories ON Categories.Id = Items.CategoryId
                    WHERE Items.IsSoftDeleted = 0 {SQL}
                    ORDER BY Items.IsFavorite DESC, Items.Name";

            List<ItemsModel> result = (await dapper.SearchByQuery<ItemsModel>(SQL)) ?? new List<ItemsModel>();

            if (result == null || result.Count == 0)
                return (false, new List<ItemsModel>());
            else
                return (true, result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ItemsService.Search Error");
            return (false, new List<ItemsModel>());
        }
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
                          Items.DefaultDiscount,
                          Items.CostPrice,
                          Items.BasePrice,
                          Items.CategoryId,
                          Categories.Code as CategoryCode,
                          Categories.Name as CategoryName,
                          Items.StockType,
                          Items.SaleType,
                          Items.Unit,
                          Items.TaxRuleId,
                          Items.ExpenseAccountId,
                          Items.RevenueAccountId,
                          Items.ServingSize,
                          Items.IsFavorite,
                          Items.IsActive,
                          Items.CreatedBy,
                          Items.CreatedOn,
                          Items.CreatedFrom,
                          Items.UpdatedBy,
                          Items.UpdatedOn,
                          Items.UpdatedFrom
                          ISNULL(Itm.Quantity, 0) as Quantity
                    FROM Items
                    LEFT JOIN Categories ON Categories.Id = Items.CategoryId
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
            // TaxRuleId is optional - can be NULL

            string Code = dapper.GetCode("", "Items", "Code", 12)!;
            string taxRuleIdValue = (obj.TaxRuleId.HasValue && obj.TaxRuleId.Value > 0) ? obj.TaxRuleId.Value.ToString() : "NULL";
            string expenseAccountIdValue = (obj.ExpenseAccountId.HasValue && obj.ExpenseAccountId.Value > 0) ? obj.ExpenseAccountId.Value.ToString() : "NULL";
            string revenueAccountIdValue = (obj.RevenueAccountId.HasValue && obj.RevenueAccountId.Value > 0) ? obj.RevenueAccountId.Value.ToString() : "NULL";
            string SQLDuplicate = $@"SELECT * FROM Items WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"
                                INSERT INTO Items 
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
                                    DefaultDiscount, 
                                    CostPrice, 
                                    BasePrice, 
                                    CategoryId, 
                                    StockType, 
                                    Unit, 
                                    TaxRuleId,
                                    ExpenseAccountId,
                                    RevenueAccountId,
                                    IsFavorite, 
                                    IsActive, 
                                    CreatedBy, 
                                    CreatedOn, 
                                    CreatedFrom
                                ) 
                                VALUES 
                                (
                                    {obj.OrganizationId},
                                    '{obj.Pic?.ToUpper() ?? "null"}', 
                                    '{Code?.ToUpper() ?? "null"}', 
                                    '{obj.HsCode?.ToUpper() ?? "null"}', 
                                    '{obj.Name?.ToUpper() ?? "null"}', 
                                    '{obj.Description?.ToUpper() ?? "null"}', 
                                    {obj.MinQty},
                                    {obj.MaxQty},
                                    {obj.ReorderQty},
                                    {obj.DefaultDiscount},
                                    {obj.CostPrice},
                                    {obj.BasePrice},
                                    {obj.CategoryId},
                                    '{obj.StockType?.ToUpper() ?? "null"}', 
                                    '{obj.Unit?.ToUpper() ?? "null"}', 
                                    {taxRuleIdValue},
                                    {expenseAccountIdValue},
                                    {revenueAccountIdValue},
                                    {obj.IsFavorite},
                                    {obj.IsActive},
                                    {obj.CreatedBy},
                                    '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                                    '{obj.CreatedFrom?.ToUpper() ?? "null"}'
                                );";


            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ItemsModel> Output = new List<ItemsModel>();
                var result = await Search($"Items.id={res.Item2}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                // Check if it's a duplicate or other error
                if (!string.IsNullOrEmpty(res.Item3) && !res.Item3.Contains("Duplicate"))
                {
                    return (false, null!, $"Failed to create item '{obj.Name}'. {res.Item3}");
                }
                
                // Check for duplicate Item Code
                var existingCode = await dapper.SearchByQuery<dynamic>($"SELECT * FROM Items WHERE UPPER(Code) = '{obj.Code!.ToUpper()}' AND IsSoftDeleted = 0", "Default");
                if (existingCode != null && existingCode.Any())
                {
                    return (false, null!, $"Item Code '{obj.Code}' already exists. Please use a different Item Code.");
                }
                
                return (false, null!, $"Item '{obj.Name}' could not be created. A duplicate record was found (Item Code '{obj.Code}' already exists).");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ItemsService.Post Error for Item: {Name}, Code: {Code}", obj.Name, obj.Code);
            return (false, null!, $"Failed to create item '{obj.Name}' (Code: {obj.Code}): {ex.Message}");
        }
    }

    public async Task<(bool, ItemsModel, string)> Put(ItemsModel obj)
    {
        try
        {
            // TaxRuleId is optional - can be NULL

            string taxRuleIdValueUpdate = (obj.TaxRuleId.HasValue && obj.TaxRuleId.Value > 0) ? obj.TaxRuleId.Value.ToString() : "NULL";
            string expenseAccountIdValueUpdate = (obj.ExpenseAccountId.HasValue && obj.ExpenseAccountId.Value > 0) ? obj.ExpenseAccountId.Value.ToString() : "NULL";
            string revenueAccountIdValueUpdate = (obj.RevenueAccountId.HasValue && obj.RevenueAccountId.Value > 0) ? obj.RevenueAccountId.Value.ToString() : "NULL";
            string SQLDuplicate = $@"SELECT * FROM Items WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"
                    UPDATE Items SET 
                        OrganizationId = {obj.OrganizationId}, 
                        Pic = '{obj.Pic?.ToUpper() ?? "null"}', 
                        Code = '{obj.Code?.ToUpper() ?? "null"}', 
                        HsCode = '{obj.HsCode ?? "null"}', 
                        Name = '{obj.Name?.ToUpper() ?? "null"}', 
                        Description = '{obj.Description?.ToUpper() ?? "null"}', 
                        MinQty = {obj.MinQty}, 
                        MaxQty = {obj.MaxQty}, 
                        ReorderQty = {obj.ReorderQty}, 
                        DefaultDiscount = {obj.DefaultDiscount}, 
                        CostPrice = {obj.CostPrice}, 
                        BasePrice = {obj.BasePrice}, 
                        CategoryId = {obj.CategoryId}, 
                        StockType = '{obj.StockType?.ToUpper() ?? "null"}', 
                        Unit = '{obj.Unit?.ToUpper() ?? "null"}',
                        TaxRuleId = {taxRuleIdValueUpdate}, 
                        ExpenseAccountId = {expenseAccountIdValueUpdate},
                        RevenueAccountId = {revenueAccountIdValueUpdate},
                        ServingSize = '{obj.ServingSize?.ToUpper() ?? "null"}',
                        IsFavorite = {obj.IsFavorite}, 
                        IsActive = {obj.IsActive}, 
                        UpdatedBy = {obj.UpdatedBy}, 
                        UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
                        UpdatedFrom = '{obj.UpdatedFrom?.ToUpper() ?? "null"}'
                    WHERE Id = {obj.Id};";


            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ItemsModel> Output = new List<ItemsModel>();
                var result = await Search($"Items.id={obj.Id}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                // Check if it's a duplicate or other error
                if (!string.IsNullOrEmpty(res.Item2) && !res.Item2.Contains("Duplicate") && !res.Item2.Contains("Record Not Saved"))
                {
                    return (false, null!, $"Failed to update item (ID: {obj.Id}) '{obj.Name}'. {res.Item2}");
                }
                
                if (!string.IsNullOrEmpty(res.Item2) && res.Item2.Contains("Record Not Saved"))
                {
                    return (false, null!, $"Item record (ID: {obj.Id}) '{obj.Name}' could not be updated. The record may not exist, no changes were made, or the update query failed.");
                }
                
                // Check for duplicate Item Code
                var existingCode = await dapper.SearchByQuery<dynamic>($"SELECT * FROM Items WHERE UPPER(Code) = '{obj.Code!.ToUpper()}' AND ID != {obj.Id} AND IsSoftDeleted = 0", "Default");
                if (existingCode != null && existingCode.Any())
                {
                    return (false, null!, $"Item Code '{obj.Code}' is already assigned to another item (ID: {existingCode.FirstOrDefault()?.Id}). Please use a different Item Code.");
                }
                
                return (false, null!, $"Item record (ID: {obj.Id}) '{obj.Name}' could not be updated. A duplicate record was found (Item Code '{obj.Code}' already exists).");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, $"Failed to update item (ID: {obj.Id}) '{obj.Name}' (Code: {obj.Code}): {ex.Message}");
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
            return (false, null!, ex.Message);
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
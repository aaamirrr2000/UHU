using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroERP.Shared.Helper;
using MicroERP.Shared.Models;
using MicroERP.API.Services;
using MicroERP.API.Helper;
using Serilog;

namespace MicroERP.API.Services;

public interface IInventoryService
{
    Task<(bool, List<InventoryModel>)>? Search(string Criteria = "");
    Task<(bool, InventoryModel?)>? Get(int id);
    Task<(bool, InventoryModel, string)> Post(InventoryModel obj);
    Task<(bool, InventoryModel, string)> Put(InventoryModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(InventoryModel obj);
}

public class InventoryService : IInventoryService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<InventoryModel>)>? Search(string Criteria = "")
    {
        // Use view for calculated inventory, fallback to table if view doesn't exist
        string SQL = $@"SELECT 
                        ISNULL(v.OrganizationId, inv.OrganizationId) as OrganizationId,
                        ISNULL(v.LocationId, inv.LocationId) as LocationId,
                        ISNULL(v.ItemId, inv.ItemId) as ItemId,
                        ISNULL(v.StockCondition, inv.StockCondition) as StockCondition,
                        ISNULL(v.Quantity, inv.Quantity) as Quantity,
                        ISNULL(v.AverageCost, inv.AverageCost) as AverageCost,
                        ISNULL(v.LastMovementDate, inv.LastMovementDate) as LastMovementDate,
                        l.Name as LocationName, 
                        i.Code as ItemCode, 
                        i.Name as ItemName,
                        inv.ReservedQuantity,
                        inv.LastCost,
                        inv.ReorderLevel,
                        inv.MaxLevel
                        FROM Inventory as inv
                        LEFT JOIN vw_Inventory as v ON v.OrganizationId = inv.OrganizationId 
                            AND v.LocationId = inv.LocationId 
                            AND v.ItemId = inv.ItemId 
                            AND v.StockCondition = inv.StockCondition
                        LEFT JOIN Locations as l on l.Id = ISNULL(v.LocationId, inv.LocationId)
                        LEFT JOIN Items as i on i.Id = ISNULL(v.ItemId, inv.ItemId)
                        Where inv.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by l.Name, i.Name";

        List<InventoryModel> result = (await dapper.SearchByQuery<InventoryModel>(SQL)) ?? new List<InventoryModel>();

        if (result == null || result.Count == 0)
            return (false, new List<InventoryModel>());
        else
            return (true, result);
    }

    public async Task<(bool, InventoryModel?)>? Get(int id)
    {
        string SQL = $@"SELECT inv.*, l.Name as LocationName, i.Code as ItemCode, i.Name as ItemName
                       FROM Inventory as inv
                       LEFT JOIN Locations as l on l.Id=inv.LocationId
                       LEFT JOIN Items as i on i.Id=inv.ItemId
                       Where inv.Id={id} AND inv.IsSoftDeleted=0";

        InventoryModel result = (await dapper.SearchByQuery<InventoryModel>(SQL))?.FirstOrDefault() ?? new InventoryModel();
        
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    // Inventory is maintained automatically through transactions (Purchase Invoices, Sales Invoices, Stock Movements)
    // Direct insert/update is disabled - use transactions to update inventory
    public async Task<(bool, InventoryModel, string)> Post(InventoryModel obj)
    {
        return (false, null!, "Inventory cannot be created directly. Use Purchase Invoices to receive stock or Stock Movements to transfer stock.");
    }

    public async Task<(bool, InventoryModel, string)> Put(InventoryModel obj)
    {
        // Allow updating reorder levels and max levels only
        try
        {
            string SQLUpdate = $@"UPDATE Inventory SET 
					ReorderLevel = {obj.ReorderLevel},
					MaxLevel = {obj.MaxLevel},
					ReservedQuantity = {obj.ReservedQuantity},
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}' 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                return (true, obj, "OK");
            }
            else
                return (false, null!, res.Item2 ?? "Failed to update inventory settings.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "InventoryService Put Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Inventory", id);
    }

    public async Task<(bool, string)> SoftDelete(InventoryModel obj)
    {
        string SQLUpdate = $@"UPDATE Inventory SET 
					UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
					UpdatedBy = {obj.UpdatedBy},
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



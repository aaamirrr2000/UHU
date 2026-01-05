using NG.MicroERP.API.Helper;
using Serilog;

namespace NG.MicroERP.API.Helper;

public static class InventoryHelper
{
    /// <summary>
    /// Reverses inventory impact from an invoice (used when updating invoices)
    /// </summary>
    public static async Task ReverseInventoryFromInvoice(DapperFunctions dapper, int invoiceId, int organizationId, int locationId, string invoiceType)
    {
        try
        {
            // Get invoice details
            string sqlDetails = $@"SELECT id.ItemId, id.Qty, id.UnitPrice, ISNULL(id.StockCondition, 'NEW') as StockCondition
                                  FROM InvoiceDetail id
                                  INNER JOIN Invoice i ON i.Id = id.InvoiceId
                                  WHERE i.Id = {invoiceId} 
                                  AND i.InvoiceType = '{invoiceType}'
                                  AND i.IsSoftDeleted = 0
                                  AND id.IsSoftDeleted = 0";

            var details = await dapper.SearchByQuery<dynamic>(sqlDetails) ?? new List<dynamic>();

            foreach (var detail in details)
            {
                int itemId = Convert.ToInt32(detail.ItemId);
                double qty = Convert.ToDouble(detail.Qty);
                double unitPrice = Convert.ToDouble(detail.UnitPrice);
                string stockCondition = detail.StockCondition?.ToString() ?? "NEW";

                // Reverse the impact: if it was purchase (stock in), reverse by decreasing; if sale (stock out), reverse by increasing
                if (invoiceType?.ToUpper() == "PURCHASE")
                {
                    // Reverse purchase: decrease quantity
                    string reverseSql = $@"UPDATE Inventory SET 
                                          Quantity = Quantity - {qty},
                                          LastMovementDate = GETDATE(),
                                          UpdatedOn = GETDATE()
                                          WHERE OrganizationId = {organizationId} 
                                          AND LocationId = {locationId} 
                                          AND ItemId = {itemId} 
                                          AND StockCondition = '{stockCondition}'
                                          AND Quantity >= {qty}";
                    await dapper.ExecuteQuery(reverseSql);
                }
                else if (invoiceType?.ToUpper() == "SALE")
                {
                    // Reverse sale: increase quantity
                    string checkSql = $@"SELECT * FROM Inventory 
                                        WHERE OrganizationId = {organizationId} 
                                        AND LocationId = {locationId} 
                                        AND ItemId = {itemId} 
                                        AND StockCondition = '{stockCondition}'";

                    var existing = await dapper.SearchByQuery<dynamic>(checkSql);

                    if (existing != null && existing.Any())
                    {
                        string reverseSql = $@"UPDATE Inventory SET 
                                              Quantity = Quantity + {qty},
                                              LastMovementDate = GETDATE(),
                                              UpdatedOn = GETDATE()
                                              WHERE OrganizationId = {organizationId} 
                                              AND LocationId = {locationId} 
                                              AND ItemId = {itemId} 
                                              AND StockCondition = '{stockCondition}'";
                        await dapper.ExecuteQuery(reverseSql);
                    }
                    else
                    {
                        // Insert if doesn't exist (shouldn't happen, but handle it)
                        string insertSql = $@"INSERT INTO Inventory 
                                            (OrganizationId, LocationId, ItemId, StockCondition, Quantity, 
                                             AverageCost, LastCost, LastMovementDate, CreatedOn, IsSoftDeleted)
                                            VALUES 
                                            ({organizationId}, {locationId}, {itemId}, '{stockCondition}', {qty},
                                             {unitPrice}, {unitPrice}, GETDATE(), GETDATE(), 0)";
                        await dapper.ExecuteQuery(insertSql);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reversing inventory from invoice {InvoiceId}", invoiceId);
        }
    }

    /// <summary>
    /// Checks stock availability before creating sale invoice
    /// </summary>
    public static async Task<(bool, string)> CheckStockAvailability(DapperFunctions dapper, int organizationId, int locationId, List<(int ItemId, double Qty, string StockCondition)> items)
    {
        try
        {
            foreach (var item in items)
            {
                string checkSql = $@"SELECT Quantity FROM Inventory 
                                    WHERE OrganizationId = {organizationId} 
                                    AND LocationId = {locationId} 
                                    AND ItemId = {item.ItemId} 
                                    AND StockCondition = '{item.StockCondition ?? "NEW"}'";

                var result = await dapper.SearchByQuery<dynamic>(checkSql);
                double availableQty = 0;

                if (result != null && result.Any())
                {
                    availableQty = Convert.ToDouble(result.First().Quantity);
                }

                if (availableQty < item.Qty)
                {
                    string itemName = await GetItemName(dapper, item.ItemId);
                    return (false, $"Insufficient stock for {itemName}. Available: {availableQty:N2}, Required: {item.Qty:N2}");
                }
            }
            return (true, "");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking stock availability");
            return (false, $"Error checking stock: {ex.Message}");
        }
    }

    private static async Task<string> GetItemName(DapperFunctions dapper, int itemId)
    {
        try
        {
            var result = await dapper.SearchByQuery<dynamic>($"SELECT Name FROM Items WHERE Id = {itemId}");
            if (result != null && result.Any())
                return result.First().Name?.ToString() ?? $"Item {itemId}";
            return $"Item {itemId}";
        }
        catch
        {
            return $"Item {itemId}";
        }
    }

    /// <summary>
    /// Updates inventory when a purchase invoice is saved (stock in)
    /// </summary>
    public static async Task UpdateInventoryFromPurchaseInvoice(DapperFunctions dapper, int invoiceId, int organizationId, int locationId)
    {
        try
        {
            // Get invoice details
            string sqlDetails = $@"SELECT id.ItemId, id.Qty, id.UnitPrice, ISNULL(id.StockCondition, 'NEW') as StockCondition
                                  FROM InvoiceDetail id
                                  INNER JOIN Invoice i ON i.Id = id.InvoiceId
                                  WHERE i.Id = {invoiceId} 
                                  AND i.InvoiceType = 'PURCHASE'
                                  AND i.IsSoftDeleted = 0
                                  AND id.IsSoftDeleted = 0";

            var details = await dapper.SearchByQuery<dynamic>(sqlDetails) ?? new List<dynamic>();

            foreach (var detail in details)
            {
                int itemId = Convert.ToInt32(detail.ItemId);
                double qty = Convert.ToDouble(detail.Qty);
                double unitPrice = Convert.ToDouble(detail.UnitPrice);
                string stockCondition = detail.StockCondition?.ToString() ?? "NEW";

                // Check if inventory record exists
                string checkSql = $@"SELECT * FROM Inventory 
                                    WHERE OrganizationId = {organizationId} 
                                    AND LocationId = {locationId} 
                                    AND ItemId = {itemId} 
                                    AND StockCondition = '{stockCondition}'";

                var existing = await dapper.SearchByQuery<dynamic>(checkSql);

                if (existing != null && existing.Any())
                {
                    // Update existing inventory - calculate weighted average cost
                    string updateSql = $@"UPDATE Inventory SET 
                                        Quantity = Quantity + {qty},
                                        AverageCost = CASE 
                                            WHEN (Quantity + {qty}) > 0 
                                            THEN ((AverageCost * Quantity) + ({unitPrice} * {qty})) / (Quantity + {qty})
                                            ELSE AverageCost
                                        END,
                                        LastCost = {unitPrice},
                                        LastMovementDate = GETDATE(),
                                        UpdatedOn = GETDATE()
                                        WHERE OrganizationId = {organizationId} 
                                        AND LocationId = {locationId} 
                                        AND ItemId = {itemId} 
                                        AND StockCondition = '{stockCondition}'";
                    await dapper.ExecuteQuery(updateSql);
                }
                else
                {
                    // Insert new inventory record
                    string insertSql = $@"INSERT INTO Inventory 
                                        (OrganizationId, LocationId, ItemId, StockCondition, Quantity, 
                                         AverageCost, LastCost, LastMovementDate, CreatedOn, IsSoftDeleted)
                                        VALUES 
                                        ({organizationId}, {locationId}, {itemId}, '{stockCondition}', {qty},
                                         {unitPrice}, {unitPrice}, GETDATE(), GETDATE(), 0)";
                    await dapper.ExecuteQuery(insertSql);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating inventory from purchase invoice {InvoiceId}", invoiceId);
        }
    }

    /// <summary>
    /// Updates inventory when a sale invoice is saved (stock out)
    /// </summary>
    public static async Task UpdateInventoryFromSaleInvoice(DapperFunctions dapper, int invoiceId, int organizationId, int locationId)
    {
        try
        {
            // Get invoice details
            string sqlDetails = $@"SELECT id.ItemId, id.Qty, ISNULL(id.StockCondition, 'NEW') as StockCondition
                                  FROM InvoiceDetail id
                                  INNER JOIN Invoice i ON i.Id = id.InvoiceId
                                  WHERE i.Id = {invoiceId} 
                                  AND i.InvoiceType = 'SALE'
                                  AND i.IsSoftDeleted = 0
                                  AND id.IsSoftDeleted = 0";

            var details = await dapper.SearchByQuery<dynamic>(sqlDetails) ?? new List<dynamic>();

            foreach (var detail in details)
            {
                int itemId = Convert.ToInt32(detail.ItemId);
                double qty = Convert.ToDouble(detail.Qty);
                string stockCondition = detail.StockCondition?.ToString() ?? "NEW";

                // Update inventory (decrease quantity)
                string updateSql = $@"UPDATE Inventory SET 
                                    Quantity = Quantity - {qty},
                                    LastMovementDate = GETDATE(),
                                    UpdatedOn = GETDATE()
                                    WHERE OrganizationId = {organizationId} 
                                    AND LocationId = {locationId} 
                                    AND ItemId = {itemId} 
                                    AND StockCondition = '{stockCondition}'
                                    AND Quantity >= {qty}"; // Ensure we don't go negative

                var result = await dapper.ExecuteQuery(updateSql);
                if (!result.Item1)
                {
                    Log.Warning("Insufficient stock for ItemId {ItemId} at LocationId {LocationId}", itemId, locationId);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating inventory from sale invoice {InvoiceId}", invoiceId);
        }
    }

    /// <summary>
    /// Updates inventory when stock movement is completed
    /// </summary>
    public static async Task UpdateInventoryFromStockMovement(DapperFunctions dapper, int movementId, int organizationId, int fromLocationId, int toLocationId)
    {
        try
        {
            // Get movement details
            string sqlDetails = $@"SELECT smd.ItemId, smd.Quantity, smd.UnitCost, ISNULL(smd.StockCondition, 'NEW') as StockCondition
                                  FROM StockMovementDetails smd
                                  INNER JOIN StockMovements sm ON sm.Id = smd.MovementId
                                  WHERE sm.Id = {movementId} 
                                  AND sm.Status = 'COMPLETED'
                                  AND sm.IsSoftDeleted = 0
                                  AND smd.IsSoftDeleted = 0";

            var details = await dapper.SearchByQuery<dynamic>(sqlDetails) ?? new List<dynamic>();

            foreach (var detail in details)
            {
                int itemId = Convert.ToInt32(detail.ItemId);
                double qty = Convert.ToDouble(detail.Quantity);
                double unitCost = Convert.ToDouble(detail.UnitCost);
                string stockCondition = detail.StockCondition?.ToString() ?? "NEW";

                // Decrease inventory at FromLocation
                if (fromLocationId > 0)
                {
                    // Check if inventory exists at from location
                    string checkFromSql = $@"SELECT * FROM Inventory 
                                            WHERE OrganizationId = {organizationId} 
                                            AND LocationId = {fromLocationId} 
                                            AND ItemId = {itemId} 
                                            AND StockCondition = '{stockCondition}'";

                    var fromExists = await dapper.SearchByQuery<dynamic>(checkFromSql);

                    if (fromExists != null && fromExists.Any())
                    {
                        string decreaseSql = $@"UPDATE Inventory SET 
                                               Quantity = Quantity - {qty},
                                               LastMovementDate = GETDATE(),
                                               UpdatedOn = GETDATE()
                                               WHERE OrganizationId = {organizationId} 
                                               AND LocationId = {fromLocationId} 
                                               AND ItemId = {itemId} 
                                               AND StockCondition = '{stockCondition}'
                                               AND Quantity >= {qty}";
                        var result = await dapper.ExecuteQuery(decreaseSql);
                        if (!result.Item1)
                        {
                            Log.Warning("Insufficient stock for ItemId {ItemId} at FromLocationId {FromLocationId} in Movement {MovementId}", itemId, fromLocationId, movementId);
                        }
                    }
                    else
                    {
                        Log.Warning("No inventory found for ItemId {ItemId} at FromLocationId {FromLocationId} in Movement {MovementId}", itemId, fromLocationId, movementId);
                    }
                }

                // Increase inventory at ToLocation
                if (toLocationId > 0)
                {
                    string checkSql = $@"SELECT * FROM Inventory 
                                        WHERE OrganizationId = {organizationId} 
                                        AND LocationId = {toLocationId} 
                                        AND ItemId = {itemId} 
                                        AND StockCondition = '{stockCondition}'";

                    var existing = await dapper.SearchByQuery<dynamic>(checkSql);

                    if (existing != null && existing.Any())
                    {
                        // Update existing - calculate weighted average
                        string increaseSql = $@"UPDATE Inventory SET 
                                               Quantity = Quantity + {qty},
                                               AverageCost = CASE 
                                                   WHEN (Quantity + {qty}) > 0 
                                                   THEN ((AverageCost * Quantity) + ({unitCost} * {qty})) / (Quantity + {qty})
                                                   ELSE AverageCost
                                               END,
                                               LastCost = {unitCost},
                                               LastMovementDate = GETDATE(),
                                               UpdatedOn = GETDATE()
                                               WHERE OrganizationId = {organizationId} 
                                               AND LocationId = {toLocationId} 
                                               AND ItemId = {itemId} 
                                               AND StockCondition = '{stockCondition}'";
                        await dapper.ExecuteQuery(increaseSql);
                    }
                    else
                    {
                        // Insert new
                        string insertSql = $@"INSERT INTO Inventory 
                                            (OrganizationId, LocationId, ItemId, StockCondition, Quantity, 
                                             AverageCost, LastCost, LastMovementDate, CreatedOn, IsSoftDeleted)
                                            VALUES 
                                            ({organizationId}, {toLocationId}, {itemId}, '{stockCondition}', {qty},
                                             {unitCost}, {unitCost}, GETDATE(), GETDATE(), 0)";
                        await dapper.ExecuteQuery(insertSql);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating inventory from stock movement {MovementId}", movementId);
        }
    }
}

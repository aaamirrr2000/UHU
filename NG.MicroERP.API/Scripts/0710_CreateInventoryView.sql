-- =============================================
-- Create Inventory View
-- Calculates current inventory from all stock transactions
-- =============================================

IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_Inventory' AND schema_id = SCHEMA_ID('dbo'))
    DROP VIEW dbo.vw_Inventory;
GO

CREATE VIEW dbo.vw_Inventory
AS
WITH StockTransactions AS (
    -- Purchase Invoices (Stock In)
    SELECT 
        i.OrganizationId,
        i.LocationId,
        id.ItemId,
        ISNULL(id.StockCondition, 'NEW') AS StockCondition,
        id.Qty AS Quantity,
        id.UnitPrice AS UnitCost,
        i.TranDate AS TransactionDate,
        'PURCHASE_INVOICE' AS TransactionType,
        i.Id AS TransactionId,
        i.Code AS TransactionNo
    FROM Invoice i
    INNER JOIN InvoiceDetail id ON i.Id = id.InvoiceId
    WHERE i.InvoiceType = 'PURCHASE'
        AND i.IsSoftDeleted = 0
        AND id.IsSoftDeleted = 0
    
    UNION ALL
    
    -- Sales Invoices (Stock Out)
    SELECT 
        i.OrganizationId,
        i.LocationId,
        id.ItemId,
        ISNULL(id.StockCondition, 'NEW') AS StockCondition,
        -id.Qty AS Quantity, -- Negative for stock out
        id.UnitPrice AS UnitCost,
        i.TranDate AS TransactionDate,
        'SALE_INVOICE' AS TransactionType,
        i.Id AS TransactionId,
        i.Code AS TransactionNo
    FROM Invoice i
    INNER JOIN InvoiceDetail id ON i.Id = id.InvoiceId
    WHERE i.InvoiceType = 'SALE'
        AND i.IsSoftDeleted = 0
        AND id.IsSoftDeleted = 0
    
    UNION ALL
    
    -- Shipments (Stock In/Out)
    SELECT 
        s.OrganizationId,
        s.LocationId,
        sd.ItemId,
        ISNULL(sd.StockCondition, 'NEW') AS StockCondition,
        CASE 
            WHEN s.ShipmentType = 'INCOMING' THEN sd.Quantity
            WHEN s.ShipmentType = 'OUTGOING' THEN -sd.Quantity
            ELSE 0
        END AS Quantity,
        sd.UnitPrice AS UnitCost,
        s.ShipmentDate AS TransactionDate,
        'SHIPMENT' AS TransactionType,
        s.Id AS TransactionId,
        s.ShipmentNo AS TransactionNo
    FROM Shipments s
    INNER JOIN ShipmentDetails sd ON s.Id = sd.ShipmentId
    WHERE s.Status = 'COMPLETED'
        AND s.IsSoftDeleted = 0
        AND sd.IsSoftDeleted = 0
    
    UNION ALL
    
    -- Stock Movements (Transfer between locations - In)
    SELECT 
        sm.OrganizationId,
        sm.ToLocationId AS LocationId,
        smd.ItemId,
        ISNULL(smd.StockCondition, 'NEW') AS StockCondition,
        smd.Quantity,
        smd.UnitCost,
        sm.MovementDate AS TransactionDate,
        'STOCK_MOVEMENT_IN' AS TransactionType,
        sm.Id AS TransactionId,
        sm.MovementNo AS TransactionNo
    FROM StockMovements sm
    INNER JOIN StockMovementDetails smd ON sm.Id = smd.MovementId
    WHERE sm.Status = 'COMPLETED'
        AND sm.ToLocationId IS NOT NULL
        AND sm.IsSoftDeleted = 0
        AND smd.IsSoftDeleted = 0
    
    UNION ALL
    
    -- Stock Movements (Transfer between locations - Out)
    SELECT 
        sm.OrganizationId,
        sm.FromLocationId AS LocationId,
        smd.ItemId,
        ISNULL(smd.StockCondition, 'NEW') AS StockCondition,
        -smd.Quantity, -- Negative for stock out
        smd.UnitCost,
        sm.MovementDate AS TransactionDate,
        'STOCK_MOVEMENT_OUT' AS TransactionType,
        sm.Id AS TransactionId,
        sm.MovementNo AS TransactionNo
    FROM StockMovements sm
    INNER JOIN StockMovementDetails smd ON sm.Id = smd.MovementId
    WHERE sm.Status = 'COMPLETED'
        AND sm.FromLocationId IS NOT NULL
        AND sm.IsSoftDeleted = 0
        AND smd.IsSoftDeleted = 0
)
SELECT 
    OrganizationId,
    LocationId,
    ItemId,
    StockCondition,
    SUM(Quantity) AS Quantity,
    SUM(Quantity * UnitCost) / NULLIF(SUM(Quantity), 0) AS AverageCost,
    MAX(TransactionDate) AS LastMovementDate,
    COUNT(*) AS TransactionCount
FROM StockTransactions
GROUP BY OrganizationId, LocationId, ItemId, StockCondition;
GO

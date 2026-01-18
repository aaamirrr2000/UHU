-- =============================================
-- Stock Module - Comprehensive Inventory Management
-- Note: Using Locations table for warehouses (merged)
-- =============================================

-- =============================================
-- 1. Inventory Table (Current Stock Levels by Location/Warehouse)
-- =============================================
CREATE TABLE dbo.Inventory
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NOT NULL,
    LocationId          INT NOT NULL, -- Using LocationId instead of WarehouseId
    ItemId              INT NOT NULL,
    StockCondition      VARCHAR(50) NULL DEFAULT 'NEW', -- NEW, USED, DAMAGED, etc.
    Quantity            DECIMAL(18, 4) NOT NULL DEFAULT 0,
    ReservedQuantity    DECIMAL(18, 4) NOT NULL DEFAULT 0, -- Reserved for orders
    AvailableQuantity   AS (Quantity - ReservedQuantity) PERSISTED, -- Computed column
    AverageCost         DECIMAL(18, 4) NOT NULL DEFAULT 0,
    LastCost            DECIMAL(18, 4) NOT NULL DEFAULT 0,
    LastMovementDate    DATETIME NULL,
    ReorderLevel        DECIMAL(18, 4) NULL DEFAULT 0,
    MaxLevel             DECIMAL(18, 4) NULL DEFAULT 0,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_Inventory_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations(Id),
    CONSTRAINT FK_Inventory_Location FOREIGN KEY (LocationId) REFERENCES dbo.Locations(Id),
    CONSTRAINT FK_Inventory_Item FOREIGN KEY (ItemId) REFERENCES dbo.Items(Id),
    CONSTRAINT FK_Inventory_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Inventory_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_Inventory_Location_Item_Condition UNIQUE (LocationId, ItemId, StockCondition)
);
GO

-- =============================================
-- 2. SerializedItems Table (For Serialized Inventory Tracking)
-- =============================================
CREATE TABLE dbo.SerializedItems
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NOT NULL,
    SerialNumber        VARCHAR(255) NOT NULL,
    ItemId              INT NOT NULL,
    LocationId          INT NULL, -- Current location/warehouse
    Status              VARCHAR(50) NOT NULL DEFAULT 'AVAILABLE', -- AVAILABLE, RESERVED, SOLD, DAMAGED, RETURNED
    PurchaseDate        DATETIME NULL,
    PurchasePrice       DECIMAL(18, 4) NULL DEFAULT 0,
    SaleDate            DATETIME NULL,
    SalePrice           DECIMAL(18, 4) NULL DEFAULT 0,
    PartyId             INT NULL, -- Customer/Supplier
    InvoiceId           INT NULL, -- Related invoice
    BatchNumber         VARCHAR(100) NULL,
    ExpiryDate          DATETIME NULL,
    WarrantyExpiryDate  DATETIME NULL,
    Notes               VARCHAR(MAX) NULL,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_SerializedItems_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations(Id),
    CONSTRAINT FK_SerializedItems_Item FOREIGN KEY (ItemId) REFERENCES dbo.Items(Id),
    CONSTRAINT FK_SerializedItems_Location FOREIGN KEY (LocationId) REFERENCES dbo.Locations(Id),
    CONSTRAINT FK_SerializedItems_Party FOREIGN KEY (PartyId) REFERENCES dbo.Parties(Id),
    -- Note: InvoiceId foreign key constraint removed - Invoice table is created later (0370)
    -- The relationship can be enforced at application level or added via ALTER TABLE after Invoice is created
    CONSTRAINT FK_SerializedItems_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_SerializedItems_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_SerializedItems_SerialNumber_Organization UNIQUE (SerialNumber, OrganizationId)
);
GO

-- =============================================
-- 3. Shipments Table (Incoming/Outgoing Shipments)
-- =============================================
CREATE TABLE dbo.Shipments
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NOT NULL,
    ShipmentNo          VARCHAR(50) NOT NULL,
    ShipmentType        VARCHAR(50) NOT NULL, -- INCOMING, OUTGOING, TRANSFER
    LocationId          INT NULL, -- Source or destination location/warehouse
    PartyId             INT NULL, -- Supplier or Customer
    ReferenceNo         VARCHAR(100) NULL, -- PO Number, Invoice Number, etc.
    ReferenceType       VARCHAR(50) NULL, -- PURCHASE_ORDER, INVOICE, etc.
    ReferenceId         INT NULL,
    ShipmentDate        DATETIME NOT NULL,
    ExpectedDate        DATETIME NULL,
    ReceivedDate        DATETIME NULL,
    Status              VARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING, IN_TRANSIT, RECEIVED, PARTIAL, COMPLETED, CANCELLED
    TotalItems          INT NULL DEFAULT 0,
    TotalQuantity       DECIMAL(18, 4) NULL DEFAULT 0,
    TotalValue          DECIMAL(18, 4) NULL DEFAULT 0,
    CarrierName         VARCHAR(255) NULL,
    TrackingNumber      VARCHAR(255) NULL,
    Notes               VARCHAR(MAX) NULL,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_Shipments_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations(Id),
    CONSTRAINT FK_Shipments_Location FOREIGN KEY (LocationId) REFERENCES dbo.Locations(Id),
    CONSTRAINT FK_Shipments_Party FOREIGN KEY (PartyId) REFERENCES dbo.Parties(Id),
    CONSTRAINT FK_Shipments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Shipments_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_Shipments_ShipmentNo_Organization UNIQUE (ShipmentNo, OrganizationId)
);
GO

-- =============================================
-- 4. ShipmentDetails Table
-- =============================================
CREATE TABLE dbo.ShipmentDetails
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    ShipmentId          INT NOT NULL,
    ItemId              INT NOT NULL,
    StockCondition      VARCHAR(50) NULL DEFAULT 'NEW',
    Quantity            DECIMAL(18, 4) NOT NULL,
    ReceivedQuantity    DECIMAL(18, 4) NOT NULL DEFAULT 0,
    UnitPrice           DECIMAL(18, 4) NOT NULL DEFAULT 0,
    TotalPrice          DECIMAL(18, 4) NOT NULL DEFAULT 0,
    BatchNumber         VARCHAR(100) NULL,
    ExpiryDate          DATETIME NULL,
    Description         VARCHAR(500) NULL,
    SeqNo               INT NULL DEFAULT 1,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_ShipmentDetails_Shipment FOREIGN KEY (ShipmentId) REFERENCES dbo.Shipments(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ShipmentDetails_Item FOREIGN KEY (ItemId) REFERENCES dbo.Items(Id),
    CONSTRAINT FK_ShipmentDetails_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_ShipmentDetails_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id)
);
GO

-- =============================================
-- 5. StockMovements Table (Transfers between locations)
-- =============================================
CREATE TABLE dbo.StockMovements
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NOT NULL,
    MovementNo          VARCHAR(50) NOT NULL,
    DocumentType        VARCHAR(10) NULL, -- GRN (Goods Receipt Note), SIR (Stock Issue Requisition), STN (Stock Transfer Note), ADJ (Adjustment), RTN (Return), DMG (Damage), LSS (Loss)
    MovementType        VARCHAR(50) NOT NULL, -- TRANSFER, ADJUSTMENT, RETURN, DAMAGE, LOSS
    FromLocationId      INT NULL, -- NULL for adjustments/receipts
    ToLocationId        INT NULL, -- NULL for adjustments/issues
    MovementDate        DATETIME NOT NULL,
    ReferenceNo         VARCHAR(100) NULL,
    ReferenceType       VARCHAR(50) NULL,
    ReferenceId         INT NULL,
    Status              VARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING, IN_TRANSIT, COMPLETED, CANCELLED
    Reason              VARCHAR(500) NULL,
    ApprovedBy          INT NULL,
    ApprovedDate        DATETIME NULL,
    Notes               VARCHAR(MAX) NULL,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_StockMovements_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations(Id),
    CONSTRAINT FK_StockMovements_FromLocation FOREIGN KEY (FromLocationId) REFERENCES dbo.Locations(Id),
    CONSTRAINT FK_StockMovements_ToLocation FOREIGN KEY (ToLocationId) REFERENCES dbo.Locations(Id),
    CONSTRAINT FK_StockMovements_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_StockMovements_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_StockMovements_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_StockMovements_MovementNo_Organization UNIQUE (MovementNo, OrganizationId)
);
GO

-- =============================================
-- 6. StockMovementDetails Table
-- =============================================
CREATE TABLE dbo.StockMovementDetails
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    MovementId          INT NOT NULL,
    ItemId              INT NOT NULL,
    StockCondition      VARCHAR(50) NULL DEFAULT 'NEW',
    Quantity            DECIMAL(18, 4) NOT NULL,
    UnitCost            DECIMAL(18, 4) NOT NULL DEFAULT 0,
    TotalCost           DECIMAL(18, 4) NOT NULL DEFAULT 0,
    BatchNumber         VARCHAR(100) NULL,
    ExpiryDate          DATETIME NULL,
    Description         VARCHAR(500) NULL,
    SeqNo               INT NULL DEFAULT 1,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_StockMovementDetails_Movement FOREIGN KEY (MovementId) REFERENCES dbo.StockMovements(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockMovementDetails_Item FOREIGN KEY (ItemId) REFERENCES dbo.Items(Id),
    CONSTRAINT FK_StockMovementDetails_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_StockMovementDetails_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id)
);
GO

-- =============================================
-- Create Indexes for Performance
-- =============================================
CREATE INDEX IX_Inventory_Location_Item ON dbo.Inventory(LocationId, ItemId, StockCondition);
CREATE INDEX IX_Inventory_Organization ON dbo.Inventory(OrganizationId);
CREATE INDEX IX_SerializedItems_SerialNumber ON dbo.SerializedItems(SerialNumber, OrganizationId);
CREATE INDEX IX_SerializedItems_Item_Location ON dbo.SerializedItems(ItemId, LocationId, Status);
CREATE INDEX IX_Shipments_ShipmentNo ON dbo.Shipments(ShipmentNo, OrganizationId);
CREATE INDEX IX_Shipments_Status_Date ON dbo.Shipments(Status, ShipmentDate);
CREATE INDEX IX_StockMovements_MovementNo ON dbo.StockMovements(MovementNo, OrganizationId);
CREATE INDEX IX_StockMovements_Status_Date ON dbo.StockMovements(Status, MovementDate);
GO


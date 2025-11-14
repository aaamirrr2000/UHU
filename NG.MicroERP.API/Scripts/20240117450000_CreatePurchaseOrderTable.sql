CREATE TABLE PurchaseOrders
(
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    Guid              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    CustomerId        INT              NOT NULL,
    PONumber          VARCHAR(50)      NOT NULL,
    PODate            DATE             NOT NULL,
    ExpectedDelivery  DATE             NULL,
    ReferenceNo       VARCHAR(100)     NULL,
    Status            INT              NOT NULL,
    Priority          INT              NULL,
    CurrencyId        INT              NULL,
    ExchangeRate      DECIMAL(18,4)    NULL DEFAULT 1.0,
    PaymentTerms      VARCHAR(100)     NULL,
    DeliveryAddress   VARCHAR(255)     NULL,
    Remarks           VARCHAR(500)     NULL,
    CreatedBy         INT              NOT NULL,
    CreatedOn         DATETIME         NOT NULL DEFAULT GETDATE(),
    CreatedFrom       VARCHAR(100)     NULL,
    UpdatedBy         INT              NULL,
    UpdatedOn         DATETIME         NULL,
    UpdatedFrom       VARCHAR(100)     NULL,
    IsSoftDeleted     INT              NOT NULL DEFAULT 0,
    RowVersion        ROWVERSION,
    FOREIGN KEY (CustomerId) REFERENCES Parties(Id),
    FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),
);
GO

CREATE TABLE PurchaseOrderCharges
(
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    Guid              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    PurchaseOrderId   INT NOT NULL,
    AccountId         INT NULL,
    ChargeDescription VARCHAR(255) NULL,
    Amount            DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    IsPercentage      BIT NOT NULL DEFAULT 0,
    PercentageValue   DECIMAL(9,2) NULL,
    CreatedBy         INT NOT NULL,
    CreatedOn         DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedBy         INT NULL,
    UpdatedOn         DATETIME NULL,
    IsSoftDeleted     INT NOT NULL DEFAULT 0,
    RowVersion        ROWVERSION,
    FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(Id),
    FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrders(Id)
);
GO

CREATE TABLE PurchaseOrderDetails
(
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    Guid             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    PurchaseOrderId  INT           NOT NULL,
    ItemId           INT           NOT NULL,
    ItemDescription  VARCHAR(255)  NULL,
    Quantity         DECIMAL(18,3) NOT NULL,
    UnitPrice        DECIMAL(18,2) NOT NULL,
    DiscountPercent  DECIMAL(5,2)  NULL DEFAULT 0,
    TaxPercent       DECIMAL(5,2)  NULL DEFAULT 0,
    DeliveryDate     DATE          NULL,
    Remarks          VARCHAR(255)  NULL,
    CreatedBy        INT           NOT NULL,
    CreatedOn        DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedBy        INT           NULL,
    UpdatedOn        DATETIME      NULL,
    IsSoftDeleted    INT           NOT NULL DEFAULT 0,
    RowVersion       ROWVERSION,
    FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrders(Id)
);
GO

CREATE TABLE Items
(
    Id                    INT              IDENTITY (1,1) PRIMARY KEY,
    OrganizationId        INT              NOT NULL DEFAULT 1,
    Code                  VARCHAR(50)      NOT NULL,
    Name                  VARCHAR(255)     NOT NULL,
    Description           VARCHAR(255)     NULL,
    Pic                   VARCHAR(255)     NULL,
    HSCode                VARCHAR(50)      NULL,   -- Used mainly for inventory / taxable items
    CategoryId            INT              NOT NULL,
    MinQty                DECIMAL(16,2)    NOT NULL DEFAULT 0.00,
    MaxQty                DECIMAL(16,2)    NOT NULL DEFAULT 0.00,
    ReorderQty            DECIMAL(16,2)    NOT NULL DEFAULT 0.00,
    StockType             VARCHAR(50)      NOT NULL DEFAULT 'ITEM',    -- ITEM | SERVICE | FIXED ASSET
    Unit                  VARCHAR(50)      NULL,
    ServingSize           VARCHAR(MAX)     NULL,
    CostPrice             DECIMAL(18,4)    NOT NULL DEFAULT 0.00,
    BasePrice             DECIMAL(18,4)    NOT NULL DEFAULT 0.00,
    DefaultDiscount       DECIMAL(16,6)    NOT NULL DEFAULT 0.00,
    TaxRuleId             INT              NULL,
    ExpenseAccountId      INT              NULL,
    RevenueAccountId      INT              NULL,
    IsFavorite            BIT              NOT NULL DEFAULT 0,
    IsActive              BIT              NOT NULL DEFAULT 1,
    IsSoftDeleted         BIT              NOT NULL DEFAULT 0,
    CreatedBy             INT              NULL,
    CreatedOn             DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom           VARCHAR(255)     NULL,
    UpdatedBy             INT              NULL,
    UpdatedOn             DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom           VARCHAR(255)     NULL,
    FOREIGN KEY (CreatedBy) REFERENCES Users (Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users (Id),
    FOREIGN KEY (CategoryId) REFERENCES Categories (Id),
    FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id),
    FOREIGN KEY (TaxRuleId) REFERENCES TaxRule(Id),
    FOREIGN KEY (ExpenseAccountId) REFERENCES ChartOfAccounts(Id),
    FOREIGN KEY (RevenueAccountId) REFERENCES ChartOfAccounts(Id)
);
GO
SET IDENTITY_INSERT Items ON;

INSERT INTO Items
    (Id, Code, Name, Description, Pic, HSCode, CategoryId, 
     Unit, ServingSize, TaxRuleId, ExpenseAccountId, RevenueAccountId, 
     CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom)
VALUES
    (1, '000000000001', 'MEDICINES',      '', '', '', 1, 
     'PIECE', NULL, NULL, 57, 58, 
     1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01161735', 
     1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01220821'),

    (2, '000000000002', 'COSMETICS',      '', '', '', 2, 
     'PIECE', NULL, NULL, 57, 58, 
     1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01161735', 
     1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01220821'),

    (3, '000000000003', 'SURGICAL ITEMS', '', '', '', 3, 
     'PIECE', NULL, NULL, 57, 58, 
     1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01161735', 
     1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01220821');

SET IDENTITY_INSERT Items OFF;

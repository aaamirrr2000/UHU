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
    CONSTRAINT FK_Items_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Items_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Items_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT FK_Items_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Items_TaxRule FOREIGN KEY (TaxRuleId) REFERENCES TaxRule(Id),
    CONSTRAINT FK_Items_ExpenseAccount FOREIGN KEY (ExpenseAccountId) REFERENCES ChartOfAccounts(Id),
    CONSTRAINT FK_Items_RevenueAccount FOREIGN KEY (RevenueAccountId) REFERENCES ChartOfAccounts(Id)
);

GO

SET IDENTITY_INSERT Items ON;

-- TaxRuleId=1 (No Tax) assigned by default; change to 2 for Pakistan Sale Tax Rule when item is taxable
INSERT INTO Items (Id, Code, Name, Description, CategoryId, StockType, Unit, CostPrice, BasePrice, DefaultDiscount, TaxRuleId, ExpenseAccountId, RevenueAccountId, IsFavorite, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom)
VALUES
	(1, '000000000001', 'MEDICINES', 'PHARMACEUTICAL MEDICINES AND DRUGS', 1, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(2, '000000000002', 'COSMETICS', 'BEAUTY AND COSMETIC PRODUCTS', 2, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(3, '000000000003', 'GROCERY', 'GENERAL GROCERY ITEMS', 3, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(4, '000000000004', 'PERSONAL CARE', 'PERSONAL HYGIENE AND CARE PRODUCTS', 4, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(5, '000000000005', 'MEDICAL DEVICES', 'MEDICAL EQUIPMENT AND DEVICES', 5, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(6, '000000000006', 'SUPPLEMENTS', 'VITAMINS AND DIETARY SUPPLEMENTS', 6, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(7, '000000000007', 'BABY CARE', 'BABY PRODUCTS AND CARE ITEMS', 7, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(8, '000000000008', 'HEALTH CARE', 'GENERAL HEALTH CARE PRODUCTS', 8, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(9, '000000000009', 'SURGICAL ITEMS', 'SURGICAL SUPPLIES AND EQUIPMENT', 9, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(10, '000000000010', 'AYURVEDIC AND HERBAL', 'AYURVEDIC AND HERBAL MEDICINES', 10, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(11, '000000000011', 'HOMEOPATHIC', 'HOMEOPATHIC REMEDIES', 11, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(12, '000000000012', 'EYE CARE', 'EYE CARE PRODUCTS AND MEDICATIONS', 12, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(13, '000000000013', 'DENTAL CARE', 'DENTAL HYGIENE PRODUCTS', 13, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(14, '000000000014', 'FITNESS AND WELLNESS', 'FITNESS AND WELLNESS PRODUCTS', 14, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(15, '000000000015', 'OVER THE COUNTER', 'OTC MEDICATIONS', 15, 'ITEM', 'PCS', 0.00, 0.00, 0.00, 1, 57, 58, 0, 1, 1, NULL, 1, NULL);

SET IDENTITY_INSERT Items OFF;
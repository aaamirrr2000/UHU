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

-- Food menu items with ServingSize = JSON array of ServingSizeModel (Size, Price, Pic)
-- RevenueAccountId = 58 (REVENUE OF SALES / ITEM REVENUE), ExpenseAccountId = 57 (COST OF PURCHASES / ITEM EXPENSE) from ChartOfAccounts
INSERT INTO Items (Id, Code, Name, Description, CategoryId, StockType, Unit, MinQty, MaxQty, ReorderQty, ServingSize, CostPrice, BasePrice, DefaultDiscount, TaxRuleId, ExpenseAccountId, RevenueAccountId, IsFavorite, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom)
VALUES
	(1, '000000000001', 'Garlic Bread', 'Toasted bread with garlic butter and herbs', 1, 'ITEM', 'Pc', 0, 1, 0, N'[{"Size":"Half","Price":2.49,"Pic":""},{"Size":"Full","Price":4.99,"Pic":""}]', 0, 4.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(2, '000000000002', 'Soup of the Day', 'Chef daily soup with bread roll', 1, 'ITEM', 'Bowl', 0, 1, 0, N'[{"Size":"Cup","Price":3.99,"Pic":""},{"Size":"Bowl","Price":5.49,"Pic":""}]', 0, 5.49, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(3, '000000000003', 'Bruschetta', 'Toasted bread with tomato, basil and mozzarella', 1, 'ITEM', 'Pc', 0, 1, 0, N'[{"Size":"Single","Price":4.99,"Pic":""},{"Size":"Share","Price":6.99,"Pic":""}]', 0, 6.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(4, '000000000004', 'Chicken Wings', 'Crispy wings with your choice of sauce', 1, 'ITEM', 'Portion', 0, 1, 0, N'[{"Size":"6 pcs","Price":6.99,"Pic":""},{"Size":"12 pcs","Price":10.99,"Pic":""}]', 0, 8.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(5, '000000000005', 'Caesar Salad', 'Romaine lettuce, parmesan, croutons, Caesar dressing', 1, 'ITEM', 'Bowl', 0, 1, 0, N'[{"Size":"Half","Price":4.99,"Pic":""},{"Size":"Full","Price":7.49,"Pic":""}]', 0, 7.49, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(6, '000000000006', 'Grilled Chicken Breast', 'Juicy chicken breast with herbs and lemon', 2, 'ITEM', 'Pc', 0, 1, 0, N'[{"Size":"Regular","Price":14.99,"Pic":""},{"Size":"Large","Price":17.99,"Pic":""}]', 0, 14.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(7, '000000000007', 'Beef Burger', 'Angus beef patty with lettuce, tomato and fries', 2, 'ITEM', 'Pc', 0, 1, 0, N'[{"Size":"Single","Price":12.99,"Pic":""},{"Size":"Double","Price":15.99,"Pic":""}]', 0, 12.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(8, '000000000008', 'Fish and Chips', 'Beer-battered cod with chunky fries and tartar sauce', 2, 'ITEM', 'Portion', 0, 1, 0, N'[{"Size":"Regular","Price":13.99,"Pic":""}]', 0, 13.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(9, '000000000009', 'Margherita Pizza', 'Tomato sauce, mozzarella and fresh basil', 2, 'ITEM', 'Pc', 0, 1, 0, N'[{"Size":"Small","Price":9.99,"Pic":""},{"Size":"Medium","Price":11.99,"Pic":""},{"Size":"Large","Price":14.99,"Pic":""}]', 0, 11.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(10, '000000000010', 'Pasta Carbonara', 'Spaghetti with bacon, egg and parmesan cream sauce', 2, 'ITEM', 'Bowl', 0, 1, 0, N'[{"Size":"Regular","Price":12.49,"Pic":""}]', 0, 12.49, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(11, '000000000011', 'Chocolate Brownie', 'Warm brownie with vanilla ice cream', 3, 'ITEM', 'Pc', 0, 1, 0, N'[{"Size":"Single","Price":6.99,"Pic":""}]', 0, 6.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(12, '000000000012', 'Tiramisu', 'Classic Italian dessert with coffee and mascarpone', 3, 'ITEM', 'Pc', 0, 1, 0, N'[{"Size":"Single","Price":7.49,"Pic":""}]', 0, 7.49, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(13, '000000000013', 'Ice Cream Sundae', 'Three scoops with whipped cream and sauce', 3, 'ITEM', 'Pc', 0, 1, 0, N'[{"Size":"2 Scoops","Price":4.99,"Pic":""},{"Size":"3 Scoops","Price":5.99,"Pic":""}]', 0, 5.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(14, '000000000014', 'Fresh Orange Juice', 'Freshly squeezed orange juice', 4, 'ITEM', 'Glass', 0, 1, 0, N'[{"Size":"Small","Price":3.49,"Pic":""},{"Size":"Large","Price":4.99,"Pic":""}]', 0, 3.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(15, '000000000015', 'Iced Coffee', 'Chilled coffee with milk and ice', 4, 'ITEM', 'Glass', 0, 1, 0, N'[{"Size":"Regular","Price":4.49,"Pic":""},{"Size":"Large","Price":5.49,"Pic":""}]', 0, 4.49, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(16, '000000000016', 'French Fries', 'Crispy golden fries with salt', 5, 'ITEM', 'Portion', 0, 1, 0, N'[{"Size":"Regular","Price":3.99,"Pic":""},{"Size":"Large","Price":5.49,"Pic":""}]', 0, 3.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL),
	(17, '000000000017', 'Garden Salad', 'Mixed greens with vinaigrette', 5, 'ITEM', 'Bowl', 0, 1, 0, N'[{"Size":"Side","Price":3.99,"Pic":""},{"Size":"Large","Price":4.99,"Pic":""}]', 0, 4.99, 0, 1, 57, 58, 0, 1, 1, NULL, 1, NULL);

SET IDENTITY_INSERT Items OFF;
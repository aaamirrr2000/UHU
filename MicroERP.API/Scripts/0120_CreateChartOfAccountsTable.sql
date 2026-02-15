CREATE TABLE ChartOfAccounts
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NULL    DEFAULT 1,
    Pic                 VARCHAR(MAX) NULL,
    Code                VARCHAR(20) NOT NULL,
    Name                VARCHAR(100) NOT NULL,
    Type                VARCHAR(50) NOT NULL,
    InterfaceType       VARCHAR(50) NULL,
    Description         VARCHAR(100) NULL,
    ParentId            INT NULL DEFAULT 0,
    OpeningBalance      DECIMAL(15, 2) DEFAULT 0.00,
    IsActive            SMALLINT NOT NULL DEFAULT 1,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    CONSTRAINT FK_ChartOfAccounts_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_ChartOfAccounts_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_ChartOfAccounts_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations(Id)
);

GO

SET IDENTITY_INSERT ChartOfAccounts ON;

INSERT INTO ChartOfAccounts (Id, Pic, Code, Name, Type, InterfaceType, Description, ParentId, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom)
VALUES
    (1, 'fa-solid fa-sack-dollar', '10000', 'ASSET', 'ASSET', NULL, 'MAIN ASSET ACCOUNT', 0, 1, NULL, 1, NULL),
    (2, 'fa-solid fa-file-invoice-dollar', '20000', 'LIABILITY', 'LIABILITY', NULL, 'MAIN LIABILITY ACCOUNT', 0, 1, NULL, 1, NULL),
    (3, 'fa-solid fa-money-bill-trend-up', '30000', 'REVENUE', 'REVENUE', NULL, 'MAIN REVENUE ACCOUNT', 0, 1, NULL, 1, NULL),
    (4, 'fa-solid fa-receipt', '40000', 'EXPENSE', 'EXPENSE', NULL, 'MAIN EXPENSE ACCOUNT', 0, 1, NULL, 1, NULL),
    (5, 'fa-solid fa-balance-scale', '50000', 'EQUITY', 'EQUITY', NULL, 'MAIN EQUITY ACCOUNT', 0, 1, NULL, 1, NULL),
    (6, 'fa-solid fa-coins', '30001', 'OTHER REVENUE', 'REVENUE', 'NONE', 'MISCELLANEOUS REVENUE INCOME', 3, 1, NULL, 1, NULL),
    (7, 'fa-solid fa-users', '40001', 'SALARIES', 'EXPENSE', 'NONE', 'EMPLOYEE SALARY EXPENSE', 4, 1, NULL, 1, NULL),
    (8, 'fa-solid fa-bolt', '40002', 'ELECTRICITY BILL', 'EXPENSE', 'NONE', 'ELECTRICITY AND POWER EXPENSE', 4, 1, NULL, 1, NULL),
    (9, 'fa-solid fa-building-columns', '10001', 'BANKS', 'ASSET', 'NONE', 'BANK ACCOUNTS GROUP', 51, 1, NULL, 1, NULL),
    (10, 'fa-solid fa-landmark', '10002', 'COLLECTION BANK', 'ASSET', 'BANK', 'BANK ACCOUNT FOR COLLECTIONS', 9, 1, NULL, 1, NULL),
    (11, 'fa-solid fa-hand-holding-dollar', '10003', 'ACCOUNT RECEIVABLES', 'ASSET', 'NONE', 'AMOUNTS RECEIVABLE FROM CUSTOMERS', 54, 1, NULL, 1, NULL),
    (12, 'fa-solid fa-user', '10004', 'B2C CUSTOMER', 'ASSET', 'ACCOUNTS RECEIVABLE', 'BUSINESS TO CONSUMER RECEIVABLES', 11, 1, NULL, 1, NULL),
    (13, 'fa-solid fa-building', '10005', 'B2B CUSTOMER', 'ASSET', 'ACCOUNTS RECEIVABLE', 'BUSINESS TO BUSINESS RECEIVABLES', 11, 1, NULL, 1, NULL),
    (14, 'fa-solid fa-money-check-dollar', '20001', 'ACCOUNT PAYABLES', 'LIABILITY', 'NONE', 'AMOUNTS PAYABLE TO SUPPLIERS', 52, 1, NULL, 1, NULL),
    (15, 'fa-solid fa-truck', '20002', 'SUPPLIER', 'LIABILITY', NULL, 'SUPPLIER ACCOUNTS PAYABLE', 14, 1, NULL, 1, NULL),
    (16, 'fa-solid fa-truck-ramp-box', '20003', 'B2B SUPPLIER', 'LIABILITY', 'ACCOUNTS PAYABLE', 'BUSINESS TO BUSINESS SUPPLIER PAYABLES', 14, 1, NULL, 1, NULL),
    (17, 'fa-solid fa-percent', '20004', 'GENERAL SALES TAX', 'LIABILITY', 'TAX', 'SALES TAX LIABILITY ACCOUNT', 14, 1, NULL, 1, NULL),
    (18, 'fa-solid fa-building-columns', '10006', 'PAYMENT BANK', 'ASSET', 'BANK', 'BANK ACCOUNT FOR PAYMENTS', 9, 1, NULL, 1, NULL),
    (19, 'fa-solid fa-file-invoice', '30001', 'ADVANCE TAX', 'REVENUE', 'TAX', 'ADVANCE TAX REVENUE ACCOUNT', 3, 1, NULL, 1, NULL),
    (20, 'fa-solid fa-hand-holding-dollar', '30002', 'SERVICE CHARGES', 'REVENUE', 'SERVICE', 'DELIVERY HANDLING SERVICE CHARGES', 6, 1, NULL, 1, NULL),
    (21, 'fa-solid fa-tags', '30003', 'SALES DISCOUNT', 'REVENUE', 'DISCOUNT', 'INVOICE LEVEL DISCOUNT GIVEN', 3, 1, NULL, 1, NULL),
    (22, 'fa-solid fa-wallet', '10007', 'CASH', 'ASSET', 'PAYMENT METHOD', 'PHYSICAL CASH ACCOUNT', 51, 1, NULL, 1, NULL),
    (23, 'fa-solid fa-money-bill-transfer', '10008', 'ONLINE BANK TRANSFER', 'ASSET', 'PAYMENT METHOD', 'ONLINE BANK TRANSFER PAYMENTS', 51, 1, NULL, 1, NULL),
    (24, 'fa-solid fa-credit-card', '10009', 'CREDIT CARD', 'LIABILITY', 'PAYMENT METHOD', 'CREDIT CARD PAYMENT METHOD', 52, 1, NULL, 1, NULL),
    (25, 'fa-solid fa-credit-card', '10010', 'DEBIT CARD', 'ASSET', 'PAYMENT METHOD', 'DEBIT CARD PAYMENT METHOD', 51, 1, NULL, 1, NULL),
    (26, 'fa-solid fa-mobile-screen-button', '10011', 'JAZZCASH', 'ASSET', 'PAYMENT METHOD', 'JAZZCASH MOBILE WALLET ACCOUNT', 53, 1, NULL, 1, NULL),
    (27, 'fa-solid fa-mobile-screen-button', '10012', 'EASYPAISA', 'ASSET', 'PAYMENT METHOD', 'EASYPAISA MOBILE WALLET ACCOUNT', 53, 1, NULL, 1, NULL),
    (28, 'fa-solid fa-chart-line', '30004', 'SALES REVENUE', 'REVENUE', 'NONE', 'SALES REVENUE FROM PRODUCTS', 3, 1, NULL, 1, NULL),
    (29, 'fa-solid fa-hand-holding-dollar', '30005', 'SERVICE REVENUE', 'REVENUE', 'NONE', 'SERVICE RELATED REVENUE', 3, 1, NULL, 1, NULL),
    (30, 'fa-solid fa-cash-register', '40003', 'OPERATING EXPENSE', 'EXPENSE', 'NONE', 'GENERAL OPERATING EXPENSES', 4, 1, NULL, 1, NULL),
    (31, 'fa-solid fa-cart-shopping', '40004', 'PURCHASE EXPENSE', 'EXPENSE', 'NONE', 'PURCHASE AND PROCUREMENT EXPENSES', 4, 1, NULL, 1, NULL),
    (32, 'fa-solid fa-truck', '20005', 'B2C SUPPLIER', 'LIABILITY', 'ACCOUNTS PAYABLE', 'BUSINESS TO CONSUMER SUPPLIER PAYABLES', 14, 1, NULL, 1, NULL),
    (33, 'fa-solid fa-money-bill', '10013', 'ADVANCE AGAINST SALARY', 'ASSET', 'ADVANCE', 'EMPLOYEE SALARY ADVANCES', 50, 1, NULL, 1, NULL),
    (34, 'fa-solid fa-taxi', '40005', 'TAXI AND FUEL CHARGES', 'EXPENSE', 'NONE', 'TAXI AND FUEL TRANSPORTATION EXPENSES', 4, 1, NULL, NULL, NULL),
    (35, 'fa-solid fa-boxes-stacked', '10014', 'ADVANCE AGAINST PURCHASES', 'ASSET', 'ADVANCE', 'ADVANCE PAYMENT FOR STOCK PURCHASES', 50, 1, NULL, 1, NULL),
    (36, 'fa-solid fa-paperclip', '40006', 'STATIONERY', 'EXPENSE', 'NONE', 'OFFICE STATIONERY SUPPLIES', 4, 1, NULL, NULL, NULL),
    (37, 'fa-solid fa-wrench', '40007', 'REPAIR AND MAINTENANCE', 'EXPENSE', 'NONE', 'REPAIR AND MAINTENANCE EXPENSE', 4, 1, NULL, NULL, NULL),
    (38, 'fa-solid fa-user-tie', '40008', 'EMPLOYEE EXPENSES', 'EXPENSE', 'NONE', 'EMPLOYEE RELATED EXPENSES', 4, 1, NULL, NULL, NULL),
    (39, 'fa-solid fa-hand-holding-heart', '40009', 'CHARITY', 'EXPENSE', 'NONE', 'CHARITABLE DONATIONS AND CONTRIBUTIONS', 4, 1, NULL, NULL, NULL),
    (40, 'fa-solid fa-users', '40010', 'SALARIES AND WAGES', 'EXPENSE', 'NONE', 'EMPLOYEE SALARIES AND WAGES', 4, 1, NULL, NULL, NULL),
    (41, 'fa-solid fa-print', '40011', 'PHOTOSTATE', 'EXPENSE', 'NONE', 'PHOTOCOPYING AND PRINTING EXPENSES', 4, 1, NULL, NULL, NULL),
    (42, 'fa-solid fa-ellipsis', '40012', 'MISCELLANEOUS EXPENSES', 'EXPENSE', 'NONE', 'MISCELLANEOUS AND OTHER EXPENSES', 4, 1, NULL, NULL, NULL),
    (43, 'fa-solid fa-truck-fast', '40013', 'DELIVERY EXPENSES', 'EXPENSE', 'NONE', 'DELIVERY AND SHIPPING EXPENSES', 4, 1, NULL, NULL, NULL),
    (44, 'fa-solid fa-box', '40014', 'COURIER EXPENSES', 'EXPENSE', 'NONE', 'COURIER AND SHIPPING CHARGES', 4, 1, NULL, NULL, NULL),
    (45, 'fa-solid fa-handshake', '50001', 'CAPITAL PARTNER 1 (AAMIR)', 'EQUITY', 'NONE', 'CAPITAL INVESTMENT BY AAMIR', 5, 1, NULL, NULL, NULL),
    (46, 'fa-solid fa-handshake', '50002', 'CAPITAL PARTNER 2 (TALHA)', 'EQUITY', 'NONE', 'CAPITAL INVESTMENT BY TALHA', 5, 1, NULL, NULL, NULL),
    (47, 'fa-solid fa-briefcase', '40015', 'OFFICE EXPENSES', 'EXPENSE', 'NONE', 'GENERAL OFFICE EXPENSES', 4, 1, NULL, NULL, NULL),
    (48, 'fa-solid fa-champagne-glasses', '40016', 'ENTERTAINMENT', 'EXPENSE', 'NONE', 'ENTERTAINMENT AND HOSPITALITY EXPENSES', 4, 1, NULL, NULL, NULL),
    (50, 'fa-solid fa-money-bill-transfer', '10016', 'ADVANCES', 'ASSET', 'NONE', 'ADVANCE PAYMENTS GROUP', 54, 1, NULL, 1, NULL),
    (51, 'fa-solid fa-wallet', '10017', 'CASH AND CASH EQUIVALENTS', 'ASSET', 'NONE', 'CASH AND CASH EQUIVALENT ASSETS', 54, 1, NULL, 1, NULL),
    (52, 'fa-solid fa-file-invoice-dollar', '20006', 'CURRENT LIABILITIES', 'LIABILITY', 'NONE', 'CURRENT LIABILITIES GROUP', 2, 1, NULL, NULL, NULL),
    (53, 'fa-solid fa-mobile-screen-button', '10018', 'MOBILE WALLET', 'ASSET', 'NONE', 'MOBILE WALLET ACCOUNTS', 51, 1, NULL, NULL, NULL),
    (54, 'fa-solid fa-sack-dollar', '10019', 'CURRENT ASSETS', 'ASSET', 'NONE', 'CURRENT ASSETS GROUP', 1, 1, NULL, NULL, NULL),
    (55, 'fa-solid fa-cart-shopping', '40017', 'COST OF MANUAL PURCHASES', 'EXPENSE', 'MANUAL ITEM EXPENSE', 'COST OF GOODS SOLD MANUAL ITEMS', 4, 1, NULL, NULL, NULL),
    (56, 'fa-solid fa-cash-register', '30006', 'REVENUE FROM MANUAL SALES', 'REVENUE', 'MANUAL ITEM REVENUE', 'REVENUE FROM MANUAL SALES ITEMS', 3, 1, NULL, NULL, NULL),
    (57, 'fa-solid fa-cart-shopping', '40018', 'COST OF PURCHASES', 'EXPENSE', 'ITEM EXPENSE', 'COST OF GOODS SOLD', 4, 1, NULL, NULL, NULL),
    (58, 'fa-solid fa-chart-line', '30007', 'REVENUE OF SALES', 'REVENUE', 'ITEM REVENUE', 'REVENUE FROM SALES ITEMS', 3, 1, NULL, NULL, NULL),
    (59, 'fa-solid fa-building', '40019', 'ADMINISTRATIVE EXPENSES', 'EXPENSE', 'NONE', 'ADMINISTRATIVE AND OFFICE EXPENSES', 4, 1, NULL, 1, NULL),
    (60, 'fa-solid fa-boxes-stacked', '10020', 'ADVANCE AGAINST STOCK', 'ASSET', 'ADVANCE', 'ADVANCE PAYMENTS FOR STOCK INVENTORY', 50, 1, NULL, 1, NULL),
    (61, 'fa-solid fa-user-tie', '40020', 'AGENT COMMISSION', 'EXPENSE', 'NONE', 'COMMISSION PAID TO AGENTS', 4, 1, NULL, 1, NULL),
    (62, 'fa-solid fa-piggy-bank', '10021', 'BANK DEPOSIT', 'ASSET', 'NONE', 'BANK DEPOSIT ACCOUNTS', 9, 1, NULL, 1, NULL),
    (63, 'fa-solid fa-chart-simple', '40021', 'BUSINESS DEVELOPMENT', 'EXPENSE', 'NONE', 'BUSINESS DEVELOPMENT AND GROWTH EXPENSES', 4, 1, NULL, 1, NULL),
    (64, 'fa-solid fa-money-bill-wave', '50003', 'CAPITAL INVESTMENT', 'EQUITY', 'NONE', 'CAPITAL INVESTMENTS AND CONTRIBUTIONS', 5, 1, NULL, 1, NULL),
    (65, 'fa-solid fa-hand-holding-heart', '40022', 'CHARITY EXPENSES', 'EXPENSE', 'NONE', 'CHARITABLE DONATIONS AND EXPENSES', 4, 1, NULL, 1, NULL),
    (66, 'fa-solid fa-truck-fast', '40023', 'COURIER CHARGES', 'EXPENSE', 'NONE', 'COURIER AND SHIPPING CHARGES', 4, 1, NULL, 1, NULL),
    (67, 'fa-solid fa-truck-fast', '40024', 'DELIVERY CHARGES', 'EXPENSE', 'NONE', 'DELIVERY AND TRANSPORTATION CHARGES', 4, 1, NULL, 1, NULL),
    (68, 'fa-solid fa-bolt', '40025', 'ELECTRICITY BILL', 'EXPENSE', 'NONE', 'ELECTRICITY AND UTILITIES EXPENSES', 4, 1, NULL, 1, NULL),
    (69, 'fa-solid fa-champagne-glasses', '40026', 'ENTERTAINMENT', 'EXPENSE', 'NONE', 'ENTERTAINMENT AND HOSPITALITY EXPENSES', 4, 1, NULL, 1, NULL),
    (70, 'fa-solid fa-money-bill-trend-up', '30008', 'GROSS REVENUE', 'REVENUE', 'NONE', 'TOTAL GROSS REVENUE', 3, 1, NULL, 1, NULL),
    (71, 'fa-solid fa-pills', '30009', 'IMPORTED MEDICINE SALE', 'REVENUE', 'NONE', 'REVENUE FROM IMPORTED MEDICINES', 3, 1, NULL, 1, NULL),
    (72, 'fa-solid fa-wifi', '40027', 'INTERNET BILL', 'EXPENSE', 'NONE', 'INTERNET AND COMMUNICATION EXPENSES', 4, 1, NULL, 1, NULL),
    (73, 'fa-solid fa-certificate', '40028', 'LICENSE FEE', 'EXPENSE', 'NONE', 'LICENSE AND REGISTRATION FEES', 4, 1, NULL, 1, NULL),
    (74, 'fa-solid fa-chart-pie', '30010', 'MARGIN ON SALES', 'REVENUE', 'NONE', 'PROFIT MARGIN ON SALES', 3, 1, NULL, 1, NULL),
    (75, 'fa-solid fa-ellipsis', '40029', 'MISCELLANEOUS EXPENSES', 'EXPENSE', 'NONE', 'MISCELLANEOUS AND OTHER EXPENSES', 4, 1, NULL, 1, NULL),
    (76, 'fa-solid fa-computer', '40030', 'OFFICE EQUIPMENT', 'EXPENSE', 'NONE', 'OFFICE EQUIPMENT PURCHASES', 4, 1, NULL, 1, NULL),
    (77, 'fa-solid fa-file-invoice-dollar', '20007', 'PAYMENT AGAINST PURCHASE INVOICE', 'LIABILITY', 'NONE', 'PAYMENTS AGAINST PURCHASE INVOICES', 14, 1, NULL, 1, NULL),
    (78, 'fa-solid fa-receipt', '10022', 'RECEIPT AGAINST SALE INVOICE', 'ASSET', 'NONE', 'RECEIPTS AGAINST SALE INVOICES', 11, 1, NULL, 1, NULL),
    (79, 'fa-solid fa-hammer', '40031', 'RENOVATION', 'EXPENSE', 'NONE', 'RENOVATION AND IMPROVEMENT EXPENSES', 4, 1, NULL, 1, NULL),
    (80, 'fa-solid fa-house', '40032', 'RENT', 'EXPENSE', 'NONE', 'RENT AND LEASE EXPENSES', 4, 1, NULL, 1, NULL),
    (81, 'fa-solid fa-wrench', '40033', 'REPAIR AND MAINTENANCE', 'EXPENSE', 'NONE', 'REPAIR AND MAINTENANCE EXPENSES', 4, 1, NULL, 1, NULL),
    (82, 'fa-solid fa-users', '40034', 'SALARIES AND WAGES', 'EXPENSE', 'NONE', 'EMPLOYEE SALARIES AND WAGES', 4, 1, NULL, 1, NULL),
    (83, 'fa-solid fa-shield', '10023', 'SECURITY DEPOSIT (ASSET)', 'ASSET', 'NONE', 'SECURITY DEPOSITS PAID', 54, 1, NULL, 1, NULL),
    (84, 'fa-solid fa-paperclip', '40035', 'STATIONERY', 'EXPENSE', 'NONE', 'OFFICE STATIONERY AND SUPPLIES', 4, 1, NULL, 1, NULL),
    (85, 'fa-solid fa-percent', '20008', 'TAX ON MARGIN', 'LIABILITY', 'TAX', 'TAX PAYABLE ON PROFIT MARGIN', 14, 1, NULL, 1, NULL),
    (86, 'fa-solid fa-taxi', '40036', 'TAXI AND FUEL EXPENSES', 'EXPENSE', 'NONE', 'TAXI AND FUEL TRANSPORTATION EXPENSES', 4, 1, NULL, 1, NULL);

SET IDENTITY_INSERT ChartOfAccounts OFF;

GO

-- FK from Employees.BankAccountId (Employees table created in 0090, before ChartOfAccounts)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Employees')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employees_BankAccount')
        ALTER TABLE Employees ADD CONSTRAINT FK_Employees_BankAccount FOREIGN KEY (BankAccountId) REFERENCES ChartOfAccounts(Id);
END
GO

-- Index on HireDate for reporting (if not already present)
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Employees')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employees_HireDate' AND object_id = OBJECT_ID('Employees'))
        CREATE INDEX IX_Employees_HireDate ON Employees(HireDate);
END
GO

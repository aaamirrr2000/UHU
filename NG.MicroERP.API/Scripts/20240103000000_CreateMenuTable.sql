CREATE TABLE dbo.Menu
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Guid            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    MenuCaption     VARCHAR(100) NULL,
    AdditionalInfo  VARCHAR(100) NULL,
    Tooltip         VARCHAR(255) NULL,
    PageName        VARCHAR(100) NULL,
    ParentId        INT NULL,
    Icon            VARCHAR(50) NULL,
    SeqNo           INT NULL,
    Live            SMALLINT NULL,
    CreatedBy       INT NULL,
    CreatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom     VARCHAR(255) NULL,
    UpdatedBy       INT NULL,
    UpdatedOn       DATETIME NULL,
    UpdatedFrom     VARCHAR(255) NULL,
    IsSoftDeleted   SMALLINT NOT NULL DEFAULT 0,
    RowVersion      ROWVERSION
);
GO


SET IDENTITY_INSERT dbo.Menu ON;
GO

/*──────────────────────────── SALES ─────────────────────────────*/
INSERT INTO Menu
    (Id, MenuCaption, AdditionalInfo, Tooltip, PageName, ParentId,
     Icon, SeqNo, Live,
     CreatedBy, CreatedOn, CreatedFrom,
     UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted)
VALUES
(20, 'Operations', NULL, 'Sales and purchase related operations',
     NULL, 0,
     'fas fa-shopping-cart', 1000, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(21, 'Purchase Order', 'Customers Purchase Order',
    'Create customer purchase orders', 'PurchaseOrderDashboard', 20,
     'fas fa-file-invoice-dollar', 1010, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(22, 'Sale Invoice', 'SALE', 'Generate and view customer invoices',
     'BillDashboard', 20,
     'fas fa-file-invoice', 1020, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(23, 'Cash Book', NULL, 'Record and monitor cash receipts & payments',
     'CashBookDashboard', 20,
     'fas fa-wallet', 1030, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),


/*──────────────────────────── STOCK ─────────────────────────────*/
(40, 'Stock', NULL, 'Manage inventory and stock movements',
     NULL, 0,
     'fas fa-boxes', 2000, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(41, 'Stock Received', NULL, 'Record incoming stock receipts',
     'StockPage', 40,
     'fas fa-truck-loading', 2010, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(42, 'Stock Issue', NULL, 'Record outgoing stock issues',
     'StockIssuePage', 40,
     'fas fa-dolly', 2020, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),


/*──────────────────────────── REPORTS ─────────────────────────────*/
(60, 'Reports', NULL, 'Generate operational and financial reports',
     NULL, 0,
     'fas fa-chart-line', 3000, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(61, 'Collection Report', 'REPORT', 'View collections and payments status',
     'CollectionReport', 60,
     'fas fa-hand-holding-usd', 3010, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(62, 'Cash Position Report', 'REPORT', 'Daily cash balance and transactions',
     'CashPositionReport', 60,
     'fas fa-money-bill-wave', 3020, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),


/*──────────────────────────── SECURITY ─────────────────────────────*/
(80, 'Security Setup', NULL, 'User accounts and access permissions',
     NULL, 0,
     'fas fa-shield-alt', 4000, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(81, 'Users', NULL, 'Add, edit, and manage system users',
     'UsersDashboard', 80,
     'fas fa-user', 4010, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(82, 'Groups', NULL, 'Manage user roles and group permissions',
     'GroupsDashboard', 80,
     'fas fa-users-cog', 4020, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(83, 'Permissions', NULL, 'Assign and manage user or group permissions',
     'PermissionsDashboard', 80,
     'fas fa-lock', 4030, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),


/*──────────────────────────── SETUP / MASTER DATA ─────────────────────────────*/
(100, 'General Setup', NULL, 'System configuration and master data',
     NULL, 0,
     'fas fa-cogs', 5000, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(101, 'Organization', NULL, 'Manage organisation profile and details',
     'OrganizationsDashboard', 100,
     'fas fa-building', 5010, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(102, 'Locations', NULL, 'Branch or warehouse location setup',
     'LocationsDashboard', 100,
     'fas fa-map-marker-alt', 5020, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(103, 'Employee', NULL, 'Employee profiles and HR setup',
     'EmployeesDashboard', 100,
     'fas fa-id-badge', 5030, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(104, 'Parties', NULL, 'Customer and vendor master data',
     'PartiesDashboard', 100,
     'fas fa-address-book', 5040, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(105, 'Categories', NULL, 'Product categories and grouping',
     'CategoriesDashboard', 100,
     'fas fa-tags', 5050, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(106, 'Items', NULL, 'Product and SKU management',
     'ItemsDashboard', 100,
     'fas fa-barcode', 5060, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(107, 'Currency', NULL, 'Define Currencies and Rates',
     'ChargeRulesDashboard', 100,
     'fas fa-receipt', 5070, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(108, 'Areas', NULL, 'Cities, regions, provinces, and countries setup',
     'AreasDashboard', 100,
     'fas fa-map', 5080, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(109, 'Shifts', NULL, 'Shifts Management',
     'ShiftsDashboard', 100,
     'fas fa-stopwatch', 5030, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

/*──────────────────────────── FINANCIAL ─────────────────────────────*/
(120, 'Financial Setup', NULL, 'Financial accounting and ledger setup',
     NULL, 0,
     'fas fa-balance-scale', 6000, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(121, 'Chart of Accounts', NULL, 'Define and manage chart of accounts',
     'ChartOfAccountsDashboard', 120,
     'fas fa-sitemap', 6010, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

(122, 'Banks', NULL, 'Maintain banks and branches',
     'BankDashboard', 120,
     'fas fa-university', 6020, 1,
     1, GETDATE(), NULL, 1, GETDATE(), NULL, 0);

GO
SET IDENTITY_INSERT dbo.Menu OFF;
GO


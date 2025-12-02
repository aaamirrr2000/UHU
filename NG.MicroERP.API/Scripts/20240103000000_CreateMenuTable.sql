CREATE TABLE dbo.Menu
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
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
    IsSoftDeleted   SMALLINT NOT NULL DEFAULT 0
);
GO

SET IDENTITY_INSERT dbo.Menu ON;

INSERT INTO Menu (Id, MenuCaption, AdditionalInfo, Tooltip, PageName, ParentId, Icon, SeqNo, Live, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted)
VALUES 
/*──────────────────────────── OPERATIONS ─────────────────────────────*/
(50, 'Operations', NULL, 'Sales and purchase related operations', NULL, 0, 'fas fa-shopping-cart', 1000, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(51, 'Purchase Order', 'Customers Purchase Order', 'Create customer purchase orders', 'PurchaseOrdersDashboard', 50, 'fas fa-file-invoice-dollar', 1010, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(52, 'Cash Book', NULL, 'Record and monitor cash receipts & payments', 'CashBookDashboard', 50, 'fas fa-wallet', 1020, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

/*──────────────────────────── STOCK MANAGEMENT ─────────────────────────────*/
(100, 'Stock Management', NULL, 'Manage inventory and stock movements', NULL, 0, 'fas fa-boxes', 2000, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(101, 'Stock Received', NULL, 'Record incoming stock receipts', 'StockPage', 100, 'fas fa-truck-loading', 2010, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(102, 'Stock Issue', NULL, 'Record outgoing stock issues', 'StockIssuePage', 100, 'fas fa-dolly', 2020, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

/*──────────────────────────── REPORTS ─────────────────────────────*/
(150, 'Reports', NULL, 'Generate operational and financial reports', NULL, 0, 'fas fa-chart-line', 3000, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(151, 'Collections Report', 'REPORT', 'View collections and payments status', 'CollectionReport', 150, 'fas fa-hand-holding-usd', 3010, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(152, 'Cash Position Report', 'REPORT', 'Daily cash balance and transactions', 'CashPositionReport', 150, 'fas fa-money-bill-wave', 3020, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

/*──────────────────────────── LEAVE MANAGEMENT ─────────────────────────────*/
(200, 'Leave Management', NULL, 'Employee leave requests and approvals', NULL, 0, 'fas fa-calendar-check', 4000, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(201, 'Leave Requests', NULL, 'Submit and manage leave applications', 'LeaveRequestsDashboard', 200, 'fas fa-clipboard-list', 4010, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

/*──────────────────────────── SECURITY SETUP ─────────────────────────────*/
(250, 'Security Setup', NULL, 'User accounts and access permissions', NULL, 0, 'fas fa-shield-alt', 5000, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(251, 'Users', NULL, 'Add, edit, and manage system users', 'UsersDashboard', 250, 'fas fa-user', 5010, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(252, 'Groups', NULL, 'Manage user roles and group permissions', 'GroupsDashboard', 250, 'fas fa-users-cog', 5020, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(253, 'Permissions', NULL, 'Assign and manage user or group permissions', 'PermissionsDashboard', 250, 'fas fa-lock', 5030, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),

/*──────────────────────────── MASTER DATA SETUP ─────────────────────────────*/
(300, 'Master Data Setup', NULL, 'System configuration and master data', NULL, 0, 'fas fa-database', 6000, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(301, 'Organization', NULL, 'Manage organisation profile and details', 'OrganizationsDashboard', 300, 'fas fa-building', 6010, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(302, 'Locations', NULL, 'Branch or warehouse location setup', 'LocationsDashboard', 300, 'fas fa-map-marker-alt', 6020, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(303, 'Areas', NULL, 'Cities, regions, provinces, and countries setup', 'AreasDashboard', 300, 'fas fa-map', 6030, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(304, 'Parties', NULL, 'Customer and vendor master data', 'PartiesDashboard', 300, 'fas fa-address-book', 6040, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(305, 'Categories', NULL, 'Product categories and grouping', 'CategoriesDashboard', 300, 'fas fa-tags', 6050, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(306, 'Items', NULL, 'Product and SKU management', 'ItemsDashboard', 300, 'fas fa-barcode', 6060, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(307, 'Employees', NULL, 'Employee profiles and HR setup', 'EmployeesDashboard', 300, 'fas fa-id-badge', 6070, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(308, 'Departments', NULL, 'Manage organizational departments', 'DepartmentsDashboard', 300, 'fas fa-sitemap', 6080, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(309, 'Designations', NULL, 'Manage job titles and designations', 'DesignationsDashboard', 300, 'fas fa-user-tag', 6090, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(310, 'Shifts', NULL, 'Shifts Management', 'ShiftsDashboard', 300, 'fas fa-stopwatch', 6100, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(311, 'Chart of Accounts', NULL, 'Define and manage chart of accounts', 'ChartOfAccountsDashboard', 300, 'fas fa-sitemap', 6110, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(312, 'Banks', NULL, 'Maintain banks and branches', 'BankDashboard', 300, 'fas fa-university', 6120, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(313, 'Currency', NULL, 'Define Currencies and Rates', 'CurrenciesDashboard', 300, 'fas fa-money-bill-wave', 6130, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(314, 'Tax Master', NULL, 'Tax rates and configurations', 'TaxMasterDashboard', 300, 'fas fa-percentage', 6140, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(315, 'Payment Terms', NULL, 'Define payment terms and conditions', 'PaymentTermsDashboard', 300, 'fas fa-file-contract', 6150, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(316, 'Leave Types', NULL, 'Define and manage leave types', 'LeaveTypesDashboard', 300, 'fas fa-calendar-alt', 6160, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(317, 'Holiday Calendar', NULL, 'Manage public and company holidays', 'HolidayCalendarDashboard', 300, 'fas fa-calendar-day', 6170, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(318, 'Backup', NULL, 'Backup your data', 'BackupPage', 300, 'fas fa-database', 6180, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0);

SET IDENTITY_INSERT dbo.Menu OFF;
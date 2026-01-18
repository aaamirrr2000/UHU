CREATE TABLE dbo.Menu
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT NULL DEFAULT 1,
    MenuCaption     VARCHAR(100) NULL,
    Parameter       VARCHAR(100) NULL,
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

INSERT INTO Menu
(
    Id, MenuCaption, Parameter, Tooltip, PageName, ParentId, Icon, SeqNo, Live,
    CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom
)
VALUES

/*──────────────────────────── OPERATIONS ─────────────────────────────*/
(50, 'Transactions', NULL, 'Manage sales, purchases, and daily operations', NULL, 0, 'fa-solid fa-cart-shopping', 1000, 1, 1, NULL, 1, NULL),
(51, 'Sales Orders', 'SALE', 'Create and manage customer sales orders', 'InvoiceDashboard', 50, 'fa-solid fa-file-invoice-dollar', 1010, 1, 1, NULL, 1, NULL),
(52, 'Purchase Orders', 'PURCHASE', 'Create and manage supplier purchase orders', 'InvoiceDashboard', 50, 'fa-solid fa-file-invoice', 1020, 1, 1, NULL, 1, NULL),
(53, 'Cash Book', NULL, 'Record and track cash receipts and payments', 'CashBookDashboard', 50, 'fa-solid fa-wallet', 1030, 1, 1, NULL, 1, NULL),
(57, 'Physical Cash Count', NULL, 'Record daily physical cash counts by denomination', 'PhysicalCashCountDashboard', 50, 'fa-solid fa-money-bill-wave', 1040, 1, 1, NULL, 1, NULL),
(54, 'General Ledger', NULL, 'Manage general ledger entries and journal transactions', 'GeneralLedgerDashboard', 50, 'fa-solid fa-book', 1045, 1, 1, NULL, 1, NULL),

/*──────────────────────────── STOCK MANAGEMENT ─────────────────────────────*/
(100, 'Stock Management', NULL, 'Control inventory and stock movement', NULL, 0, 'fa-solid fa-boxes-stacked', 2000, 1, 1, NULL, 1, NULL),
(112, 'Shipments', NULL, 'Manage incoming and outgoing stock shipments', 'ShipmentsDashboard', 100, 'fa-solid fa-shipping-fast', 2050, 1, 1, NULL, 1, NULL),
(113, 'Stock Movements', NULL, 'Transfer stock between locations and adjustments', 'StockMovementsDashboard', 100, 'fa-solid fa-exchange-alt', 2060, 1, 1, NULL, 1, NULL),
(114, 'Serialized Items', NULL, 'Track serialized inventory items', 'SerializedItemsDashboard', 100, 'fa-solid fa-barcode', 2070, 1, 1, NULL, 1, NULL),

/*──────────────────────────── REPORTS ─────────────────────────────*/
(150, 'Reports', 'REPORT', 'View operational, financial, and analytical reports', NULL, 0, 'fa-solid fa-chart-line', 3000, 1, 1, NULL, 1, NULL),
(153, 'Daily Funds Closing', NULL, 'Daily cash and funds closing summary', 'DailyFundsClosingReport', 150, 'fa-solid fa-calendar-day', 3030, 1, 1, NULL, 1, NULL),
(154, 'Inventory Closing', NULL, 'Daily inventory closing summary', 'InventoryClosingReport', 150, 'fa-solid fa-boxes', 3040, 1, 1, NULL, 1, NULL),
(155, 'Cash by Location/User', NULL, 'Cash holdings by location and user', 'CashByLocationUserReport', 150, 'fa-solid fa-map-marked-alt', 3050, 1, 1, NULL, 1, NULL),
(156, 'Inventory by Location', NULL, 'Current inventory levels by location', 'InventoryByLocationReport', 150, 'fa-solid fa-warehouse', 3060, 1, 1, NULL, 1, NULL),
(157, 'Trial Balance', NULL, 'Trial balance as of selected date', 'TrialBalanceReport', 150, 'fa-solid fa-balance-scale', 3070, 1, 1, NULL, 1, NULL),
(158, 'Account Analysis', NULL, 'Detailed account analysis', 'AccountAnalysisReport', 150, 'fa-solid fa-chart-bar', 3080, 1, 1, NULL, 1, NULL),
(159, 'Profit & Loss', NULL, 'Income statement for selected period', 'ProfitLossReport', 150, 'fa-solid fa-chart-line', 3090, 1, 1, NULL, 1, NULL),
(160, 'Balance Sheet', NULL, 'Balance sheet as of selected date', 'BalanceSheetReport', 150, 'fa-solid fa-file-invoice-dollar', 3100, 1, 1, NULL, 1, NULL),
(161, 'Cash Reconciliation', NULL, 'Reconcile cash transactions against physical cash count', 'CashReconciliationReport', 150, 'fa-solid fa-balance-scale', 3110, 1, 1, NULL, 1, NULL),
(162, 'Employee Advance Report', NULL, 'View outstanding employee advances from cashbook', 'EmployeeAdvanceReport', 150, 'fa-solid fa-user-clock', 3120, 1, 1, NULL, 1, NULL),

/*──────────────────────────── LEAVE MANAGEMENT ─────────────────────────────*/
(200, 'Leave Management', NULL, 'Manage employee leave requests and approvals', NULL, 0, 'fa-solid fa-calendar-check', 4000, 1, 1, NULL, 1, NULL),
(201, 'Leave Requests', NULL, 'Apply, review, and approve leave requests', 'LeaveRequestsDashboard', 200, 'fa-solid fa-clipboard-list', 4010, 1, 1, NULL, 1, NULL),

/*──────────────────────────── HRMS ─────────────────────────────*/
(220, 'HRMS', NULL, 'Human Resource Management System', NULL, 0, 'fa-solid fa-users', 4500, 1, 1, NULL, 1, NULL),
(221, 'Employee Management', NULL, 'Manage employee records with enhanced HRMS features', 'EmployeesDashboard', 220, 'fa-solid fa-user-tie', 4510, 1, 1, NULL, 1, NULL),
(222, 'Salary Management', NULL, 'Process employee salaries, allowances, and deductions', 'SalaryDashboard', 220, 'fa-solid fa-money-check-dollar', 4520, 1, 1, NULL, 1, NULL),

/*──────────────────────────── SECURITY ─────────────────────────────*/
(250, 'Security Setup', 'SETUP', 'Control user access and permissions', NULL, 0, 'fa-solid fa-shield-halved', 5000, 1, 1, NULL, 1, NULL),
(251, 'Users', NULL, 'Create and manage system users', 'UsersDashboard', 250, 'fa-solid fa-user', 5010, 1, 1, NULL, 1, NULL),
(252, 'User Groups', NULL, 'Define roles and group permissions', 'GroupsDashboard', 250, 'fa-solid fa-users-cog', 5020, 1, 1, NULL, 1, NULL),
(253, 'Permissions', NULL, 'Assign menu and action permissions', 'PermissionsDashboard', 250, 'fa-solid fa-lock', 5030, 1, 1, NULL, 1, NULL),

/*──────────────────────────── GENERAL SETUP ─────────────────────────────*/
(300, 'General Setup', 'SETUP', 'Setup general system parameters', NULL, 0, 'fa-solid fa-cogs', 7100, 1, 1, NULL, 1, NULL),
(301, 'Organization', NULL, 'Define company profile and registration details', 'OrganizationsDashboard', 300, 'fa-solid fa-building', 7110, 1, 1, NULL, 1, NULL),
(302, 'Locations', NULL, 'Define branches, warehouses, and locations', 'LocationsDashboard', 300, 'fa-solid fa-map-marker-alt', 7120, 1, 1, NULL, 1, NULL),
(303, 'Areas', NULL, 'Define cities, regions, and geographical areas', 'AreasDashboard', 300, 'fa-solid fa-map', 7130, 1, 1, NULL, 1, NULL),
(304, 'Parties', NULL, 'Manage customers, suppliers, and contacts', 'PartiesDashboard', 300, 'fa-solid fa-address-book', 7140, 1, 1, NULL, 1, NULL),
(305, 'Item Categories', NULL, 'Setup product categories and grouping', 'CategoriesDashboard', 300, 'fa-solid fa-tags', 7150, 1, 1, NULL, 1, NULL),
(306, 'Items', NULL, 'Manage products and SKUs', 'ItemsDashboard', 300, 'fa-solid fa-barcode', 7160, 1, 1, NULL, 1, NULL),
(307, 'Employees', NULL, 'Define employee master records', 'EmployeesDashboard', 300, 'fa-solid fa-id-badge', 7170, 1, 1, NULL, 1, NULL),
(308, 'Departments', NULL, 'Define organizational departments', 'DepartmentsDashboard', 300, 'fa-solid fa-sitemap', 7180, 1, 1, NULL, 1, NULL),
(309, 'Designations', NULL, 'Define job titles and roles', 'DesignationsDashboard', 300, 'fa-solid fa-user-tag', 7190, 1, 1, NULL, 1, NULL),
(310, 'Shifts', NULL, 'Define work shifts and timings', 'ShiftsDashboard', 300, 'fa-solid fa-clock', 7200, 1, 1, NULL, 1, NULL),
(311, 'System Backup', NULL, 'Backup and restore system data', 'BackupPage', 300, 'fa-solid fa-database', 7210, 1, 1, NULL, 1, NULL),
(312, 'System Configuration', NULL, 'Configure system settings like backup, email, FTP, and images', 'SystemConfigurationDashboard', 300, 'fa-solid fa-sliders', 7220, 1, 1, NULL, 1, NULL),

/*──────────────────────────── FINANCIAL SETUP ─────────────────────────────*/
(400, 'Financial Setup', 'SETUP', 'Setup financial system parameters', NULL, 0, 'fa-solid fa-wallet', 7300, 1, 1, NULL, 1, NULL),
(401, 'Chart of Accounts', NULL, 'Define accounting ledger structure', 'ChartOfAccountsDashboard', 400, 'fa-solid fa-project-diagram', 7310, 1, 1, NULL, 1, NULL),
(402, 'Banks', NULL, 'Setup banks and bank branches', 'BankDashboard', 400, 'fa-solid fa-university', 7320, 1, 1, NULL, 1, NULL),
(403, 'Currencies', NULL, 'Define currencies and exchange rates', 'CurrenciesDashboard', 400, 'fa-solid fa-coins', 7330, 1, 1, NULL, 1, NULL),
(404, 'Tax Master', NULL, 'Define taxes and rates', 'TaxMasterDashboard', 400, 'fa-solid fa-percent', 7340, 1, 1, NULL, 1, NULL),
(405, 'Tax Rules', NULL, 'Configure tax applicability and rules', 'TaxRuleDashboard', 400, 'fa-solid fa-balance-scale', 7350, 1, 1, NULL, 1, NULL),
(406, 'Payment Terms', NULL, 'Define payment terms and conditions', 'PaymentTermsDashboard', 400, 'fa-solid fa-file-contract', 7360, 1, 1, NULL, 1, NULL),
(407, 'Price Lists', NULL, 'Setup sales and purchase price lists', 'PriceListDashboard', 400, 'fa-solid fa-list-alt', 7370, 1, 1, NULL, 1, NULL),
(408, 'Services & Discounts', NULL, 'Configure service and discount charges for sale invoices', 'InvoiceChargesRulesDashboard', 400, 'fa-solid fa-concierge-bell', 7380, 1, 1, NULL, 1, NULL),
(409, 'Period Close', NULL, 'Manage accounting periods for GL posting', 'PeriodCloseDashboard', 400, 'fa-solid fa-calendar-alt', 7390, 1, 1, NULL, 1, NULL);

SET IDENTITY_INSERT dbo.Menu OFF;

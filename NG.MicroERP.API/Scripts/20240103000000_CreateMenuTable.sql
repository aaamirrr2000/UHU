CREATE TABLE Menu
(
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    MenuCaption         VARCHAR(100)    NULL DEFAULT NULL,
    AdditionalInfo      VARCHAR(100)    NULL DEFAULT NULL,
    Tooltip             VARCHAR(255)    NULL DEFAULT NULL,
    PageName            VARCHAR(100)    NULL DEFAULT NULL,
    ParentId            INT             NULL DEFAULT NULL,
    Icon                VARCHAR(50)     NULL DEFAULT NULL,
    SeqNo               INT             NULL DEFAULT NULL,
    Live                SMALLINT        NULL DEFAULT NULL,
    CreatedBy           INT             NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL,
    UpdatedOn           DATETIME        NULL DEFAULT NULL,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT        NOT NULL DEFAULT 0,
    RowVersion          ROWVERSION      NOT NULL
);

SET IDENTITY_INSERT Menu ON;

INSERT INTO Menu 
    (Id, MenuCaption, AdditionalInfo, Tooltip, PageName, ParentId, Icon, SeqNo, Live, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted)
VALUES 
(1, 'Sale', '', 'Manage sales transactions', NULL, 0, 'fas fa-shopping-cart', 1000, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(2, 'Bill', '', 'View and generate bills', 'BillDashboard', 1, 'fas fa-file-invoice-dollar', 1001, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(3, 'Cash Book', '', 'Track daily cash transactions', 'CashBookDashboard', 1, 'fas fa-wallet', 1002, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),

(4, 'Stock', '', 'Manage inventory and stock levels', NULL, 0, 'fas fa-boxes', 2000, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(5, 'Stock Received', '', 'Record received stock', 'StockPage', 4, 'fas fa-truck-loading', 2001, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),

(6, 'Reports', '', 'Generate and view reports', NULL, 0, 'fas fa-chart-line', 5000, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(7, 'Collection Report', 'Report', 'Analyze cash position', 'CollectionReport', 6, 'fas fa-hand-holding-usd', 5002, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(8, 'Cash Position Report', 'Report', 'View cash book transactions', 'CashPositionReport', 6, 'fas fa-money-bill-wave', 5004, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),

(9, 'Security', '', 'Manage user access and roles', NULL, 0, 'fas fa-shield-alt', 8000, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(10, 'Users', '', 'Manage application users', 'UsersDashboard', 9, 'fas fa-user', 8002, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(11, 'Groups', '', 'Manage user groups and roles', 'GroupsDashboard', 9, 'fas fa-users-cog', 8003, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(12, 'Permissions', '', 'Set menu permissions per user', 'PermissionsDashboard', 9, 'fas fa-lock', 8004, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),

(13, 'Setup', '', 'System settings and configuration', NULL, 0, 'fas fa-cogs', 9000, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(14, 'Employee', '', 'Manage employee details', 'EmployeesDashboard', 13, 'fas fa-id-badge', 9002, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(15, 'Locations', '', 'Manage office locations', 'LocationsDashboard', 13, 'fas fa-map-marker-alt', 9004, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(16, 'Parties', '', 'Manage customers and vendors', 'PartiesDashboard', 13, 'fas fa-address-book', 9005, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(17, 'Categories', '', 'Manage product categories', 'CategoriesDashboard', 13, 'fas fa-tags', 9006, 1, NULL, GETDATE(), NULL, NULL, GETDATE(), NULL, 0),
(18, 'Items', '', 'Manage product/Items/SKU', 'ItemsDashboard', 13, 'fas fa-tags', 9006, 1, NULL,  GETDATE(), NULL, NULL,  GETDATE(), NULL, 0);


SET IDENTITY_INSERT Menu OFF;

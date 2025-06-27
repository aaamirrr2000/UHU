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



SET IDENTITY_INSERT dbo.Menu ON;

INSERT INTO dbo.Menu
        (Id, MenuCaption, AdditionalInfo, Tooltip, PageName, ParentId,
         Icon,      SeqNo, Live,
         CreatedBy, CreatedOn, CreatedFrom,
         UpdatedBy, UpdatedOn, UpdatedFrom,
         IsSoftDeleted)
VALUES
/*── SALES ───────────────────────────────────────────────*/
(1 , 'Sale'      , NULL , 'Create and review sales transactions'
                       , NULL                        , 0
                       , 'fas fa-shopping-cart'      , 1000, 1
                       , 1 , GETDATE(), NULL
                       , 1 , GETDATE(), NULL
                       , 0),

(2 , 'Bill'      , NULL , 'Generate and view customer bills'
                       , 'BillDashboard'             , 1
                       , 'fas fa-file-invoice-dollar', 1100, 1
                       , 1 , GETDATE(), NULL
                       , 1 , GETDATE(), NULL
                       , 0),

(3 , 'Cash Book' , NULL , 'Enter and monitor cash receipts & payments'
                       , 'CashBookDashboard'         , 1
                       , 'fas fa-wallet'             , 1200, 1
                       , 1 , GETDATE(), NULL
                       , 1 , GETDATE(), NULL
                       , 0),

/*── STOCK ───────────────────────────────────────────────*/
(4 , 'Stock'           , NULL , 'Manage inventory levels and stock transactions'
                              , NULL                        , 0
                              , 'fas fa-boxes'             , 3000, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(5 , 'Stock Received'  , NULL , 'Record incoming stock receipts'
                              , 'StockPage'                 , 4
                              , 'fas fa-truck-loading'      , 3100, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

/*── REPORTS ─────────────────────────────────────────────*/
(6 , 'Reports'             , NULL , 'Generate operational and financial reports'
                                 , NULL                        , 0
                                 , 'fas fa-chart-line'         , 5000, 1
                                 , 1 , GETDATE(), NULL
                                 , 1 , GETDATE(), NULL
                                 , 0),

(7 , 'Collection Report'   , 'Report', 'Review incoming payments and collection status'
                                 , 'CollectionReport'          , 6
                                 , 'fas fa-hand-holding-usd'   , 5100, 1
                                 , 1 , GETDATE(), NULL
                                 , 1 , GETDATE(), NULL
                                 , 0),

(8 , 'Cash Position Report', 'Report', 'View daily cash in/out and current balance'
                                 , 'CashPositionReport'        , 6
                                 , 'fas fa-money-bill-wave'    , 5200, 1
                                 , 1 , GETDATE(), NULL
                                 , 1 , GETDATE(), NULL
                                 , 0),

/*── SECURITY ────────────────────────────────────────────*/
(9 , 'Security'        , NULL , 'Configure user security and permissions'
                              , NULL                        , 0
                              , 'fas fa-shield-alt'         , 8000, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(10, 'Users'           , NULL , 'Add, edit, and deactivate system users'
                              , 'UsersDashboard'            , 9
                              , 'fas fa-user'               , 8010, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(11, 'Groups'          , NULL , 'Create and manage user groups and roles'
                              , 'GroupsDashboard'           , 9
                              , 'fas fa-users-cog'          , 8020, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(12, 'Permissions'     , NULL , 'Assign menu permissions to users or groups'
                              , 'PermissionsDashboard'      , 9
                              , 'fas fa-lock'               , 8030, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

/*── SETUP / MASTER DATA ────────────────────────────────*/
(13, 'Setup'           , NULL , 'Configure core system settings and master data'
                              , NULL                        , 0
                              , 'fas fa-cogs'               , 9000, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(19, 'Organization'    , NULL , 'Maintain organisation profile and settings'
                              , 'OrganizationDashboard'     , 13
                              , 'fas fa-building'           , 9010, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(15, 'Locations'       , NULL , 'Maintain branch or warehouse locations'
                              , 'LocationsDashboard'        , 13
                              , 'fas fa-map-marker-alt'     , 9020, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(14, 'Employee'        , NULL , 'Maintain employee records and HR details'
                              , 'EmployeesDashboard'        , 13
                              , 'fas fa-id-badge'           , 9030, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(16, 'Parties'         , NULL , 'Maintain customer and vendor master data'
                              , 'PartiesDashboard'          , 13
                              , 'fas fa-address-book'       , 9040, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(17, 'Categories'      , NULL , 'Maintain product category list'
                              , 'CategoriesDashboard'       , 13
                              , 'fas fa-tags'               , 9050, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(18, 'Items'           , NULL , 'Add and manage individual products and SKUs'
                              , 'ItemsDashboard'            , 13
                              , 'fas fa-barcode'            , 9060, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(20, 'Service Charges' , NULL , 'Define service charge rates and rules'
                              , 'ServiceChargesDashboard'   , 13
                              , 'fas fa-receipt'            , 9070, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0),

(21, 'Tax'             , NULL , 'Define tax types and rates'
                              , 'TaxDashboard'              , 13
                              , 'fas fa-percentage'         , 9080, 1
                              , 1 , GETDATE(), NULL
                              , 1 , GETDATE(), NULL
                              , 0);

SET IDENTITY_INSERT dbo.Menu OFF;


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

INSERT INTO ChartOfAccounts
    (Id, Code, Name, Type, InterfaceType, Description, ParentId, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom)
VALUES
    (1,  '10000', 'ASSET',                        'ASSET',     NULL,                          NULL,                                 0,  1, NULL,                             1, NULL),
    (2,  '20000', 'LIABILITY',                    'LIABILITY', NULL,                          NULL,                                 0,  1, NULL,                             1, NULL),
    (3,  '30000', 'REVENUE',                      'REVENUE',   NULL,                          NULL,                                 0,  1, NULL,                             1, NULL),
    (4,  '40000', 'EXPENSE',                      'EXPENSE',   NULL,                          NULL,                                 0,  1, NULL,                             1, NULL),
    (5,  '50000', 'EQUITY',                       'EQUITY',    NULL,                          NULL,                                 0,  1, NULL,                             1, NULL),

    (6,  '30001', 'OTHER REVENUE',                'REVENUE',   'NONE',                        NULL,                                 3,  1, NULL,                             1, NULL),
    (7,  '40001', 'SALARIES',                     'EXPENSE',   'NONE',                        NULL,                                 4,  1, NULL,                             1, NULL),
    (8,  '40002', 'ELECTRICITY BILL',             'EXPENSE',   'NONE',                        NULL,                                 4,  1, NULL,                             1, NULL),

    (9,  '10001', 'BANKS',                        'ASSET',     'NONE',                        '',                                   51, 1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702'),
    (10, '10002', 'COLLECTION BANK',              'ASSET',     'BANK',                        NULL,                                 9,  1, NULL,                             1, NULL),
    (11, '10003', 'ACCOUNT RECEIVABLES',          'ASSET',     'NONE',                        '',                                   54, 1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200810'),
    (12, '10004', 'B2C CUSTOMER',                 'ASSET',     'ACCOUNTS RECEIVABLE',         NULL,                                 11, 1, NULL,                             1, NULL),
    (13, '10005', 'B2B CUSTOMER',                 'ASSET',     'ACCOUNTS RECEIVABLE',         NULL,                                 11, 1, NULL,                             1, NULL),

    (14, '20001', 'ACCOUNT PAYABLES',             'LIABILITY', 'NONE',                        '',                                   52, 1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702'),
    (15, '20002', 'SUPPLIER',                     'LIABILITY', NULL,                          NULL,                                 14, 1, NULL,                             1, NULL),
    (16, '20003', 'B2B SUPPLIER',                 'LIABILITY', 'ACCOUNTS PAYABLE',            NULL,                                 14, 1, NULL,                             1, NULL),
    (17, '20004', 'GENERAL SALES TAX',            'LIABILITY', 'TAX',                         NULL,                                 14, 1, NULL,                             1, NULL),

    (18, '10006', 'PAYMENT BANK',                 'ASSET',     'BANK',                        NULL,                                 9,  1, NULL,                             1, NULL),

    (19, '30001', 'ADVANCE TAX',                  'REVENUE',   'TAX',                         '',                                   3,  1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01210730'),
    (20, '30002', 'SERVICE CHARGES',              'REVENUE',   'SERVICE',                     'Delivery / Handling / Service Charges', 6,  1, NULL,                             1, NULL),
    (21, '30003', 'SALES DISCOUNT',               'REVENUE',   'DISCOUNT',                    'Invoice Level Discounts',            3,  1, NULL,                             1, NULL),

    (22, '10007', 'CASH',                         'ASSET',     'PAYMENT METHOD',              'PHYSICAL CASH',                      51, 1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200810'),
    (23, '10008', 'ONLINE BANK TRANSFER',         'ASSET',     'PAYMENT METHOD',              'ONLINE BANK TRANSFERS',              51, 1, NULL,                             1, 'ANONYMOUS|127.0.1|CHROME|WINDOWS 10/11|01200810'),
    (24, '10009', 'CREDIT CARD',                  'LIABILITY', 'PAYMENT METHOD',              'CREDIT CARD PAYMENTS',               52, 1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702'),
    (25, '10010', 'DEBIT CARD',                   'ASSET',     'PAYMENT METHOD',              'DEBIT CARD PAYMENTS',                51, 1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200810'),
    (26, '10011', 'JAZZCASH',                     'ASSET',     'PAYMENT METHOD',              'JAZZCASH MOBILE WALLET',             53, 1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200810'),
    (27, '10012', 'EASYPAISA',                    'ASSET',     'PAYMENT METHOD',              'EASYPAISA MOBILE WALLET',            53, 1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200810'),

    (28, '30004', 'SALES REVENUE',                'REVENUE',   'NONE',                        'Sales revenue account',              3,  1, NULL,                             1, NULL),
    (29, '30005', 'SERVICE REVENUE',              'REVENUE',   'NONE',                        'Service revenue account',            3,  1, NULL,                             1, NULL),

    (30, '40003', 'OPERATING EXPENSE',            'EXPENSE',   'NONE',                        'General operating expenses',         4,  1, NULL,                             1, NULL),
    (31, '40004', 'PURCHASE EXPENSE',             'EXPENSE',   'NONE',                        'Purchase and procurement expenses',  4,  1, NULL,                             1, NULL),

    (32, '20005', 'B2C SUPPLIER',                 'LIABILITY', 'ACCOUNTS PAYABLE',            'B2C supplier accounts payable',      14, 1, NULL,                             1, NULL),

    (33, '10013', 'ADVANCE AGAINST SALARY',       'ASSET',     'ADVANCE',                     'EMPLOYEE ADVANCES ACCOUNT',          50, 1, NULL,                             1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702'),

    (34, '40005', 'TAXI AND FUEL CHARGES',        'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|0.0.0.0|UNKNOWN|UNKNOWN|01191019', NULL, '2026-01-19 15:21:08.493'),

    (35, '10014', 'ADVANCE AGAINST PURCHASES',    'ASSET',     'ADVANCE',                     '',                                   50, 1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01191109', 1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702'),

    (36, '40006', 'STATIONERY',                   'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200426', NULL, '2026-01-20 09:26:45.007'),
    (37, '40007', 'REPAIR AND MAINTENANCE',       'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200426', NULL, '2026-01-20 09:27:58.190'),
    (38, '40008', 'EMPLOYEE EXPENSES',            'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200426', NULL, '2026-01-20 09:28:58.370'),
    (39, '40009', 'CHARITY',                      'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200446', NULL, '2026-01-20 09:46:44.177'),
    (40, '40010', 'SALARIES AND WAGES',           'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200446', NULL, '2026-01-20 09:47:19.187'),
    (41, '40011', 'PHOTOSTATE',                   'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200446', NULL, '2026-01-20 09:48:00.187'),
    (42, '40012', 'MISCELLANEOUS EXPENSES',       'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', NULL, '2026-01-20 12:21:18.903'),
    (43, '40013', 'DELIVERY EXPENSES',            'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', NULL, '2026-01-20 12:22:39.000'),
    (44, '40014', 'COURIER EXPENSES',             'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', NULL, '2026-01-20 12:22:55.540'),

    (45, '50001', 'CAPITAL PARTNER 1 (AAMIR)',    'EQUITY',    'NONE',                        '',                                   5,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', NULL, '2026-01-20 12:26:30.897'),
    (46, '50002', 'CAPITAL PARTNER 2 (TALHA)',    'EQUITY',    'NONE',                        '',                                   5,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', NULL, '2026-01-20 12:26:47.600'),

    (47, '40015', 'OFFICE EXPENSES',              'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', NULL, '2026-01-20 12:33:57.127'),
    (48, '40016', 'ENTERTAINMENT',                'EXPENSE',   'NONE',                        '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', NULL, '2026-01-20 12:36:07.703'),

    (50, '10016', 'ADVANCES',                     'ASSET',     'NONE',                        '',                                   54, 1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', 1,  '2026-01-20 13:15:06.000'),
    (51, '10017', 'CASH & CASH EQUIVALENTS',      'ASSET',     'NONE',                        '',                                   54, 1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', 1,  '2026-01-20 13:14:15.000'),
    (52, '20006', 'CURRENT LIABILITIES',          'LIABILITY', 'NONE',                        '',                                   2,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200702', NULL, '2026-01-20 13:06:12.360'),
    (53, '10018', 'MOBILE WALLET',                'ASSET',     'NONE',                        '',                                   51, 1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200810', NULL, '2026-01-20 13:11:33.300'),
    (54, '10019', 'CURRENT ASSETS',               'ASSET',     'NONE',                        '',                                   1,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01200810', NULL, '2026-01-20 13:13:58.910'),

    (55, '40017', 'COST OF MANUAL PURCHASES',     'EXPENSE',   'MANUAL ITEM EXPENSE',         '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01210730', NULL, '2026-01-21 12:47:06.530'),
    (56, '30006', 'REVENUE FROM MANUAL SALES',    'REVENUE',   'MANUAL ITEM REVENUE',         '',                                   3,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01210730', NULL, '2026-01-21 12:47:36.557'),
    (57, '40018', 'COST OF PURCHASES',            'EXPENSE',   'ITEM EXPENSE',                '',                                   4,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01210730', NULL, '2026-01-21 12:52:11.040'),
    (58, '30007', 'REVENUE OF SALES',             'REVENUE',   'ITEM REVENUE',                '',                                   3,  1, 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01210730', NULL, '2026-01-21 12:53:27.780');

SET IDENTITY_INSERT ChartOfAccounts OFF;

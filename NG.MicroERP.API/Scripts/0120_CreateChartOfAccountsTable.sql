CREATE TABLE dbo.ChartOfAccounts
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NULL,
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


SET IDENTITY_INSERT dbo.ChartOfAccounts ON;
GO

INSERT INTO dbo.ChartOfAccounts
(
    Id,
    OrganizationId,
    Code,
    Name,
    Type,
    InterfaceType,
    Description,
    ParentId,
    OpeningBalance,
    IsActive,
    CreatedBy,
    CreatedOn,
    CreatedFrom,
    UpdatedBy,
    UpdatedOn,
    UpdatedFrom,
    IsSoftDeleted
)
VALUES
(1, 1, '10000', 'ASSET', 'ASSET', NULL, NULL, 0, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(2, 1, '20000', 'LIABILITY', 'LIABILITY', NULL, NULL, 0, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(3, 1, '30000', 'REVENUE', 'REVENUE', NULL, NULL, 0, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(4, 1, '40000', 'EXPENSE', 'EXPENSE', NULL, NULL, 0, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(5, 1, '50000', 'EQUITY', 'EQUITY', NULL, NULL, 0, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(6, 1, '30001', 'OTHER REVENUE', 'REVENUE', 'REVENUE', NULL, 3, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(7, 1, '40001', 'SALARIES', 'EXPENSE', 'EXPENSE', NULL, 4, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(8, 1, '40002', 'ELECTRICITY BILL', 'EXPENSE', 'EXPENSE', NULL, 4, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(9, 1, '10001', 'BANKS', 'ASSET', NULL, NULL, 1, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(10, 1, '10002', 'COLLECTION BANK', 'ASSET', 'BANK', NULL, 9, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(11, 1, '10003', 'ACCOUNT RECEIVABLES', 'ASSET', NULL, NULL, 1, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(12, 1, '10004', 'B2C CUSTOMER', 'ASSET', 'ACCOUNTS RECEIVABLE', NULL, 11, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(13, 1, '10005', 'B2B CUSTOMER', 'ASSET', 'ACCOUNTS RECEIVABLE', NULL, 11, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(14, 1, '20001', 'ACCOUNT PAYABLES', 'LIABILITY', NULL, NULL, 2, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(15, 1, '20002', 'SUPPLIER', 'LIABILITY', NULL, NULL, 14, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(16, 1, '20003', 'B2B SUPPLIER', 'LIABILITY', 'ACCOUNTS PAYABLE', NULL, 14, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(17, 1, '20004', 'GENERAL SALES TAX', 'LIABILITY', 'TAX', NULL, 14, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(18, 1, '10006', 'PAYMENT BANK', 'ASSET', 'BANK', NULL, 9, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(19, 1, '30001', 'ADVANCE TAX', 'ASSET', 'TAX', NULL, 3, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(20, 1, '30002', 'SERVICE CHARGES', 'REVENUE', 'SERVICE', 'Delivery / Handling / Service Charges', 6, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(21, 1, '30003', 'SALES DISCOUNT', 'REVENUE', 'DISCOUNT', 'Invoice Level Discounts', 3, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(22, 1, '10007', 'CASH IN HAND', 'ASSET', 'PAYMENT METHOD', 'Physical cash', 9, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(23, 1, '10008', 'ONLINE BANK TRANSFER', 'PAYMENT METHOD', 'BANK', 'Online bank transfers', 9, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(24, 1, '10009', 'CREDIT CARD', 'ASSET', 'PAYMENT METHOD', 'Credit card payments', 9, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(25, 1, '10010', 'DEBIT CARD', 'ASSET', 'PAYMENT METHOD', 'Debit card payments', 9, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(26, 1, '10011', 'JAZZCASH', 'ASSET', 'PAYMENT METHOD', 'JazzCash mobile wallet', 9, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
(27, 1, '10012', 'EASYPAISA', 'ASSET', 'PAYMENT METHOD', 'Easypaisa mobile wallet', 9, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0);
GO

SET IDENTITY_INSERT dbo.ChartOfAccounts OFF;
GO

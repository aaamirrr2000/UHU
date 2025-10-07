CREATE TABLE ChartOfAccounts
(
  Id                      INT             PRIMARY KEY IDENTITY(1,1),
  Guid                    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
  OrganizationId          INT             NULL DEFAULT NULL,
  Pic                     VARCHAR(MAX)    NULL,
  Code                    VARCHAR(20)     NOT NULL,
  Name                    VARCHAR(100)    NOT NULL,
  Type                    VARCHAR(50)     NOT NULL,
  InterfaceType           VARCHAR(50)     NULL,
  Description             VARCHAR(100)    NULL,
  ParentId                INT             NULL DEFAULT 0,
  OpeningBalance          DECIMAL(15, 2)  DEFAULT 0.00,
  IsActive                SMALLINT        NOT NULL DEFAULT 1,
  CreatedBy               INT             NULL DEFAULT NULL,
  CreatedOn               DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CreatedFrom             VARCHAR(255)    NULL DEFAULT NULL,
  UpdatedBy               INT             NULL DEFAULT NULL,
  UpdatedOn               DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UpdatedFrom             VARCHAR(255)    NULL DEFAULT NULL,
  IsSoftDeleted           SMALLINT        NULL DEFAULT 0,
  RowVersion              ROWVERSION,
  FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
  FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
  FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
);

SET IDENTITY_INSERT ChartOfAccounts ON;

INSERT INTO ChartOfAccounts 
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
  (10, 1, '10002', 'ALLIED BANK', 'ASSET', 'BANK', NULL, 9, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
  (11, 1, '10003', 'ACCOUNT RECEIVABLES', 'ASSET', NULL, NULL, 1, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
  (12, 1, '10004', 'B2C CUSTOMER', 'ASSET', 'ACCOUNTS RECEIVABLE', NULL, 11, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
  (13, 1, '10005', 'B2B CUSTOMER', 'ASSET', 'ACCOUNTS RECEIVABLE', NULL, 11, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
  (14, 1, '20001', 'ACCOUNT PAYABLES', 'LIABILITY', NULL, NULL, 2, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
  (15, 1, '20002', 'SUPPLIER', 'LIABILITY', NULL, NULL, 14, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0),
  (16, 1, '20003', 'B2B SUPPLIER', 'LIABILITY', 'ACCOUNTS PAYABLE', NULL, 14, 0.00, 1, 1, GETDATE(), NULL, 1, GETDATE(), NULL, 0);

  SET IDENTITY_INSERT ChartOfAccounts OFF;
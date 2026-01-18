-- =============================================
-- Bank Reconciliation Module
-- =============================================

-- =============================================
-- 1. BankReconciliation Table
-- =============================================
CREATE TABLE dbo.BankReconciliation
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NOT NULL,
    ReconciliationNo    VARCHAR(50) NOT NULL,
    BankAccountId      INT NOT NULL, -- Chart of Accounts ID with InterfaceType='BANK'
    StatementDate       DATETIME NOT NULL,
    OpeningBalance      DECIMAL(18, 4) NOT NULL DEFAULT 0,
    StatementBalance    DECIMAL(18, 4) NOT NULL DEFAULT 0,
    BookBalance         DECIMAL(18, 4) NOT NULL DEFAULT 0,
    Difference          DECIMAL(18, 4) NOT NULL DEFAULT 0,
    Status              VARCHAR(50) NOT NULL DEFAULT 'OPEN', -- OPEN, RECONCILED, CLOSED
    Notes               VARCHAR(MAX) NULL,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_BankReconciliation_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations(Id),
    CONSTRAINT FK_BankReconciliation_BankAccount FOREIGN KEY (BankAccountId) REFERENCES dbo.ChartOfAccounts(Id),
    CONSTRAINT FK_BankReconciliation_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_BankReconciliation_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_BankReconciliation_No_Organization UNIQUE (ReconciliationNo, OrganizationId)
);
GO

-- =============================================
-- 2. BankReconciliationDetails Table
-- =============================================
CREATE TABLE dbo.BankReconciliationDetails
(
    Id                      INT IDENTITY(1,1) PRIMARY KEY,
    ReconciliationId        INT NOT NULL,
    TransactionType         VARCHAR(50) NOT NULL, -- DEPOSIT, WITHDRAWAL, CHARGE, INTEREST
    TransactionDate         DATETIME NOT NULL,
    ReferenceNo             VARCHAR(100) NULL,
    Description             VARCHAR(500) NULL,
    Amount                  DECIMAL(18, 4) NOT NULL,
    IsMatched               SMALLINT NOT NULL DEFAULT 0, -- 0 = Unmatched, 1 = Matched
    MatchedTransactionId    INT NULL, -- Reference to CashBook or GL entry
    MatchedTransactionType  VARCHAR(50) NULL, -- CASHBOOK, GENERALLEDGER
    Source                  VARCHAR(50) NOT NULL, -- STATEMENT, BOOK
    CreatedBy               INT NULL,
    CreatedOn               DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom             VARCHAR(255) NULL,
    UpdatedBy               INT NULL,
    UpdatedOn               DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom             VARCHAR(255) NULL,
    IsSoftDeleted           SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_BankReconciliationDetails_Reconciliation FOREIGN KEY (ReconciliationId) REFERENCES dbo.BankReconciliation(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BankReconciliationDetails_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_BankReconciliationDetails_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id)
);
GO

-- =============================================
-- Create Indexes for Performance
-- =============================================
CREATE INDEX IX_BankReconciliation_No ON dbo.BankReconciliation(ReconciliationNo, OrganizationId);
CREATE INDEX IX_BankReconciliation_BankAccount ON dbo.BankReconciliation(BankAccountId, StatementDate);
CREATE INDEX IX_BankReconciliation_Status ON dbo.BankReconciliation(Status, StatementDate);
CREATE INDEX IX_BankReconciliationDetails_Reconciliation ON dbo.BankReconciliationDetails(ReconciliationId, IsMatched);
CREATE INDEX IX_BankReconciliationDetails_Matched ON dbo.BankReconciliationDetails(MatchedTransactionId, MatchedTransactionType);
GO

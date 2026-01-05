-- =============================================
-- General Ledger Header Table Structure
-- Supports Double-Entry Bookkeeping with Header-Detail
-- =============================================

CREATE TABLE dbo.GeneralLedgerHeader
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NOT NULL    Default 1,
    EntryNo             VARCHAR(50) NOT NULL,              -- Unique entry number
    EntryDate           DATE NOT NULL,                     -- Transaction date
    Source              VARCHAR(50) NULL DEFAULT 'MANUAL', -- Source of entry (MANUAL, INVOICE, CASHBOOK, etc.)
    Description         VARCHAR(500) NULL,                 -- Entry description
    ReferenceNo         VARCHAR(100) NULL,                 -- External reference number
    ReferenceType       VARCHAR(50) NULL,                  -- Type of reference (Invoice, Payment, etc.)
    ReferenceId         INT NULL,                          -- ID of referenced document
    PartyId             INT NULL,                          -- Party/Customer/Supplier reference (nullable: not all entries have parties, e.g., depreciation, accruals)
    LocationId          INT NULL,                          -- Location/Branch reference
    TotalDebit          DECIMAL(18, 2) DEFAULT 0.00,       -- Total debit amount (sum of all detail debits)
    TotalCredit         DECIMAL(18, 2) DEFAULT 0.00,       -- Total credit amount (sum of all detail credits)
    IsReversed          SMALLINT DEFAULT 0,                -- Flag for reversed entries
    ReversedEntryNo     VARCHAR(50) NULL,                  -- Reference to reversed entry
    IsPosted            SMALLINT DEFAULT 0,                -- Posted status
    PostedDate          DATETIME NULL,                     -- Date when entry was posted
    PostedBy            INT NULL,                           -- User who posted the entry
    IsAdjusted          SMALLINT DEFAULT 0,                -- Flag for adjustment entries
    AdjustmentEntryNo   VARCHAR(50) NULL,                 -- Reference to original entry if adjusted
    FileAttachment      VARCHAR(255) NULL,                 -- Supporting document attachment
    Notes               VARCHAR(MAX) NULL,                 -- Additional notes
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_GeneralLedgerHeader_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations(Id),
    CONSTRAINT FK_GeneralLedgerHeader_Party FOREIGN KEY (PartyId) REFERENCES dbo.Parties(Id),
    CONSTRAINT FK_GeneralLedgerHeader_Location FOREIGN KEY (LocationId) REFERENCES dbo.Locations(Id),
    CONSTRAINT FK_GeneralLedgerHeader_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_GeneralLedgerHeader_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_GeneralLedgerHeader_PostedBy FOREIGN KEY (PostedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CHK_GeneralLedgerHeader_Balanced CHECK (
        TotalDebit = TotalCredit
    ) -- Ensures double-entry balance
);
GO

-- =============================================
-- General Ledger Detail Table Structure
-- Individual account lines for each header entry
-- =============================================

CREATE TABLE dbo.GeneralLedgerDetail
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    HeaderId            INT NOT NULL,                      -- Reference to GeneralLedgerHeader
    AccountId           INT NOT NULL,                      -- Chart of Accounts reference
    Description         VARCHAR(500) NULL,                 -- Line description
    DebitAmount         DECIMAL(18, 2) DEFAULT 0.00,      -- Debit amount (0 if credit entry)
    CreditAmount        DECIMAL(18, 2) DEFAULT 0.00,       -- Credit amount (0 if debit entry)
    PartyId             INT NULL,                          -- Party reference for this line
    LocationId          INT NULL,                          -- Location reference for this line
    CostCenterId        INT NULL,                          -- Cost center (if applicable)
    ProjectId           INT NULL,                          -- Project reference (if applicable)
    CurrencyId          INT NULL,                          -- Currency (if multi-currency)
    ExchangeRate        DECIMAL(18, 6) DEFAULT 1.000000,  -- Exchange rate for foreign currency
    SeqNo               INT DEFAULT 1,                     -- Sequence number for ordering
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_GeneralLedgerDetail_Header FOREIGN KEY (HeaderId) REFERENCES dbo.GeneralLedgerHeader(Id) ON DELETE CASCADE,
    CONSTRAINT FK_GeneralLedgerDetail_Account FOREIGN KEY (AccountId) REFERENCES dbo.ChartOfAccounts(Id),
    CONSTRAINT FK_GeneralLedgerDetail_Party FOREIGN KEY (PartyId) REFERENCES dbo.Parties(Id),
    CONSTRAINT FK_GeneralLedgerDetail_Location FOREIGN KEY (LocationId) REFERENCES dbo.Locations(Id),
    CONSTRAINT FK_GeneralLedgerDetail_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_GeneralLedgerDetail_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CHK_GeneralLedgerDetail_DebitCredit CHECK (
        (DebitAmount > 0 AND CreditAmount = 0) OR 
        (DebitAmount = 0 AND CreditAmount > 0) OR
        (DebitAmount = 0 AND CreditAmount = 0)
    ) -- Ensures either debit or credit, not both
);
GO

-- Create Indexes for Performance
CREATE INDEX IX_GeneralLedgerHeader_EntryNo ON dbo.GeneralLedgerHeader(EntryNo);
CREATE INDEX IX_GeneralLedgerHeader_EntryDate ON dbo.GeneralLedgerHeader(EntryDate);
CREATE INDEX IX_GeneralLedgerHeader_OrganizationId ON dbo.GeneralLedgerHeader(OrganizationId);
CREATE INDEX IX_GeneralLedgerHeader_PartyId ON dbo.GeneralLedgerHeader(PartyId);
CREATE INDEX IX_GeneralLedgerHeader_ReferenceType_ReferenceId ON dbo.GeneralLedgerHeader(ReferenceType, ReferenceId);
CREATE INDEX IX_GeneralLedgerHeader_IsPosted ON dbo.GeneralLedgerHeader(IsPosted);
CREATE INDEX IX_GeneralLedgerHeader_IsSoftDeleted ON dbo.GeneralLedgerHeader(IsSoftDeleted);

CREATE INDEX IX_GeneralLedgerDetail_HeaderId ON dbo.GeneralLedgerDetail(HeaderId);
CREATE INDEX IX_GeneralLedgerDetail_AccountId ON dbo.GeneralLedgerDetail(AccountId);
CREATE INDEX IX_GeneralLedgerDetail_IsSoftDeleted ON dbo.GeneralLedgerDetail(IsSoftDeleted);
GO

-- =============================================
-- General Ledger Summary View
-- Provides aggregated view of account balances
-- =============================================

CREATE VIEW dbo.GeneralLedgerSummary AS
SELECT 
    glh.OrganizationId,
    gld.AccountId,
    coa.Code AS AccountCode,
    coa.Name AS AccountName,
    coa.Type AS AccountType,
    glh.EntryDate,
    SUM(gld.DebitAmount) AS TotalDebit,
    SUM(gld.CreditAmount) AS TotalCredit,
    SUM(gld.DebitAmount - gld.CreditAmount) AS NetAmount,
    COUNT(DISTINCT glh.EntryNo) AS EntryCount
FROM dbo.GeneralLedgerDetail gld
INNER JOIN dbo.GeneralLedgerHeader glh ON gld.HeaderId = glh.Id
INNER JOIN dbo.ChartOfAccounts coa ON gld.AccountId = coa.Id
WHERE gld.IsSoftDeleted = 0 
    AND glh.IsSoftDeleted = 0
    AND glh.IsPosted = 1
GROUP BY 
    glh.OrganizationId,
    gld.AccountId,
    coa.Code,
    coa.Name,
    coa.Type,
    glh.EntryDate;
GO

-- =============================================
-- General Ledger Report View
-- Detailed view with related information
-- =============================================

CREATE VIEW dbo.GeneralLedgerReport AS
SELECT 
    gld.Id,
    glh.OrganizationId,
    glh.EntryNo,
    glh.EntryDate,
    gld.AccountId,
    coa.Code AS AccountCode,
    coa.Name AS AccountName,
    coa.Type AS AccountType,
    gld.DebitAmount,
    gld.CreditAmount,
    gld.Description,
    glh.ReferenceNo,
    glh.ReferenceType,
    glh.ReferenceId,
    gld.PartyId,
    p.Name AS PartyName,
    gld.LocationId,
    loc.Name AS LocationName,
    glh.IsPosted,
    glh.PostedDate,
    glh.PostedBy,
    u1.Username AS PostedByUser,
    glh.IsReversed,
    glh.ReversedEntryNo,
    glh.IsAdjusted,
    glh.AdjustmentEntryNo,
    glh.FileAttachment,
    glh.Notes,
    glh.CreatedBy,
    glh.CreatedOn,
    glh.CreatedFrom,
    u2.Username AS CreatedByUser,
    glh.UpdatedBy,
    glh.UpdatedOn,
    glh.UpdatedFrom,
    u3.Username AS UpdatedByUser
FROM dbo.GeneralLedgerDetail gld
INNER JOIN dbo.GeneralLedgerHeader glh ON gld.HeaderId = glh.Id
LEFT JOIN dbo.ChartOfAccounts coa ON gld.AccountId = coa.Id
LEFT JOIN dbo.Parties p ON gld.PartyId = p.Id
LEFT JOIN dbo.Locations loc ON gld.LocationId = loc.Id
LEFT JOIN dbo.Users u1 ON glh.PostedBy = u1.Id
LEFT JOIN dbo.Users u2 ON glh.CreatedBy = u2.Id
LEFT JOIN dbo.Users u3 ON glh.UpdatedBy = u3.Id
WHERE gld.IsSoftDeleted = 0 AND glh.IsSoftDeleted = 0;
GO

-- =============================================
-- Account Balance View
-- Shows running balance for each account
-- =============================================

CREATE VIEW dbo.AccountBalance AS
SELECT 
    glh.OrganizationId,
    gld.AccountId,
    coa.Code AS AccountCode,
    coa.Name AS AccountName,
    coa.Type AS AccountType,
    coa.OpeningBalance,
    SUM(gld.DebitAmount) AS TotalDebit,
    SUM(gld.CreditAmount) AS TotalCredit,
    (coa.OpeningBalance + SUM(gld.DebitAmount) - SUM(gld.CreditAmount)) AS CurrentBalance
FROM dbo.GeneralLedgerDetail gld
INNER JOIN dbo.GeneralLedgerHeader glh ON gld.HeaderId = glh.Id
INNER JOIN dbo.ChartOfAccounts coa ON gld.AccountId = coa.Id
WHERE gld.IsSoftDeleted = 0 
    AND glh.IsSoftDeleted = 0
    AND glh.IsPosted = 1
GROUP BY 
    glh.OrganizationId,
    gld.AccountId,
    coa.Code,
    coa.Name,
    coa.Type,
    coa.OpeningBalance;
GO


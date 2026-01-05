-- =============================================
-- Period Close Table Structure
-- Manages accounting periods for GL, Invoice, and CashBook posting
-- =============================================

CREATE TABLE dbo.PeriodClose
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NOT NULL,
    PeriodName          VARCHAR(50) NOT NULL,              -- e.g., "2024-01", "Jan-2024"
    ModuleType          VARCHAR(50) NOT NULL DEFAULT 'ALL', -- ALL, STOCK, INVOICE, CASHBOOK, GENERALLEDGER
    StartDate           DATE NOT NULL,                      -- Period start date
    EndDate             DATE NOT NULL,                      -- Period end date
    Status              VARCHAR(50) DEFAULT 'OPEN',          -- OPEN, CLOSE, OPEN_PENDING
    ClosedDate          DATETIME NULL,                      -- Date when period was closed
    ClosedBy            INT NULL,                            -- User who closed the period
    Notes               VARCHAR(MAX) NULL,                  -- Additional notes
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NULL DEFAULT 0,
    
    CONSTRAINT FK_PeriodClose_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations(Id),
    CONSTRAINT FK_PeriodClose_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_PeriodClose_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_PeriodClose_ClosedBy FOREIGN KEY (ClosedBy) REFERENCES dbo.Users(Id)
);
GO

-- Add IsPostedToGL to Invoice table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Invoice') AND name = 'IsPostedToGL')
BEGIN
    ALTER TABLE dbo.Invoice
    ADD IsPostedToGL SMALLINT DEFAULT 0,
        PostedToGLDate DATETIME NULL,
        PostedToGLBy INT NULL,
        GLEntryNo VARCHAR(50) NULL;
    
    ALTER TABLE dbo.Invoice
    ADD CONSTRAINT FK_Invoice_PostedToGLBy FOREIGN KEY (PostedToGLBy) REFERENCES dbo.Users(Id);
END
GO

-- Add IsPostedToGL to Cashbook table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Cashbook') AND name = 'IsPostedToGL')
BEGIN
    ALTER TABLE dbo.Cashbook
    ADD IsPostedToGL SMALLINT DEFAULT 0,
        PostedToGLDate DATETIME NULL,
        PostedToGLBy INT NULL,
        GLEntryNo VARCHAR(50) NULL;
    
    ALTER TABLE dbo.Cashbook
    ADD CONSTRAINT FK_Cashbook_PostedToGLBy FOREIGN KEY (PostedToGLBy) REFERENCES dbo.Users(Id);
END
GO



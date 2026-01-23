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

INSERT INTO PeriodClose
    (OrganizationId, PeriodName, ModuleType, StartDate, EndDate, Status, ClosedDate, ClosedBy, Notes, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted) VALUES
    (1, 'YEAR 2025-26', 'ALL', '2025-07-01', '2026-06-30', 'OPEN', NULL, NULL, NULL, 1, '2026-01-22 11:57:20.000', 'ANONYMOUS|127.0.0.1|CHROME|WINDOWS 10/11|01220656', NULL, '2026-01-22 11:57:20.600', NULL, 0);

CREATE TABLE PaymentTerms
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Code            VARCHAR(20) NOT NULL UNIQUE,      -- e.g. CASH, NET30, ADV, COD
    Description     VARCHAR(255) NOT NULL,            -- e.g. Cash Payment, Net 30 Days, Advance Payment, etc.
    DaysDue         INT NULL,                         -- No. of days from invoice date
    IsDefault       BIT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1,

    CreatedBy       INT NULL,
    CreatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedBy       INT NULL,
    UpdatedOn       DATETIME NULL,
    IsSoftDeleted   BIT NOT NULL DEFAULT 0
);
GO

INSERT INTO PaymentTerms (Code, Description, DaysDue, IsDefault)
VALUES
('CASH',  'Cash Payment on Delivery', NULL, 0),
('ADV',   'Advance Payment', NULL, 0),
('NET15', 'Net 15 Days Credit', 15, 0),
('NET30', 'Net 30 Days Credit', 30, 1),
('COD',   'Cash on Delivery', NULL, 0);
GO
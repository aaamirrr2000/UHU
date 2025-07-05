CREATE TABLE dbo.Tax
(

    Id              INT              IDENTITY(1,1) PRIMARY KEY,
    GUID             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    TaxName         NVARCHAR(100)    NOT NULL,                 -- e.g. “GST”
    TaxCode         NVARCHAR(20)     NOT NULL UNIQUE,          -- short code for invoices
    RatePercent     DECIMAL(9,4)     NOT NULL,                 -- 17.0000  = 17 %
    IsCompound      BIT              NOT NULL DEFAULT 0,       -- 1 = applied on top of other tax
    AppliesTo       VARCHAR(20)      NOT NULL,                 -- “SERVICE”, “GOODS”, “ALL”
    EffectiveFrom   DATE            NOT NULL,
    EffectiveTo     DATE            NULL,
    IsActive        BIT              NOT NULL DEFAULT 1,

    CreatedBy       INT              NULL,
    CreatedOn       DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(255)     NULL,

    UpdatedBy       INT              NULL,
    UpdatedOn       DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom     VARCHAR(255)     NULL,

    IsSoftDeleted   BIT              NOT NULL DEFAULT 0,
    RowVersion      ROWVERSION
);

/* 1)  General Sales Tax – standard 18 % */
INSERT INTO dbo.Tax
        (TaxName,      TaxCode, RatePercent, IsCompound, AppliesTo, EffectiveFrom, EffectiveTo,
         IsActive,
         CreatedBy,    CreatedOn,     CreatedFrom, 
         UpdatedBy,    UpdatedOn,     UpdatedFrom,
         IsSoftDeleted)
VALUES  ('GST (Pakistan)', 'GST', 18.0000, 0, 'ALL', '2025-01-01', '2025-12-31',
         DEFAULT,         
         1,            DEFAULT,      '10.0.0.15',
         1,            DEFAULT,      '10.0.0.15',
         DEFAULT);                       


/* 2)  Provincial Services Tax – non-compound 15 % */
INSERT INTO dbo.Tax
        (TaxName,              TaxCode, RatePercent, IsCompound, AppliesTo, EffectiveFrom, EffectiveTo,
         IsActive,
         CreatedBy,            CreatedOn,     CreatedFrom,
         UpdatedBy,            UpdatedOn,     UpdatedFrom,
         IsSoftDeleted)
VALUES  ('Provincial Services Tax', 'PST', 15.0000, 0, 'SERVICE', '2025-01-01', '2025-12-31',
         DEFAULT,
         2,                    DEFAULT,      '10.0.0.23',
         2,                    DEFAULT,      '10.0.0.23',
         DEFAULT);

CREATE TABLE TaxMaster (
    Id              INT                 IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT                 DEFAULT 1,
    TaxType         VARCHAR(100)        NOT NULL,  -- e.g., "VAT", "GST"
    TaxName         VARCHAR(255)        NOT NULL,  -- e.g., "Standard VAT", "Reduced Rate VAT"
    TaxRate         DECIMAL(5,2)        NOT NULL,  -- Percentage (e.g., 17.00)
    IsActive        INT                 NOT NULL DEFAULT 1,

    CreatedBy       INT                 NULL,
    CreatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(255)        NULL,

    UpdatedBy       INT                 NULL,
    UpdatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom     VARCHAR(255)        NULL,

    IsSoftDeleted   INT                 NOT NULL DEFAULT 0
);




CREATE TABLE TaxItems
(
    Id                INT             IDENTITY(1,1) PRIMARY KEY,
    OrganizationId    INT             DEFAULT 1,
    TaxId             INT             NOT NULL,      -- References TaxMaster
    ItemId            INT             NOT NULL,      -- References Items
    InvoiceType       VARCHAR(50)     NOT NULL,      -- e.g., Sale, Purchase, Sale Return, etc.
    ApplicableFrom    DATE            NOT NULL DEFAULT GETDATE(), -- Effective from date for this item tax
    IsActive          INT             NOT NULL DEFAULT 1,

    CreatedBy         INT             NULL,
    CreatedOn         DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom       VARCHAR(255)    NULL,

    UpdatedBy         INT             NULL,
    UpdatedOn         DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom       VARCHAR(255)    NULL,

    IsSoftDeleted     INT             NOT NULL DEFAULT 0,

    FOREIGN KEY (TaxId)   REFERENCES TaxMaster(Id),
    FOREIGN KEY (ItemId)  REFERENCES Items(Id)
);
GO



CREATE TABLE TaxItems (
    Id              INT                 IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT                 DEFAULT 1,
    TaxId           INT,
    ItemId          INT,
    Invoicetype     VARCHAR(50),
    IsActive        INT                 NOT NULL DEFAULT 1,

    CreatedBy       INT                 NULL,
    CreatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(255)        NULL,

    UpdatedBy       INT                 NULL,
    UpdatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom     VARCHAR(255)        NULL,

    IsSoftDeleted   INT                 NOT NULL DEFAULT 0,
    FOREIGN KEY (TaxId)   REFERENCES TaxMaster(Id),
    FOREIGN KEY (ItemId)   REFERENCES Items(Id)
);




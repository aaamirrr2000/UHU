CREATE TABLE TaxMaster
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT             NOT NULL DEFAULT 1,
    AccountId           INT             NULL DEFAULT 17,
    TaxName VARCHAR(100) NOT NULL,
    TaxType VARCHAR(20) NOT NULL,        -- PERCENTAGE / FLAT
    TaxBaseType VARCHAR(50) NOT NULL,    -- BASE_ONLY / BASE_PLUS_SELECTED / RUNNING_TOTAL
    Rate DECIMAL(18,4) NOT NULL,         -- Tax rate or fixed amount
    Description VARCHAR(255) NULL,
    IsActive            SMALLINT        NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL,
    UpdatedBy           INT             NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL,
    IsSoftDeleted       SMALLINT        NOT NULL DEFAULT 0,
 
    FOREIGN KEY (OrganizationId)    REFERENCES Organizations(Id),
    FOREIGN KEY (AccountId)         REFERENCES ChartOfAccounts(Id),
    FOREIGN KEY (CreatedBy)         REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)         REFERENCES Users(Id)
);

INSERT INTO TaxMaster
(
    TaxName,
    AccountId,
    TaxType,
    TaxBaseType,
    Rate,
    Description
)
VALUES
('GST @ 18%',              17, 'PERCENTAGE', 'BASE_ONLY',      18.0000, 'Goods and Services Tax, applicable to all'),
('FED @ 5%',               17, 'PERCENTAGE', 'BASE_PLUS_SELECTED', 5.0000, 'Federal Excise Duty, same for all scenarios'),
('LUXURY TAX 12%',         17, 'PERCENTAGE', 'RUNNING_TOTAL', 12.0000, 'Luxury Tax, same for all scenarios'),
('ENVIRONMENTAL FEE 150',  17, 'FLAT',       'BASE_ONLY',     150.0000, 'Environmental protection fee, same for all scenarios');

-- WHT
INSERT INTO TaxMaster
(
    TaxName,
    AccountId,
    TaxType,
    TaxBaseType,
    Rate,
    Description
)
VALUES
('WHT FILER 10%',            14, 'PERCENTAGE', 'BASE_ONLY', 10.0000, 'Withholding Tax for filers (registered or not)'),
('WHT NON FILER 15%',        14, 'PERCENTAGE', 'BASE_ONLY', 15.0000, 'Withholding Tax for non-filers');

-- ADVANCE TAX
INSERT INTO TaxMaster
(
    TaxName,
    AccountId,
    TaxType,
    TaxBaseType,
    Rate,
    Description
)
VALUES
('ADVANCE TAX FILER 8%',     19, 'PERCENTAGE', 'BASE_ONLY', 8.0000, 'Advance Tax for filers'),
('ADVANCE TAX NON FILER 12%',19, 'PERCENTAGE', 'BASE_ONLY', 12.0000, 'Advance Tax for non-filers');


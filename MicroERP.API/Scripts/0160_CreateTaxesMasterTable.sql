CREATE TABLE TaxMaster
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT             NOT NULL DEFAULT 1,
    AccountId           INT             NULL DEFAULT 17,
    TaxName             VARCHAR(100) NOT NULL,
    Description         VARCHAR(255) NULL,
    ConditionType       VARCHAR(255) NULL,          -- Condition type text (e.g., "Filer and Register Check Applied", "Any Other Check is Applied")
    IsActive            SMALLINT        NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL,
    UpdatedBy           INT             NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL,
    IsSoftDeleted       SMALLINT        NOT NULL DEFAULT 0,
 
    CONSTRAINT FK_TaxMaster_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_TaxMaster_Account FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(Id),
    CONSTRAINT FK_TaxMaster_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_TaxMaster_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- AccountId must match ChartOfAccounts with InterfaceType='TAX': 17=GENERAL SALES TAX (LIABILITY), 19=ADVANCE TAX (REVENUE)
SET IDENTITY_INSERT TaxMaster ON;

INSERT INTO TaxMaster (Id, OrganizationId, AccountId, TaxName, Description, ConditionType, IsActive, CreatedBy, UpdatedBy)
VALUES
(1, 1, 17, 'Sales Tax', 'General sales tax on taxable value (base only). Rate and conditions in tax rule.', 'Registered seller only', 1, 1, 1),
(2, 1, 19, 'Advance Tax', 'Advance income tax withheld. Rate (5%/10%/12%) and conditions in tax rule.', 'Filer/Non-Filer, Registered/Unregistered', 1, 1, 1);

SET IDENTITY_INSERT TaxMaster OFF;
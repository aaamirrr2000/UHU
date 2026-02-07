CREATE TABLE TaxRule
(
    Id                  INT             IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT             NOT NULL DEFAULT 1,
    RuleName            VARCHAR(100)    NOT NULL,      -- Descriptive rule name
    AppliesTo           VARCHAR(20)     NOT NULL,      -- SALE / PURCHASE
    EffectiveFrom       DATE            NOT NULL,
    EffectiveTo         DATE            NULL,
    IsActive            SMALLINT        NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL,
    UpdatedBy           INT             NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL,
    IsSoftDeleted       SMALLINT        NOT NULL DEFAULT 0,
    CONSTRAINT FK_TaxRule_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_TaxRule_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_TaxRule_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);
GO


CREATE TABLE TaxRuleDetail
(
    Id                  INT             IDENTITY(1,1) PRIMARY KEY,
    TaxRuleId           INT             NOT NULL,
    TaxId               INT             NOT NULL,
    SequenceNo          INT             NOT NULL,      -- Order of tax application
    TaxType             VARCHAR(20)     NOT NULL,      -- PERCENTAGE / FLAT (required in rule detail)
    TaxBaseType         VARCHAR(50)     NOT NULL,      -- BASE_ONLY / BASE_PLUS_SELECTED / RUNNING_TOTAL (required in rule detail)
    TaxAmount           DECIMAL(18,4)   NOT NULL,      -- Tax rate/amount (required in rule detail)
    IsRegistered        INT             NULL,          -- NULL = N/A, 1 = Registered, 0 = Unregistered
    IsFiler             INT             NULL,          -- NULL = N/A, 1 = Filer, 0 = Non-Filer
    CONSTRAINT FK_TaxRuleDetail_TaxRule FOREIGN KEY (TaxRuleId) REFERENCES TaxRule(Id),
    CONSTRAINT FK_TaxRuleDetail_TaxMaster FOREIGN KEY (TaxId) REFERENCES TaxMaster(Id)
);
GO


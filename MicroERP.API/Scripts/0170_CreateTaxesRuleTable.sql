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


SET IDENTITY_INSERT TaxRule ON;

-- Rule 1: No Tax (no details; assign to categories/items for non-taxable)
INSERT INTO TaxRule (Id, OrganizationId, RuleName, AppliesTo, EffectiveFrom, EffectiveTo, IsActive, CreatedBy, UpdatedBy)
VALUES
(1, 1, 'No Tax', 'SALE', '2025-01-01', NULL, 1, 1, 1),
(2, 1, 'Tax on Grocery', 'SALE', '2025-01-01', NULL, 1, 1, 1);

SET IDENTITY_INSERT TaxRule OFF;

GO

SET IDENTITY_INSERT TaxRuleDetail ON;

-- Rule 2 only: Sales Tax 18% BASE_ONLY + Advance Tax BASE_PLUS_SELECTED (5%/10%/12% by Filer/Registered)
-- Sequence 1: Sales Tax 18% — BASE_ONLY; only when seller is REGISTERED
INSERT INTO TaxRuleDetail (Id, TaxRuleId, TaxId, SequenceNo, TaxType, TaxBaseType, TaxAmount, IsRegistered, IsFiler)
VALUES (1, 2, 1, 1, 'PERCENTAGE', 'BASE_ONLY', 18.0000, 1, NULL);

-- Sequence 2: Advance Tax 5% — BASE_PLUS_SELECTED; Filer + Registered (TaxId=2)
INSERT INTO TaxRuleDetail (Id, TaxRuleId, TaxId, SequenceNo, TaxType, TaxBaseType, TaxAmount, IsRegistered, IsFiler)
VALUES (2, 2, 2, 2, 'PERCENTAGE', 'BASE_PLUS_SELECTED', 5.0000, 1, 1);

-- Sequence 3: Advance Tax 10% — BASE_PLUS_SELECTED; Non-Filer + Registered (TaxId=2)
INSERT INTO TaxRuleDetail (Id, TaxRuleId, TaxId, SequenceNo, TaxType, TaxBaseType, TaxAmount, IsRegistered, IsFiler)
VALUES (3, 2, 2, 3, 'PERCENTAGE', 'BASE_PLUS_SELECTED', 10.0000, 1, 0);

-- Sequence 4: Advance Tax 12% — BASE_PLUS_SELECTED; Non-Filer + Unregistered (TaxId=2)
INSERT INTO TaxRuleDetail (Id, TaxRuleId, TaxId, SequenceNo, TaxType, TaxBaseType, TaxAmount, IsRegistered, IsFiler)
VALUES (4, 2, 2, 4, 'PERCENTAGE', 'BASE_PLUS_SELECTED', 12.0000, 0, 0);

SET IDENTITY_INSERT TaxRuleDetail OFF;
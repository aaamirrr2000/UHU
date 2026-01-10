
CREATE TABLE TaxRule
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT             NOT NULL DEFAULT 1,
    RuleName VARCHAR(100) NOT NULL,          -- Descriptive rule name
    AppliesTo VARCHAR(20) NOT NULL,          -- SALE / PURCHASE
    IsRegistered INT NULL,                    -- 1=Registered, 0=Unregistered, NULL=All
    IsFiler INT NULL,                         -- 1=Filer, 0=Non-Filer, NULL=All
    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE NULL,
    IsActive            SMALLINT        NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL,
    UpdatedBy           INT             NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL,
    IsSoftDeleted       SMALLINT        NOT NULL DEFAULT 0,
 
    FOREIGN KEY (OrganizationId)    REFERENCES Organizations(Id),
    FOREIGN KEY (CreatedBy)         REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)         REFERENCES Users(Id)
);

-- GST
INSERT INTO TaxRule
(
    OrganizationId,
    RuleName,
    AppliesTo,
    IsRegistered,
    IsFiler,
    EffectiveFrom,
    EffectiveTo,
    IsActive
)
VALUES
(1, 'GST 18% SALE', 'SALE', NULL, NULL, '2025-07-01', NULL, 1);


--FILER + REGISTERED
INSERT INTO TaxRule
(
    OrganizationId,
    RuleName,
    AppliesTo,
    IsRegistered,
    IsFiler,
    EffectiveFrom,
    EffectiveTo,
    IsActive
)
VALUES
(1, 'WHT FILER REGISTERED 10% SALE', 'SALE', 1, 1, '2025-07-01', NULL, 1),
(1, 'ADVANCE TAX FILER REGISTERED 8% SALE', 'SALE', 1, 1, '2025-07-01', NULL, 1);

--FILER ONLY (Not necessarily registered)
INSERT INTO TaxRule
(
    OrganizationId,
    RuleName,
    AppliesTo,
    IsRegistered,
    IsFiler,
    EffectiveFrom,
    EffectiveTo,
    IsActive
)
VALUES
(1, 'WHT FILER 10% SALE', 'SALE', NULL, 1, '2025-07-01', NULL, 1),
(1, 'ADVANCE TAX FILER 8% SALE', 'SALE', NULL, 1, '2025-07-01', NULL, 1);

--NON-FILER + NOT REGISTERED
INSERT INTO TaxRule
(
    OrganizationId,
    RuleName,
    AppliesTo,
    IsRegistered,
    IsFiler,
    EffectiveFrom,
    EffectiveTo,
    IsActive
)
VALUES
(1, 'WHT NON FILER 15% SALE', 'SALE', 0, 0, '2025-07-01', NULL, 1),
(1, 'ADVANCE TAX NON FILER 12% SALE', 'SALE', 0, 0, '2025-07-01', NULL, 1);


CREATE TABLE TaxRuleDetail
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaxRuleId INT NOT NULL,
    TaxId INT NOT NULL,
    SequenceNo INT NOT NULL,   -- order of tax application
    CONSTRAINT FK_TaxRuleDetail_TaxRule FOREIGN KEY (TaxRuleId) REFERENCES TaxRule(Id),
    CONSTRAINT FK_TaxRuleDetail_TaxMaster FOREIGN KEY (TaxId) REFERENCES TaxMaster(Id)
);

--FILER + REGISTERED (TaxRuleId = 1)
INSERT INTO TaxRuleDetail (TaxRuleId, TaxId, SequenceNo)
VALUES
(1, 1, 1),   -- GST
(1, 2, 2),   -- FED
(1, 3, 3),   -- Luxury Tax
(1, 4, 4),   -- Environmental Fee
(1, 5, 5),   -- WHT FILER 10%
(1, 7, 6);   -- ADVANCE TAX FILER 8%

--\FILER ONLY (TaxRuleId = 2)
INSERT INTO TaxRuleDetail (TaxRuleId, TaxId, SequenceNo)
VALUES
(2, 1, 1),   -- GST
(2, 2, 2),   -- FED
(2, 3, 3),   -- Luxury Tax
(2, 4, 4),   -- Environmental Fee
(2, 5, 5),   -- WHT FILER 10%
(2, 7, 6);   -- ADVANCE TAX FILER 8%

--NON-FILER + NOT REGISTERED (TaxRuleId = 3)
INSERT INTO TaxRuleDetail (TaxRuleId, TaxId, SequenceNo)
VALUES
(3, 1, 1),   -- GST
(3, 2, 2),   -- FED
(3, 3, 3),   -- Luxury Tax
(3, 4, 4),   -- Environmental Fee
(3, 6, 5),   -- WHT NON FILER 15%
(3, 8, 6);   -- ADVANCE TAX NON FILER 12%

--GST ONLY RULE (TaxRuleId = 4)
INSERT INTO TaxRuleDetail (TaxRuleId, TaxId, SequenceNo)
VALUES
(4, 1, 1);   -- GST

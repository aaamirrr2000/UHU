CREATE TABLE ChargeRules
(
    Id              INT                 IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT                 DEFAULT 1,

    RuleName        NVARCHAR(100)       NOT NULL,                        -- e.g. “VAT”, “Discount”, “Delivery Fee”
    RuleType        VARCHAR(50)         NOT NULL CHECK (RuleType IN ('CHARGE', 'DISCOUNT')),  
                                                                          -- CHARGE = add to total, DISCOUNT = subtract
    AmountType      VARCHAR(50)         NOT NULL CHECK (AmountType IN ('FLAT', 'PERCENTAGE')),
                                                                          -- FLAT = fixed value, PERCENTAGE = % based
    Amount          DECIMAL(18,4)       NOT NULL,                         -- 100.0000 (flat) | 5.0000 (%)

    AppliesTo       INT                 NULL DEFAULT 0,                  -- Optional: Link to category/item types
    CalculationBase VARCHAR(50)         NOT NULL CHECK (CalculationBase IN ('BILLED_AMOUNT', 'AFTER_PREVIOUS_CHARGES')),
                                                                          -- Determines base for % calculation
    SequenceOrder   INT                 NOT NULL DEFAULT 1,              -- Lower number = applied first
    ChargeCategory  VARCHAR(50)         NOT NULL DEFAULT 'OTHER' CHECK (ChargeCategory IN ('SERVICE', 'TAX', 'DISCOUNT', 'OTHER')),

    EffectiveFrom   DATE                NOT NULL,
    EffectiveTo     DATE                NULL,
    IsActive        BIT                 NOT NULL DEFAULT 1,

    CreatedBy       INT                 NULL,
    CreatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(255)        NULL,

    UpdatedBy       INT                 NULL,
    UpdatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom     VARCHAR(255)        NULL,

    IsSoftDeleted   BIT                 NOT NULL DEFAULT 0
);

INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom, EffectiveTo)
VALUES ('Service Fee', 'CHARGE', 'FLAT', 100.00, 2, 'AFTER_PREVIOUS_CHARGES', 'SERVICE', '2025-01-01', '2026-12-31');

INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom, EffectiveTo)
VALUES ('GST', 'CHARGE', 'PERCENTAGE', 17.00, 2, 'BILLED_AMOUNT', 'TAX', '2025-01-01', '2026-12-31');

INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom, EffectiveTo)
VALUES ('Provisional Tax', 'CHARGE', 'PERCENTAGE', 2.50, 98, 'AFTER_PREVIOUS_CHARGES', 'TAX', '2025-01-01', '2026-12-31');
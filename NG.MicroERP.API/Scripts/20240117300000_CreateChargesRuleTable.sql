CREATE TABLE ChargeRules
(
    Id              INT                 IDENTITY(1,1) PRIMARY KEY,
    GUID            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
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

    IsSoftDeleted   BIT                 NOT NULL DEFAULT 0,
    RowVersion      ROWVERSION
);

-- Flat Charge: Delivery Fee (applied first, fixed 200)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('Delivery Fee', 'CHARGE', 'FLAT', 200.00, 1, 'BILLED_AMOUNT', 'SERVICE', '2025-01-01');

-- Flat Charge: Service Fee (applied second, after previous charges)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('Service Fee', 'CHARGE', 'FLAT', 100.00, 2, 'AFTER_PREVIOUS_CHARGES', 'SERVICE', '2025-01-01');

-- Percentage Charge: VAT (15% on original billed amount)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('VAT', 'CHARGE', 'PERCENTAGE', 15.00, 3, 'BILLED_AMOUNT', 'TAX', '2025-01-01');

-- Percentage Charge: GST (17% on billed amount, applied before VAT)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('GST', 'CHARGE', 'PERCENTAGE', 17.00, 2, 'BILLED_AMOUNT', 'TAX', '2025-01-01');

-- Percentage Charge: Environmental Fee (1.5% on amount after previous charges)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('Environmental Fee', 'CHARGE', 'PERCENTAGE', 1.50, 4, 'AFTER_PREVIOUS_CHARGES', 'TAX', '2025-01-01');

-- Percentage Charge: Provisional Tax (2.5% on amount after all charges)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('Provisional Tax', 'CHARGE', 'PERCENTAGE', 2.50, 98, 'AFTER_PREVIOUS_CHARGES', 'TAX', '2025-01-01');

-- Flat Discount: Promo Discount (applied before all charges, fixed 250)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('Promo Discount', 'DISCOUNT', 'FLAT', 250.00, 0, 'BILLED_AMOUNT', 'DISCOUNT', '2025-01-01');

-- Flat Discount: Coupon Discount (applied near the end, fixed 100)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('Coupon Discount', 'DISCOUNT', 'FLAT', 100.00, 99, 'AFTER_PREVIOUS_CHARGES', 'DISCOUNT', '2025-01-01');

-- Percentage Discount: Loyalty Discount (5% on original billed amount)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('Loyalty Discount', 'DISCOUNT', 'PERCENTAGE', 5.00, 2, 'BILLED_AMOUNT', 'DISCOUNT', '2025-01-01');

-- Percentage Discount: Festival Discount (10% on final amount after all charges)
INSERT INTO ChargeRules (RuleName, RuleType, AmountType, Amount, SequenceOrder, CalculationBase, ChargeCategory, EffectiveFrom)
VALUES ('Festival Discount', 'DISCOUNT', 'PERCENTAGE', 10.00, 100, 'AFTER_PREVIOUS_CHARGES', 'DISCOUNT', '2025-01-01');

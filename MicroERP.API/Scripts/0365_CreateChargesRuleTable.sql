CREATE TABLE ChargesRules
(
    Id              INT                 IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT                 DEFAULT 1,
    AccountId       INT                 NOT NULL,                        -- e.g. “Discount”, “Delivery Fee”
    AmountType      VARCHAR(50)         NOT NULL CHECK (AmountType IN ('FLAT', 'PERCENTAGE')),
    Amount          DECIMAL(18,4)       NOT NULL,                         -- 100.0000 (flat) | 5.0000 (%)
    ChargeCategory  VARCHAR(50)         NOT NULL DEFAULT 'OTHER' CHECK (ChargeCategory IN ('SERVICE', 'DISCOUNT')),
    EffectiveFrom   DATE                NOT NULL,
    EffectiveTo     DATE                NULL,
    IsActive        INT                 NOT NULL DEFAULT 1,
    CreatedBy       INT                 NULL,
    CreatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(255)        NULL,
    UpdatedBy       INT                 NULL,
    UpdatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom     VARCHAR(255)        NULL,
    IsSoftDeleted   INT                 NOT NULL DEFAULT 0
);

INSERT INTO ChargesRules
(
    AccountId,
    AmountType,
    Amount,
    ChargeCategory,
    EffectiveFrom,
    EffectiveTo,
    IsActive
)
VALUES
(20, 'FLAT',        250.0000, 'SERVICE',  '2026-01-01', NULL,          1), 
(20, 'PERCENTAGE',   3.5000, 'DISCOUNT',  '2026-04-01', NULL,          1);


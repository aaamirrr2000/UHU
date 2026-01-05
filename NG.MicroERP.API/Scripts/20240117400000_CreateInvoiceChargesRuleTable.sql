CREATE TABLE InvoiceChargesRules
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

INSERT INTO InvoiceChargesRules
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
(20, 'FLAT',        250.0000, 'SERVICE',  '2024-01-01', NULL,          1),  -- Delivery Fee
(21, 'PERCENTAGE',   5.0000, 'DISCOUNT', '2024-06-01', '2024-06-30',  1),  -- Seasonal Discount
(21, 'FLAT',        500.0000, 'DISCOUNT', '2024-01-01', NULL,          1),  -- Special Discount
(20, 'PERCENTAGE',   3.5000, 'SERVICE',  '2024-04-01', NULL,          1),  -- Handling %
(21, 'PERCENTAGE',  10.0000, 'DISCOUNT', '2023-01-01', '2023-12-31',  0);  -- Old Promotion


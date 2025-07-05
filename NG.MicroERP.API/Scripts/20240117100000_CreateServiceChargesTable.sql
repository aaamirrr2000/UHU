CREATE TABLE ServiceCharges
(
    Id              INT             IDENTITY(1,1) PRIMARY KEY,
    GUID             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ChargeName      NVARCHAR(100)   NOT NULL,                                               -- e.g. “Delivery”
    ChargeType      VARCHAR(50)         NOT NULL CHECK (ChargeType IN ('FLAT','PERCENTAGE')),   -- F = Flat  |  P = Percent
    Amount          DECIMAL(18,4)   NOT NULL,                                               -- 25.0000  |  7.5000 (=7.5 %)
    AppliesTo       INT             NULL DEFAULT 0,                                         -- Categories
    EffectiveFrom   DATE            NOT NULL,
    EffectiveTo     DATE            NULL,
    OrganizationId  INT             DEFAULT 1,
    IsActive        BIT             NOT NULL DEFAULT 1,

    CreatedBy       INT             NULL,
    CreatedOn       DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(255)    NULL,

    UpdatedBy       INT             NULL,
    UpdatedOn       DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom     VARCHAR(255)    NULL,

    IsSoftDeleted   BIT             NOT NULL DEFAULT 0,
    RowVersion      ROWVERSION
);
GO

INSERT INTO ServiceCharges (ChargeName, ChargeType, Amount, AppliesTo, EffectiveFrom, EffectiveTo, CreatedBy, CreatedFrom)
VALUES  ('Service Charges', 'PERCENTAGE', 10.0000, 0, '2025-01-01', '2025-12-31', 1, '10.0.0.15');
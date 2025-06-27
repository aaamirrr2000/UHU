CREATE TABLE ServiceCharges
(
    Id              INT             IDENTITY(1,1) PRIMARY KEY,
    GUID             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ChargeName      NVARCHAR(100)   NOT NULL,                                               -- e.g. “Delivery”
    ChargeType      CHAR(1)         NOT NULL CHECK (ChargeType IN ('F','P')),               -- F = Flat  |  P = Percent
    Amount          DECIMAL(18,4)   NOT NULL,                                               -- 25.0000  |  7.5000 (=7.5 %)
    AppliesTo       VARCHAR(20)     NOT NULL,                                               -- “FOOD”, “BEVERAGE”, “ALL”, etc.
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

INSERT INTO ServiceCharges (ChargeName, ChargeType, Amount, AppliesTo, EffectiveFrom, CreatedBy, CreatedFrom)
VALUES  ('Service Charges', 'P', 10.0000, 'ALL', '2025-01-01', 1, '10.0.0.15');
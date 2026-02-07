CREATE TABLE CustomerItemDiscountRules
(
    Id              INT                 IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT                 DEFAULT 1,
    PartyId         INT                 NULL,                        -- Customer/Party (NULL = applies to all customers)
    ItemId          INT                 NULL,                        -- Item (NULL = applies to all items)
    AmountType      VARCHAR(50)         NOT NULL CHECK (AmountType IN ('FLAT', 'PERCENTAGE')),
    Amount          DECIMAL(18,4)       NOT NULL,                  -- 100.0000 (flat) | 5.0000 (%)
    EffectiveFrom   DATE                NOT NULL,
    EffectiveTo     DATE                NULL,
    IsActive        INT                 NOT NULL DEFAULT 1,
    CreatedBy       INT                 NULL,
    CreatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(255)        NULL,
    UpdatedBy       INT                 NULL,
    UpdatedOn       DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom     VARCHAR(255)        NULL,
    IsSoftDeleted   INT                 NOT NULL DEFAULT 0,
    CONSTRAINT FK_CustomerItemDiscountRules_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_CustomerItemDiscountRules_Party FOREIGN KEY (PartyId) REFERENCES Parties(Id),
    CONSTRAINT FK_CustomerItemDiscountRules_Item FOREIGN KEY (ItemId) REFERENCES Items(Id),
    CONSTRAINT FK_CustomerItemDiscountRules_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_CustomerItemDiscountRules_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- Sample data (only insert if referenced records exist)
-- 5% discount for all customers on all items
IF EXISTS (SELECT 1 FROM Organizations WHERE Id = 1)
BEGIN
    INSERT INTO CustomerItemDiscountRules
    (
        OrganizationId,
        PartyId,
        ItemId,
        AmountType,
        Amount,
        EffectiveFrom,
        EffectiveTo,
        IsActive
    )
    VALUES
    (1, NULL, NULL, 'PERCENTAGE', 5.0000, '2024-01-01', NULL, 1);
END

-- 10% discount for customer 1 on all items (only if customer exists)
IF EXISTS (SELECT 1 FROM Organizations WHERE Id = 1) AND EXISTS (SELECT 1 FROM Parties WHERE Id = 1)
BEGIN
    INSERT INTO CustomerItemDiscountRules
    (
        OrganizationId,
        PartyId,
        ItemId,
        AmountType,
        Amount,
        EffectiveFrom,
        EffectiveTo,
        IsActive
    )
    VALUES
    (1, 1, NULL, 'PERCENTAGE', 10.0000, '2024-01-01', NULL, 1);
END

-- Flat 50 discount on item 1 for all customers (only if item exists)
IF EXISTS (SELECT 1 FROM Organizations WHERE Id = 1) AND EXISTS (SELECT 1 FROM Items WHERE Id = 1)
BEGIN
    INSERT INTO CustomerItemDiscountRules
    (
        OrganizationId,
        PartyId,
        ItemId,
        AmountType,
        Amount,
        EffectiveFrom,
        EffectiveTo,
        IsActive
    )
    VALUES
    (1, NULL, 1, 'FLAT', 50.0000, '2024-01-01', NULL, 1);
END

-- 15% discount for customer 1 on item 1 (limited time) (only if both exist)
IF EXISTS (SELECT 1 FROM Organizations WHERE Id = 1) 
   AND EXISTS (SELECT 1 FROM Parties WHERE Id = 1) 
   AND EXISTS (SELECT 1 FROM Items WHERE Id = 1)
BEGIN
    INSERT INTO CustomerItemDiscountRules
    (
        OrganizationId,
        PartyId,
        ItemId,
        AmountType,
        Amount,
        EffectiveFrom,
        EffectiveTo,
        IsActive
    )
    VALUES
    (1, 1, 1, 'PERCENTAGE', 15.0000, '2024-01-01', '2024-12-31', 1);
END

-- View: vw_ItemPartyTaxCalculation
-- This view calculates taxes for items based on TaxRule, TaxRuleDetail, and party conditions
-- Priority: Item-level TaxRuleId > Category-level TaxRuleId
CREATE OR ALTER VIEW vw_ItemPartyTaxCalculation AS
WITH ItemTaxes AS
(
    -- Item-level tax rules (priority 1)
    SELECT
        tm.OrganizationId,
        tm.AccountId,
        i.Id AS ItemId,
        i.Name AS ItemName,
        i.BasePrice,
        p.Id AS PartyId,
        p.Name AS PartyName,
        trd.Id AS TaxRuleDetailId,
        trd.TaxRuleId,
        trd.TaxId,
        trd.SequenceNo,
        trd.IsRegistered AS EffectiveIsRegistered,
        trd.IsFiler AS EffectiveIsFiler,
        trd.TaxType AS EffectiveTaxType, -- TaxType is now required in TaxRuleDetail
        trd.TaxAmount AS EffectiveRate, -- TaxAmount is now required in TaxRuleDetail
        trd.TaxBaseType, -- TaxBaseType is now required in TaxRuleDetail
        tm.TaxName,
        tm.ConditionType,
        tr.AppliesTo
    FROM Items i
    INNER JOIN TaxRule tr 
        ON i.TaxRuleId = tr.Id 
        AND tr.IsActive = 1
        AND tr.IsSoftDeleted = 0
        AND tr.OrganizationId = i.OrganizationId
    INNER JOIN TaxRuleDetail trd 
        ON tr.Id = trd.TaxRuleId
    INNER JOIN TaxMaster tm 
        ON trd.TaxId = tm.Id
        AND tm.IsActive = 1
        AND tm.IsSoftDeleted = 0
        AND tm.OrganizationId = i.OrganizationId
    INNER JOIN Parties p 
        ON p.OrganizationId = i.OrganizationId
    WHERE 
        -- Check tax rule effective dates
        (tr.EffectiveTo IS NULL OR tr.EffectiveTo >= CAST(GETDATE() AS DATE))
        -- Apply conditions: if tax has ConditionType text, check party conditions
        AND (
            tm.ConditionType IS NULL 
            OR (
                tm.ConditionType IS NOT NULL
                AND (trd.IsRegistered IS NULL OR trd.IsRegistered = p.IsRegistered)
                AND (trd.IsFiler IS NULL OR trd.IsFiler = p.IsFiler)
            )
        )
    
    UNION ALL
    
    -- Category-level tax rules (priority 2 - only when item-level is not set)
    SELECT
        tm.OrganizationId,
        tm.AccountId,
        i.Id AS ItemId,
        i.Name AS ItemName,
        i.BasePrice,
        p.Id AS PartyId,
        p.Name AS PartyName,
        trd.Id AS TaxRuleDetailId,
        trd.TaxRuleId,
        trd.TaxId,
        trd.SequenceNo,
        trd.IsRegistered AS EffectiveIsRegistered,
        trd.IsFiler AS EffectiveIsFiler,
        trd.TaxType AS EffectiveTaxType,
        trd.TaxAmount AS EffectiveRate,
        trd.TaxBaseType,
        tm.TaxName,
        tm.ConditionType,
        tr.AppliesTo
    FROM Items i
    INNER JOIN Categories c
        ON i.CategoryId = c.Id
        AND c.TaxRuleId IS NOT NULL
    INNER JOIN TaxRule tr 
        ON c.TaxRuleId = tr.Id 
        AND tr.IsActive = 1
        AND tr.IsSoftDeleted = 0
        AND tr.OrganizationId = i.OrganizationId
    INNER JOIN TaxRuleDetail trd 
        ON tr.Id = trd.TaxRuleId
    INNER JOIN TaxMaster tm 
        ON trd.TaxId = tm.Id
        AND tm.IsActive = 1
        AND tm.IsSoftDeleted = 0
        AND tm.OrganizationId = i.OrganizationId
    INNER JOIN Parties p 
        ON p.OrganizationId = i.OrganizationId
    WHERE 
        -- Only use category-level if item-level is not set
        i.TaxRuleId IS NULL
        -- Check tax rule effective dates
        AND (tr.EffectiveTo IS NULL OR tr.EffectiveTo >= CAST(GETDATE() AS DATE))
        -- Apply conditions: if tax has ConditionType text, check party conditions
        AND (
            tm.ConditionType IS NULL 
            OR (
                tm.ConditionType IS NOT NULL
                AND (trd.IsRegistered IS NULL OR trd.IsRegistered = p.IsRegistered)
                AND (trd.IsFiler IS NULL OR trd.IsFiler = p.IsFiler)
            )
        )
)
, RunningTotals AS
(
    SELECT
        it.OrganizationId,
        it.AccountId,
        it.ItemId,
        it.ItemName,
        it.BasePrice,
        it.PartyId,
        it.PartyName,
        it.TaxName,
        it.EffectiveTaxType AS TaxType,
        it.TaxBaseType,
        it.EffectiveRate AS Rate,
        it.SequenceNo,
        it.AppliesTo,
        -- Initial TaxableAmount is BasePrice (for BASE_ONLY and BASE_PLUS_SELECTED)
        -- For RUNNING_TOTAL, we'll calculate in the final SELECT
        it.BasePrice AS TaxableAmount,
        0.0 AS TaxAmount
    FROM ItemTaxes it
)
SELECT
    rt.OrganizationId,
    rt.AccountId,
    rt.ItemId,
    rt.ItemName,
    rt.PartyId,
    rt.PartyName,
    rt.BasePrice,
    rt.TaxName,
    rt.Rate AS Percentage,
    rt.TaxableAmount,
    CASE 
        WHEN rt.TaxType = 'PERCENTAGE' THEN rt.TaxableAmount * rt.Rate / 100
        WHEN rt.TaxType = 'FLAT' THEN rt.Rate
        ELSE 0
    END AS TaxAmount,
    rt.BasePrice + CASE 
                      WHEN rt.TaxType = 'PERCENTAGE' THEN rt.TaxableAmount * rt.Rate / 100
                      WHEN rt.TaxType = 'FLAT' THEN rt.Rate
                      ELSE 0
                   END AS FinalPrice,
    rt.AppliesTo,
    rt.SequenceNo
FROM RunningTotals rt;
GO

-- View: vw_TaxesOnPartyAndItem
CREATE VIEW vw_TaxesOnPartyAndItem AS
SELECT 
    i.Id AS ItemId,
    i.Name AS ItemName,
    p.Id AS PartyId,
    p.Name AS PartyName,
    p.IsRegistered,
    p.IsFiler,
    trd.Id AS TaxRuleDetailId,
    trd.TaxRuleId,
    trd.TaxId,
    trd.SequenceNo,
    trd.IsRegistered AS DetailIsRegistered,
    trd.IsFiler AS DetailIsFiler,
    trd.TaxAmount AS TaxRate,
    trd.TaxType,
    trd.TaxBaseType,
    tm.TaxName,
    tm.ConditionType,
    tr.RuleName,
    tr.AppliesTo,
    tr.EffectiveFrom,
    tr.EffectiveTo
FROM Items i
CROSS JOIN Parties p
INNER JOIN TaxRuleDetail trd ON 1=1
INNER JOIN TaxRule tr ON trd.TaxRuleId = tr.Id
INNER JOIN TaxMaster tm ON trd.TaxId = tm.Id
WHERE tr.IsSoftDeleted = 0 
  AND tr.IsActive = 1
  AND tm.IsSoftDeleted = 0
  AND tm.IsActive = 1
  AND (tr.EffectiveTo IS NULL OR tr.EffectiveTo >= CAST(GETDATE() AS DATE))
  -- Apply conditions: if tax has ConditionType text, check party conditions
  AND (
      tm.ConditionType IS NULL 
      OR (
          tm.ConditionType IS NOT NULL
          AND (trd.IsRegistered IS NULL OR trd.IsRegistered = p.IsRegistered)
          AND (trd.IsFiler IS NULL OR trd.IsFiler = p.IsFiler)
      )
  );
GO

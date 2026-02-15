-- View: vw_ItemTaxCalculation
-- Priority: Item-level TaxRuleId > Category-level TaxRuleId
CREATE OR ALTER VIEW vw_ItemTaxCalculation AS
WITH ItemTaxes AS
(
    -- Item-level tax rules (priority 1)
    SELECT
        t.OrganizationId,
        t.AccountId,
        i.Id AS ItemId,
        i.Name AS ItemName,
        i.BasePrice,
        t.Id AS TaxId,
        t.TaxName,
        trd.TaxType,
        trd.TaxBaseType,
        trd.TaxAmount AS Rate,
        trd.SequenceNo,
        tr.Id AS TaxRuleId
    FROM Items i
    INNER JOIN TaxRule tr 
        ON i.TaxRuleId = tr.Id 
        AND tr.IsActive = 1
        AND tr.OrganizationId = i.OrganizationId   -- same org
    INNER JOIN TaxRuleDetail trd 
        ON tr.Id = trd.TaxRuleId
    INNER JOIN TaxMaster t 
        ON trd.TaxId = t.Id
        AND t.OrganizationId = i.OrganizationId   -- same org
    WHERE i.OrganizationId = t.OrganizationId      -- filter by org
    
    UNION ALL
    
    -- Category-level tax rules (priority 2 - only when item-level is not set)
    SELECT
        t.OrganizationId,
        t.AccountId,
        i.Id AS ItemId,
        i.Name AS ItemName,
        i.BasePrice,
        t.Id AS TaxId,
        t.TaxName,
        trd.TaxType,
        trd.TaxBaseType,
        trd.TaxAmount AS Rate,
        trd.SequenceNo,
        tr.Id AS TaxRuleId
    FROM Items i
    INNER JOIN Categories c
        ON i.CategoryId = c.Id
        AND c.TaxRuleId IS NOT NULL
    INNER JOIN TaxRule tr 
        ON c.TaxRuleId = tr.Id 
        AND tr.IsActive = 1
        AND tr.OrganizationId = i.OrganizationId   -- same org
    INNER JOIN TaxRuleDetail trd 
        ON tr.Id = trd.TaxRuleId
    INNER JOIN TaxMaster t 
        ON trd.TaxId = t.Id
        AND t.OrganizationId = i.OrganizationId   -- same org
    WHERE 
        -- Only use category-level if item-level is not set
        i.TaxRuleId IS NULL
        AND i.OrganizationId = t.OrganizationId      -- filter by org
)
, RunningTotals AS
(
    SELECT
        it.OrganizationId,
        it.AccountId,
        it.ItemId,
        it.ItemName,
        it.BasePrice,
        it.TaxId,
        it.TaxName,
        it.TaxType,
        it.TaxBaseType,
        it.Rate,
        it.SequenceNo,
        -- Initial TaxableAmount is BasePrice
        it.BasePrice AS TaxableAmount,
        0.0 AS TaxAmount
    FROM ItemTaxes it
)
SELECT
    rt.OrganizationId,
    rt.AccountId,
    rt.ItemId,
    rt.ItemName,
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
                   END AS FinalPrice
FROM RunningTotals rt;

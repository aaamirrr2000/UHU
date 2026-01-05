CREATE VIEW vw_ItemPartyTaxCalculation AS

WITH ItemTaxes AS
(
    SELECT
        t.OrganizationId,
        t.AccountId,
        i.Id AS ItemId,
        i.Name AS ItemName,
        i.BasePrice,
        p.Id AS PartyId,
        p.Name AS PartyName,
        t.Id AS TaxId,
        t.TaxName,
        t.TaxType,
        t.TaxBaseType,
        t.Rate,
        trd.SequenceNo,
        tr.Id AS TaxRuleId,
        tr.AppliesTo
    FROM Items i
    INNER JOIN TaxRule tr 
        ON i.TaxRuleId = tr.Id 
        AND tr.IsActive = 1
        AND tr.OrganizationId = i.OrganizationId
    INNER JOIN TaxRuleDetail trd 
        ON tr.Id = trd.TaxRuleId
    INNER JOIN TaxMaster t 
        ON trd.TaxId = t.Id
        AND t.OrganizationId = i.OrganizationId
    INNER JOIN Parties p 
        ON p.OrganizationId = i.OrganizationId
    WHERE 
        (tr.IsRegistered IS NULL OR tr.IsRegistered = p.IsRegistered)
        AND (tr.IsFiler IS NULL OR tr.IsFiler = p.IsFiler)
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
        it.TaxId,
        it.TaxName,
        it.TaxType,
        it.TaxBaseType,
        it.Rate,
        it.SequenceNo,
        -- Initial TaxableAmount is BasePrice
        it.BasePrice AS TaxableAmount,
        0.0 AS TaxAmount,
        it.AppliesTo
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
    rt.AppliesTo,
    rt.TaxType,
    rt.TaxName,
    rt.Rate,
    rt.TaxableAmount,
    CASE 
        WHEN rt.TaxType = 'PERCENTAGE' THEN rt.TaxableAmount * rt.Rate / 100
        WHEN rt.TaxType = 'FIXED' THEN rt.Rate
        ELSE 0
    END AS TaxAmount,
    rt.BasePrice + CASE 
                      WHEN rt.TaxType = 'PERCENTAGE' THEN rt.TaxableAmount * rt.Rate / 100
                      WHEN rt.TaxType = 'FIXED' THEN rt.Rate
                      ELSE 0
                   END AS FinalPrice
FROM RunningTotals rt;

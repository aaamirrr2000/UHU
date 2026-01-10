-- Create ItemTaxes View
CREATE OR ALTER VIEW vw_ItemTaxes AS
SELECT
    ti.Id,
    ti.OrganizationId,
    ti.TaxId,
    tm.TaxName,
    tm.Rate AS TaxRate,          -- TaxValue can be percentage or fixed
    tm.TaxType,
    ti.ItemId,
    i.Name AS ItemName,
    i.Description,
    i.Code AS ItemCode,
    i.CategoryId,
    c.Name AS Category,
    i.CostPrice,
    i.BasePrice,
    i.DefaultDiscount,
    i.HSCode,
    i.IsSoftDeleted AS ItemIsSoftDeleted,
    i.IsActive AS ItemIsActive,
    ti.ApplicableFrom AS EffectiveFrom,
    NULL AS EffectiveTo,         -- TaxItems table doesn't have EffectiveTo
    ti.IsActive AS TaxItemIsActive,
    ti.InvoiceType
FROM TaxItems AS ti
LEFT JOIN Items AS i 
    ON i.Id = ti.ItemId 
    AND i.OrganizationId = ti.OrganizationId
LEFT JOIN Categories AS c 
    ON c.Id = i.CategoryId 
    AND c.OrganizationId = ti.OrganizationId
LEFT JOIN TaxMaster AS tm 
    ON tm.Id = ti.TaxId;
GO


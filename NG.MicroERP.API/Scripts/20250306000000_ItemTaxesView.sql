CREATE OR ALTER VIEW ItemTaxes AS
SELECT
    ti.Id,
    ti.OrganizationId,
    ti.TaxId,
    tm.TaxName,
    tm.TaxRate,          -- TaxValue can be percentage or fixed
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
    ti.EffectiveFrom,
    ti.EffectiveTo,
    ti.IsActive AS TaxItemIsActive
FROM TaxRule AS ti
LEFT JOIN Items AS i 
    ON i.Id = ti.ItemId 
    AND i.OrganizationId = ti.OrganizationId
LEFT JOIN Categories AS c 
    ON c.Id = ti.ItemCategoryId 
    AND c.OrganizationId = ti.OrganizationId
LEFT JOIN TaxMaster AS tm 
    ON tm.Id = ti.TaxId;

GO

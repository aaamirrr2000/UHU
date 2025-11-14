CREATE OR ALTER VIEW TaxItemConfig AS
SELECT
  a.Id,
  a.GUID,
  a.OrganizationId,
  a.TaxId,
  c.TaxName,
  C.TaxRate,
  C.TaxType,
  a.ItemId,
  b.Name,
  b.Unit,
  b.CostPrice,
  b.RetailPrice,
  b.Code,
  b.CategoriesId,
  b.Description,
  b.IsInventoryItem,
  b.HSCode,
  b.ServingSize,
  a.IsSoftDeleted,
  a.IsActive
FROM TaxItems as a
LEFT JOIN Items as b on b.Id = a.ItemId
LEFT JOIN TaxMaster as c on c.Id = a.TaxId;
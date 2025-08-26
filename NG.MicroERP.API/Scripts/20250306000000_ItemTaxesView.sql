CREATE OR ALTER VIEW ItemTaxes AS
SELECT
  a.Id,
  a.OrganizationId,
  a.TaxId,
  c.TaxName,
  c.TaxRate,
  c.TaxType,
  a.ItemId,
  b.Name as ItemName,
  b.Description,
  b.Code as ItemCode,
  b.CategoriesId,
  d.Name as Category,
  b.CostPrice,
  b.RetailPrice,
  b.Discount,
  b.HSCode,
  b.IsInventoryItem,
  b.IsSoftDeleted,
  b.IsActive,
  b.SaleTypeId,
  e.Code as SaleTypeCode,
  e.Description as SaleTypeDescription
FROM TaxItems as a
LEFT JOIN Items as b on b.id=a.ItemId and b.OrganizationId=a.OrganizationId
LEFT JOIN TaxMaster as c on c.Id=a.TaxId and c.OrganizationId=a.OrganizationId
LEFT JOIN Categories as d on d.Id=b.CategoriesId and d.OrganizationId=a.OrganizationId
LEFT JOIN DigitalInvoiceSaleType as e on e.Id=b.SaleTypeId and e.OrganizationId=a.OrganizationId;
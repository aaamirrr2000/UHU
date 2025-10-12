CREATE OR ALTER VIEW ItemsForSale AS
SELECT 
    I.Id AS RecordId,
    I.Guid,
    I.OrganizationId,
    I.Code,
    I.Name,
    I.Description,
    'ITEM' AS RecordType,
    I.RetailPrice,
    I.Discount AS DiscountPercent,
    I.Tax AS TaxPercent,
    I.Unit,
    NULL AS BundleType,
    NULL AS BundleId,
    I.IsActive,
    I.CreatedOn,
    I.UpdatedOn
FROM Items I
WHERE I.IsActive = 1 AND I.IsSoftDeleted = 0

UNION ALL

SELECT 
    B.Id AS RecordId,
    B.Guid,
    B.OrganizationId,
    B.Code,
    B.Name,
    B.Description,
    'BUNDLE' AS RecordType,
    B.RetailPrice,
    0 AS DiscountPercent,
    0 AS TaxPercent,
    NULL AS Unit,
    B.BundleType,
    B.Id AS BundleId,
    B.IsActive,
    B.CreatedOn,
    B.UpdatedOn
FROM Bundles B
WHERE B.IsActive = 1 AND B.IsSoftDeleted = 0
      AND (B.BundleType = 'SALES' OR B.BundleType = 'BOTH');
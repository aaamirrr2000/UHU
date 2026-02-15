CREATE OR ALTER VIEW vw_CategoriesHavingSomeItems AS
SELECT
  a.Id,
  a.OrganizationId,
  a.Code,
  a.Attribute,
  a.Name,
  a.ParentId,
  a.IsActive,
  a.CreatedBy,
  a.CreatedOn,
  a.CreatedFrom,
  a.UpdatedBy,
  a.UpdatedOn,
  a.UpdatedFrom,
  a.IsSoftDeleted,
  COUNT(b.Name) AS ItemCount
FROM Categories AS a
LEFT JOIN Items AS b ON b.CategoryId = a.Id
WHERE a.IsSoftDeleted = 0
GROUP BY
  a.Id,
  a.OrganizationId,
  a.Code,
  a.Attribute,
  a.Name,
  a.ParentId,
  a.IsActive,
  a.CreatedBy,
  a.CreatedOn,
  a.CreatedFrom,
  a.UpdatedBy,
  a.UpdatedOn,
  a.UpdatedFrom,
  a.IsSoftDeleted
HAVING COUNT(b.Name) != 0;
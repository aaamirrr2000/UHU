-- Create PettyCashReport View
-- Drop view if exists to recreate with EmployeeId
IF OBJECT_ID('dbo.vw_PettyCashReport', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.vw_PettyCashReport;
END
GO

CREATE VIEW dbo.vw_PettyCashReport AS
SELECT
  a.Id,
  a.SeqNo,
  a.LocationId,
  b.Name as LocationName,
  a.EmployeeId,
  c.Fullname as EmployeeName,
  a.TranDate,
  a.Description,
  a.Amount,
  a.AccountId,
  h.Name as AccountName,
  a.TranType,
  a.PaymentMethod,
  a.RefNo,
  a.TranRef,
  a.CreatedOn,
  a.CreatedBy,
  a.CreatedFrom,
  d.Username as CreatedByUser,
  e.Fullname as CreatedByName,
  a.UpdatedOn,
  a.UpdatedBy,
  a.UpdatedFrom,
  f.Username as UpdatedByUser,
  g.Fullname as UpdatedByName
FROM PettyCash as a
LEFT JOIN Locations as b on b.Id=a.LocationId
LEFT JOIN Employees as c on c.Id=a.EmployeeId
LEFT JOIN Users as d on d.Id=a.CreatedBy
LEFT JOIN Employees as e on e.Id=d.EmpId
LEFT JOIN Users as f on f.Id=a.UpdatedBy
LEFT JOIN Employees as g on g.Id=f.EmpId
LEFT JOIN ChartOfAccounts as h on h.Id=a.AccountId;


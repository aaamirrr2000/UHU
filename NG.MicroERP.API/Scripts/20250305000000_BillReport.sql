CREATE VIEW BillReport AS
SELECT
	a.Id,
	a.SeqNo,
	a.BillType,
	a.LocationId,
	b.Name as LocationName,
	a.PartyId,
	c.Name as Party,
	a.PartyName,
	a.PartyPhone,
	a.PartyEmail,
	a.PartyAddress,
	a.TranDate,
	a.DiscountAmount,
	a.TaxAmount,
	a.BillAmount,
	a.PaymentMethod,
	a.PaymentRef,
	a.PaymentAmount,
	a.Description,
	a.CreatedOn,
	d.Username,
	e.fullname,
	f.ItemId,
	g.Name as ItemName,
	g.IsInventoryItem,
	f.ServingSize,
	f.StockCondition,
	f.Description as Instructions,
	a.ServiceType,
	f.Qty,
	f.UnitPrice,
	f.DiscountAmount as ItemDiscount,
	f.TaxAmount as ItemTax,
	(f.Qty*f.UnitPrice)+f.TaxAmount-f.DiscountAmount as Amount,
	f.Status
FROM Bill as a
LEFT JOIN Locations as b on b.Id=a.LocationId
LEFT JOIN Parties as c on c.Id=a.PartyId
LEFT JOIN Users as d on d.Id=a.CreatedBy
LEFT JOIN Employees as e on e.Id=d.EmpId
LEFT JOIN BillDetail as f on f.BillId=a.Id
LEFT JOIN Items as g on g.Id=f.ItemId;
Create View DineInOrderStatus as
select
	a.id,
	c.SeqNo as Invoice,
	c.TableId,
	d.TableNumber,
	d.TableLocation,
	a.ItemId,
	b.Name as ItemName,
	a.StockCondition,
	a.ServingSize,
	a.Qty,
	a.UnitPrice,
	a.DiscountAmount,
	a.TaxAmount,
	a.BillId,
  c.LocationId,
  i.Name as LocationName,
	a.Description as ItemsInstruction,
	a.Status,
  a.IsTakeAway,
  IIF(a.IsTakeAway = 1, 'TAKEAWAY', '') AS TakeAway,
	a.TranDate,
	c.CreatedBy,
	e.Username,
	f.Fullname,
	c.Description as BillInstruction,
	c.CreatedFrom,
	c.UpdatedBy,
	g.Username as LastUpdateBy,
	h.Fullname as LastUpdatedName,
	c.UpdatedFrom,
c.OrganizationId,
j.Name as OrganizationName
from BillDetail as a 
left join items as b on b.id=a.ItemId
left join Bill as c on c.id=a.BillId and c.IsSoftDeleted=0
left join RestaurantTables as d on d.id=c.TableId
left join users as e on e.id=c.CreatedBy
left join Employees as f on f.id=e.empid
left join users as g on g.id=c.UpdatedBy
left join Employees as h on h.id=g.empid
left join Locations as i on i.id=c.LocationId
left join Organizations as j on j.id=c.OrganizationId
where a.IsSoftDeleted=0 and c.Id is not null;
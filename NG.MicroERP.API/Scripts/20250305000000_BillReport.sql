-- 1. Master-level Bill Report
CREATE OR ALTER VIEW BillMasterReport AS
SELECT
    a.Id,
    a.Guid,
    a.SeqNo,
    a.InvoiceType,
    a.Source,
    a.OrganizationId,
    a.LocationId,
    b.Name AS LocationName,
    a.PartyId,
    c.Name AS PartyNameDb,
    a.PartyName,
    a.PartyPhone,
    a.PartyEmail,
    a.PartyAddress,
    a.TableId,
    a.TranDate,
    a.DiscountAmount,

    -- Subtotal
    (SELECT SUM(qty * unitprice - discountamount) 
     FROM BillDetail 
     WHERE billid = a.Id) AS SubTotalAmount,

    -- Extra charges
    (SELECT SUM(CalculatedAmount) 
     FROM BillCharges 
     WHERE billid = a.Id) AS TotalChargeAmount,

    -- Paid amount
    (SELECT SUM(AmountPaid) 
     FROM BillPayments 
     WHERE billid = a.Id) AS TotalPaidAmount,

    -- Billed Amount = SubTotal + Charges - Discount
    (
        ISNULL((SELECT SUM(qty * unitprice - discountamount) 
                FROM BillDetail 
                WHERE billid = a.Id), 0) +
        ISNULL((SELECT SUM(CalculatedAmount) 
                FROM BillCharges 
                WHERE billid = a.Id), 0) -
        ISNULL(a.DiscountAmount, 0)
    ) AS BilledAmount,

    -- Balance Amount = Billed - Paid
    (
        ISNULL((
            ISNULL((SELECT SUM(qty * unitprice - discountamount) 
                    FROM BillDetail 
                    WHERE billid = a.Id), 0) +
            ISNULL((SELECT SUM(CalculatedAmount) 
                    FROM BillCharges 
                    WHERE billid = a.Id), 0) -
            ISNULL(a.DiscountAmount, 0)
        ), 0) -
        ISNULL((SELECT SUM(AmountPaid) 
                FROM BillPayments 
                WHERE billid = a.Id), 0)
    ) AS BalanceAmount,

    a.Description,
    a.Status,
    a.ServiceType,
    a.PreprationTime,
    a.ClientComments,
    a.Rating,
    a.CreatedOn,
    a.CreatedBy,
    d.Username,
    e.Fullname AS EmployeeFullName

FROM Bill AS a
LEFT JOIN Locations AS b ON b.Id = a.LocationId
LEFT JOIN Parties AS c ON c.Id = a.PartyId
LEFT JOIN Users AS d ON d.Id = a.CreatedBy
LEFT JOIN Employees AS e ON e.Id = d.EmpId
WHERE a.IsSoftDeleted = 0;


GO

-- 2. Item-level Bill Detail Report
CREATE OR ALTER VIEW BillItemReport AS
SELECT
    f.Id AS BillDetailId,
    f.BillId,
    a.SeqNo,
    f.ItemId,
    g.HSCode,
    g.Name AS ItemName,
    g.IsInventoryItem,
    f.ServingSize,
    f.StockCondition,
    f.Qty,
    f.UnitPrice,
    f.DiscountAmount,
    ((f.Qty * f.UnitPrice) - f.DiscountAmount) AS ItemTotalAmount,
    f.Description AS Instructions,
    f.Status,
    f.Person,
    f.TranDate,
    f.Rating,
    f.IsSoftDeleted
FROM BillDetail AS f
INNER JOIN Bill AS a ON a.Id = f.BillId
LEFT JOIN Items AS g ON g.Id = f.ItemId
WHERE f.IsSoftDeleted = 0;

GO

-- 3. Payment-level Bill Payments Report
CREATE OR ALTER VIEW BillPaymentsReport AS
SELECT
    p.Id AS PaymentId,
    p.BillId,
    a.SeqNo,
    a.PartyId,
    c.Name AS PartyName,
    p.PaymentMethod,
    p.PaymentRef,
    p.AmountPaid,
    p.PaidOn,
    p.Notes
FROM BillPayments AS p
INNER JOIN Bill AS a ON a.Id = p.BillId
LEFT JOIN Parties AS c ON c.Id = a.PartyId
WHERE p.IsSoftDeleted = 0;

GO

-- 4. Charge/Discount-level Bill Charges Report
CREATE OR ALTER VIEW BillChargesReport AS
SELECT
    ch.Id AS BillChargeId,
    ch.BillId,
    a.SeqNo,
    ch.ChargeRuleId,
    ch.RuleName,
    ch.RuleType,
    ch.AmountType,
    ch.Rate,
    ch.CalculatedAmount,
    ch.SequenceOrder,
    ch.CalculationBase
FROM BillCharges AS ch
INNER JOIN Bill AS a ON a.Id = ch.BillId
WHERE ch.IsSoftDeleted = 0;

GO

-- 5. Bill Dashboard
CREATE OR ALTER VIEW BillSummaryReport AS
SELECT
    b.Id AS BillId,
    b.SeqNo,
    b.InvoiceType,
    b.Source,
    b.SalesId,
    emp.Fullname AS SalesPerson,

    b.PartyId,
    p.Name AS PartyNameDb,
    b.PartyName,
    b.PartyPhone,
    b.PartyEmail,
    b.PartyAddress,

    b.LocationId,
    loc.Name AS LocationName,

    b.TableId,
    rt.TableNumber AS TableName,

    b.TranDate,
    b.PreprationTime,
    b.Description,
    b.ClientComments,

    b.DiscountAmount,
    SUM((bd.Qty * bd.UnitPrice) - bd.DiscountAmount) AS ItemsAmount,
    SUM(bc.CalculatedAmount) AS ChargesAmount,
    SUM(bp.AmountPaid) AS PaidAmount,

    b.Status,
    b.CreatedBy,
    usr.Username,
    b.CreatedOn

FROM Bill AS b
LEFT JOIN BillDetail AS bd ON bd.BillId = b.Id
LEFT JOIN BillPayments AS bp ON bp.BillId = b.Id
LEFT JOIN BillCharges AS bc ON bc.BillId = b.Id
LEFT JOIN Locations AS loc ON loc.Id = b.LocationId
LEFT JOIN Parties AS p ON p.Id = b.PartyId
LEFT JOIN Users AS usr ON usr.Id = b.CreatedBy
LEFT JOIN Employees AS emp ON emp.Id = usr.EmpId
LEFT JOIN RestaurantTables AS rt ON rt.Id = b.TableId

WHERE b.IsSoftDeleted = 0

GROUP BY
    b.Id,
    b.SeqNo,
    b.InvoiceType,
    b.Source,
    b.SalesId,
    emp.Fullname,
    b.PartyId,
    p.Name,
    b.PartyName,
    b.PartyPhone,
    b.PartyEmail,
    b.PartyAddress,
    b.LocationId,
    loc.Name,
    b.TableId,
    rt.TableNumber,
    b.TranDate,
    b.PreprationTime,
    b.Description,
    b.ClientComments,
    b.DiscountAmount,
    b.Status,
    b.CreatedBy,
    usr.Username,
    b.CreatedOn;

Go


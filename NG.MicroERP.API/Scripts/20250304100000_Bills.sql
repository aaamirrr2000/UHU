CREATE OR ALTER VIEW Bills AS
SELECT
    Bill.ID,
    Bill.SeqNo,
    Bill.BillType,
    Bill.Source,
    Bill.SalesId,
    Employees.Fullname,
    Bill.TableId,
    RestaurantTables.TableNumber AS TableName,
    Bill.LocationId,
    Locations.Name AS Location,
    Bill.PartyId,
    Parties.Name AS Party,
    Bill.PartyName,
    Bill.PartyPhone,
    Bill.PartyEmail,
    Bill.PartyAddress,
    Bill.TranDate,
    Bill.PreprationTime,

    -- SubTotalAmount: sum of qty * unit price from BillDetail
    ISNULL((
        SELECT SUM(Qty * UnitPrice)
        FROM BillDetail
        WHERE BillDetail.BillId = Bill.Id
    ), 0) AS SubTotalAmount,

    -- TotalChargeAmount: sum of charges from BillCharges
    ISNULL((
        SELECT SUM(CalculatedAmount)
        FROM BillCharges
        WHERE BillCharges.BillId = Bill.Id
    ), 0) AS TotalChargeAmount,

    Bill.DiscountAmount,

    -- BillAmount = SubTotal + Charges - Discount
    ISNULL((
        SELECT SUM(Qty * UnitPrice)
        FROM BillDetail
        WHERE BillDetail.BillId = Bill.Id
    ), 0)
    +
    ISNULL((
        SELECT SUM(CalculatedAmount)
        FROM BillCharges
        WHERE BillCharges.BillId = Bill.Id
    ), 0)
    -
    ISNULL(Bill.DiscountAmount, 0) AS BillAmount,

    -- TotalPaidAmount: sum from BillPayments
    ISNULL((
        SELECT SUM(AmountPaid)
        FROM BillPayments
        WHERE BillPayments.BillId = Bill.Id
    ), 0) AS TotalPaidAmount,

    -- Balance = BillAmount - PaidAmount
    ISNULL((
        ISNULL((
            SELECT SUM(Qty * UnitPrice)
            FROM BillDetail
            WHERE BillDetail.BillId = Bill.Id
        ), 0)
        +
        ISNULL((
            SELECT SUM(CalculatedAmount)
            FROM BillCharges
            WHERE BillCharges.BillId = Bill.Id
        ), 0)
        -
        ISNULL(Bill.DiscountAmount, 0)
    ), 0)
    -
    ISNULL((
        SELECT SUM(AmountPaid)
        FROM BillPayments
        WHERE BillPayments.BillId = Bill.Id
    ), 0) AS BalanceAmount,

    Bill.Description,
    Bill.CreatedBy,
    Bill.CreatedOn,
    Bill.Status,
    Users.Username,
    Bill.ClientComments

FROM Bill
LEFT JOIN Locations ON Locations.Id = Bill.LocationId
LEFT JOIN Parties ON Parties.Id = Bill.PartyId
LEFT JOIN Users ON Users.Id = Bill.CreatedBy
LEFT JOIN Employees ON Employees.Id = Users.EmpId
LEFT JOIN RestaurantTables ON RestaurantTables.Id = Bill.TableId

WHERE
    Bill.IsSoftDeleted = 0;


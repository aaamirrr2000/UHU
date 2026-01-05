CREATE OR ALTER VIEW vw_InvoiceMasterReport AS
SELECT 
    i.Id,
    i.Guid,
    i.Code AS SeqNo,
    i.InvoiceType,
    i.Source,
    i.OrganizationId,
    i.LocationId,
    ISNULL(l.Name, '') AS LocationName,
    i.PartyId,
    ISNULL(p.Name, '') AS PartyNameDb,
    ISNULL(i.PartyName, ISNULL(p.Name, '')) AS PartyName,
    ISNULL(i.PartyPhone, ISNULL((
        SELECT TOP 1 pc.ContactValue
        FROM PartyContacts pc
        WHERE pc.PartyId = i.PartyId 
            AND pc.ContactType IN ('PHONE', 'MOBILE')
            AND pc.IsPrimary = 1
            AND pc.IsSoftDeleted = 0
        ORDER BY CASE WHEN pc.ContactType = 'PHONE' THEN 1 ELSE 2 END
    ), '')) AS PartyPhone,
    ISNULL(i.PartyEmail, ISNULL((
        SELECT TOP 1 pc.ContactValue
        FROM PartyContacts pc
        WHERE pc.PartyId = i.PartyId 
            AND pc.ContactType = 'EMAIL'
            AND pc.IsPrimary = 1
            AND pc.IsSoftDeleted = 0
    ), '')) AS PartyEmail,
    ISNULL(i.PartyAddress, ISNULL(p.Address, '')) AS PartyAddress,
    NULL AS ScenarioId,
    NULL AS Description_SaleType,
    NULL AS BuyerType,
    NULL AS Purpose_TaxContext,
    0 AS TableId,
    ISNULL(i.TranDate, i.CreatedOn) AS TranDate,
    ISNULL((
        SELECT SUM(ic.AppliedAmount)
        FROM InvoiceCharges ic
        WHERE ic.InvoiceId = i.Id 
            AND ic.ChargeCategory = 'DISCOUNT'
            AND ic.IsSoftDeleted = 0
    ), 0) AS DiscountAmount,
    ISNULL((
        SELECT SUM((id.UnitPrice * id.Qty) - id.DiscountAmount + ISNULL(tax.TaxAmount, 0))
        FROM InvoiceDetail id
        LEFT JOIN (
            SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount
            FROM InvoiceDetailTax
            GROUP BY InvoiceDetailId
        ) tax ON id.Id = tax.InvoiceDetailId
        WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0
    ), 0) AS SubTotalAmount,
    ISNULL((
        SELECT SUM(ic.AppliedAmount)
        FROM InvoiceCharges ic
        WHERE ic.InvoiceId = i.Id 
            AND ic.ChargeCategory = 'SERVICE'
            AND ic.IsSoftDeleted = 0
    ), 0) AS TotalChargeAmount,
    (ISNULL((
        SELECT SUM((id.UnitPrice * id.Qty) - id.DiscountAmount + ISNULL(tax.TaxAmount, 0))
        FROM InvoiceDetail id
        LEFT JOIN (
            SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount
            FROM InvoiceDetailTax
            GROUP BY InvoiceDetailId
        ) tax ON id.Id = tax.InvoiceDetailId
        WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0
    ), 0) 
    + ISNULL((
        SELECT SUM(ic.AppliedAmount)
        FROM InvoiceCharges ic
        WHERE ic.InvoiceId = i.Id 
            AND ic.ChargeCategory = 'SERVICE'
            AND ic.IsSoftDeleted = 0
    ), 0)
    - ISNULL((
        SELECT SUM(ic.AppliedAmount)
        FROM InvoiceCharges ic
        WHERE ic.InvoiceId = i.Id 
            AND ic.ChargeCategory = 'DISCOUNT'
            AND ic.IsSoftDeleted = 0
    ), 0)) AS InvoiceAmount,
    (ISNULL((
        SELECT SUM((id.UnitPrice * id.Qty) - id.DiscountAmount + ISNULL(tax.TaxAmount, 0))
        FROM InvoiceDetail id
        LEFT JOIN (
            SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount
            FROM InvoiceDetailTax
            GROUP BY InvoiceDetailId
        ) tax ON id.Id = tax.InvoiceDetailId
        WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0
    ), 0) 
    + ISNULL((
        SELECT SUM(ic.AppliedAmount)
        FROM InvoiceCharges ic
        WHERE ic.InvoiceId = i.Id 
            AND ic.ChargeCategory = 'SERVICE'
            AND ic.IsSoftDeleted = 0
    ), 0)
    - ISNULL((
        SELECT SUM(ic.AppliedAmount)
        FROM InvoiceCharges ic
        WHERE ic.InvoiceId = i.Id 
            AND ic.ChargeCategory = 'DISCOUNT'
            AND ic.IsSoftDeleted = 0
    ), 0)) AS BillAmount,
    ISNULL((
        SELECT SUM(ip.Amount)
        FROM InvoicePayments ip
        WHERE ip.InvoiceId = i.Id AND ip.IsSoftDeleted = 0
    ), 0) AS TotalPaidAmount,
    ((ISNULL((
        SELECT SUM((id.UnitPrice * id.Qty) - id.DiscountAmount + ISNULL(tax.TaxAmount, 0))
        FROM InvoiceDetail id
        LEFT JOIN (
            SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount
            FROM InvoiceDetailTax
            GROUP BY InvoiceDetailId
        ) tax ON id.Id = tax.InvoiceDetailId
        WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0
    ), 0) 
    + ISNULL((
        SELECT SUM(ic.AppliedAmount)
        FROM InvoiceCharges ic
        WHERE ic.InvoiceId = i.Id 
            AND ic.ChargeCategory = 'SERVICE'
            AND ic.IsSoftDeleted = 0
    ), 0)
    - ISNULL((
        SELECT SUM(ic.AppliedAmount)
        FROM InvoiceCharges ic
        WHERE ic.InvoiceId = i.Id 
            AND ic.ChargeCategory = 'DISCOUNT'
            AND ic.IsSoftDeleted = 0
    ), 0))
    - ISNULL((
        SELECT SUM(ip.Amount)
        FROM InvoicePayments ip
        WHERE ip.InvoiceId = i.Id AND ip.IsSoftDeleted = 0
    ), 0)) AS BalanceAmount,
    i.Description,
    ISNULL(i.Status, '') AS Status,
    '' AS ServiceType,
    CAST('00:00:00' AS TIME) AS PreprationTime,
    ISNULL(i.ClientComments, '') AS ClientComments,
    ISNULL(i.Rating, 0) AS Rating,
    i.CreatedOn,
    ISNULL(i.CreatedBy, 0) AS CreatedBy,
    ISNULL(u.Username, '') AS Username,
    ISNULL(e.FullName, '') AS EmployeeFullName
    
FROM Invoice i
LEFT JOIN Parties p ON i.PartyId = p.Id
LEFT JOIN Locations l ON i.LocationId = l.Id
LEFT JOIN Users u ON i.CreatedBy = u.Id
LEFT JOIN Employees e ON i.SalesId = e.Id
WHERE i.IsSoftDeleted = 0;

GO

CREATE OR ALTER VIEW vw_InvoiceDetailsReport AS
SELECT 
    id.Id AS InvoiceDetailId,
    id.InvoiceId,
    i.Code AS SeqNo,
    id.ItemId,
    ISNULL(it.Name, id.Description) AS ItemName,
    ISNULL(id.ServingSize, '') AS ServingSize,
    ISNULL(id.StockCondition, '') AS StockCondition,
    id.Qty,
    id.UnitPrice,
    id.DiscountAmount,
    ISNULL((
        SELECT SUM(idt.TaxAmount)
        FROM InvoiceDetailTax idt
        WHERE idt.InvoiceDetailId = id.Id
    ), 0) AS TaxAmount,
    ((id.UnitPrice * id.Qty) - id.DiscountAmount + ISNULL((
        SELECT SUM(idt.TaxAmount)
        FROM InvoiceDetailTax idt
        WHERE idt.InvoiceDetailId = id.Id
    ), 0)) AS ItemTotalAmount,
    id.Description AS Instructions,
    ISNULL(id.Status, '') AS Status,
    0 AS Person,
    id.TranDate,
    ISNULL(id.Rating, 0) AS Rating,
    id.IsSoftDeleted
FROM InvoiceDetail id
INNER JOIN Invoice i ON id.InvoiceId = i.Id
LEFT JOIN Items it ON id.ItemId = it.Id
WHERE i.IsSoftDeleted = 0;

GO

CREATE OR ALTER VIEW vw_InvoiceChargesReport AS
SELECT 
    ic.Id AS ChargeId,
    ic.InvoiceId,
    i.Code AS SeqNo,
    ic.RulesId AS ChargeRuleId,
    ISNULL(coa.Name, '') AS RuleName,
    ic.ChargeCategory,
    ic.AmountType,
    ic.Amount AS Rate,
    ic.AppliedAmount,
    0 AS SequenceOrder,
    '' AS CalculationBase
FROM InvoiceCharges ic
INNER JOIN Invoice i ON ic.InvoiceId = i.Id
LEFT JOIN ChartOfAccounts coa ON ic.AccountId = coa.Id
WHERE i.IsSoftDeleted = 0 AND ic.IsSoftDeleted = 0;

GO

CREATE OR ALTER VIEW vw_InvoicePaymentsReport AS
SELECT 
    ip.Id AS PaymentId,
    ip.InvoiceId,
    i.Code AS SeqNo,
    i.PartyId,
    ISNULL(p.Name, '') AS PartyName,
    ISNULL(coa.Name, '') AS PaymentMethod,
    ISNULL(ip.PaymentRef, '') AS PaymentRef,
    ip.Amount AS AmountPaid,
    ip.PaidOn,
    ISNULL(ip.Notes, '') AS Notes
FROM InvoicePayments ip
INNER JOIN Invoice i ON ip.InvoiceId = i.Id
LEFT JOIN Parties p ON i.PartyId = p.Id
LEFT JOIN ChartOfAccounts coa ON ip.AccountId = coa.Id
WHERE i.IsSoftDeleted = 0 AND ip.IsSoftDeleted = 0;

GO

CREATE OR ALTER VIEW vw_InvoiceTaxesReport AS
SELECT 
    idt.Id AS TaxId,
    id.InvoiceId,
    idt.InvoiceDetailId,
    ISNULL(tm.TaxName, '') AS TaxName,
    idt.TaxRate,
    idt.TaxableAmount,
    idt.TaxAmount,
    idt.CalculationOrder
FROM InvoiceDetailTax idt
INNER JOIN InvoiceDetail id ON idt.InvoiceDetailId = id.Id
INNER JOIN Invoice i ON id.InvoiceId = i.Id
LEFT JOIN TaxMaster tm ON idt.TaxId = tm.Id
WHERE i.IsSoftDeleted = 0 AND id.IsSoftDeleted = 0;

GO

CREATE OR ALTER VIEW InvoiceReport AS
SELECT * FROM vw_InvoiceMasterReport;


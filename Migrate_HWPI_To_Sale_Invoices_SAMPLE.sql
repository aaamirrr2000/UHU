-- =============================================
-- SAMPLE: Sale invoice (Gross Revenue + payment methods only)
--        + Separate GL journal entries for Margin on Sales and Tax on Margin
-- =============================================
-- Business rule:
--   - Sale = money received → Invoice has ONLY Gross Revenue line + Payment(s) by method.
--   - Margin on Sales and Tax on Margin are booked via separate journal entries (not on invoice).
-- =============================================
-- Source row (2026-01-02): Gross Revenue 2885 (Cash), Margin 224, Tax on Margin 403
-- COA: 70=Gross Revenue, 74=Margin on Sales, 85=Tax on Margin, 22=Cash, 30=Operating Expense
-- =============================================
SET NOCOUNT ON;

DECLARE @OrganizationId     INT = 1;
DECLARE @UserId             INT = 1;
DECLARE @LocationId         INT = 1;
DECLARE @PartyId            INT = 2;   -- Customer with AR account
DECLARE @ARAccountId        INT = 12;  -- ACCOUNTS RECEIVABLE
DECLARE @BaseCurrencyId     INT = 1;
DECLARE @EnteredCurrencyId  INT = 1;
DECLARE @ExchangeRate       DECIMAL(18,6) = 1.0;

-- Amounts for sample (2026-01-02)
DECLARE @GrossRevenueAmount DECIMAL(18,2) = 2885.00;   -- Sale = money received
DECLARE @MarginAmount      DECIMAL(18,2) = 224.00;    -- Booked via journal
DECLARE @TaxOnMarginAmount DECIMAL(18,2) = 403.00;    -- Booked via journal

-- Payment method account from ChartOfAccounts: InterfaceType = 'PAYMENT METHOD', Name = 'CASH' (COA Id 22)
DECLARE @PaymentAccountId INT;
SELECT @PaymentAccountId = COALESCE(
  (SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'PAYMENT METHOD' AND UPPER(RTRIM(Name)) = 'CASH' AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY Code),
  (SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'PAYMENT METHOD' AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY Code),
  (SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'BANK' AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY Code)
);
IF @PaymentAccountId IS NULL OR @PaymentAccountId = 0
  RAISERROR('ChartOfAccounts: no account with InterfaceType ''PAYMENT METHOD'' or ''BANK''. Add at least one.', 16, 1);

DECLARE @InvoiceId    INT;
DECLARE @GLHeaderId1  INT;
DECLARE @GLHeaderId2  INT;
DECLARE @EntryNo1     VARCHAR(50);
DECLARE @EntryNo2     VARCHAR(50);

-- -------------------------------------------------------------------------
-- PART A: SALE INVOICE (Gross Revenue only + Payment method)
-- -------------------------------------------------------------------------

-- A1) Invoice header
INSERT INTO Invoice
(
    OrganizationId, Code, InvoiceType, Source, SalesId, LocationId, AccountId,
    PartyId, PartyName, PartyPhone, PartyEmail, PartyAddress,
    TranDate, Description, Status, ClientComments, Rating,
    IsPostedToGL, PostedToGLDate, PostedToGLBy, GLEntryNo,
    CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted,
    BaseCurrencyId, EnteredCurrencyId, ExchangeRate
)
VALUES
(
    @OrganizationId,
    'INV-HWPI-20260102-001',
    'SALE',
    'MIGRATION',
    NULL,
    @LocationId,
    @ARAccountId,
    @PartyId,
    'AAMIR RASHID',
    NULL, NULL, NULL,
    '2026-01-02',
    'HWPI migration - Gross Revenue 2026-01-02 (Cash)',
    'POSTED',
    NULL, NULL,
    0, NULL, NULL, NULL,
    @UserId, GETUTCDATE(), 'Migrate_HWPI_To_Sale_Invoices_SAMPLE.sql',
    @UserId, GETUTCDATE(), NULL, 0,
    @BaseCurrencyId, @EnteredCurrencyId, @ExchangeRate
);
SET @InvoiceId = SCOPE_IDENTITY();

-- A2) Single line: Gross Revenue only (COA 70)
INSERT INTO InvoiceDetail
(
    ItemId, StockCondition, ManualItem, AccountId, ServingSize,
    Qty, UnitPrice, DiscountAmount, InvoiceId, Description, Status, Rating, TranDate, IsSoftDeleted
)
VALUES
(
    NULL, NULL, 'Gross Revenue', 70, NULL,
    1.0000, @GrossRevenueAmount, 0.00, @InvoiceId,
    'Gross Revenue - HWPI 2026-01-02', NULL, NULL, '2026-01-02', 0
);

-- A3) Payment: Cash (full amount received)
INSERT INTO InvoicePayments
( InvoiceId, AccountId, PaymentRef, Amount, PaidOn, Notes, IsSoftDeleted )
VALUES
( @InvoiceId, @PaymentAccountId, 'CASH-HWPI-20260102', @GrossRevenueAmount, '2026-01-02', 'Cash - HWPI migration', 0 );

-- -------------------------------------------------------------------------
-- PART B: JOURNAL ENTRY 1 – Margin on Sales
-- Dr Operating Expense (30), Cr Margin on Sales (74)
-- -------------------------------------------------------------------------

DECLARE @NextSeq INT;
SELECT @NextSeq = ISNULL(MAX(CAST(NULLIF(LTRIM(SUBSTRING(EntryNo, 3, 50)), '') AS INT)), 0) + 1
  FROM GeneralLedgerHeader WHERE EntryNo LIKE 'GL%' AND LEN(EntryNo) >= 3;
SET @EntryNo1 = 'GL' + RIGHT('000000' + CAST(@NextSeq AS VARCHAR(10)), 6);

INSERT INTO GeneralLedgerHeader
(
    OrganizationId, EntryNo, EntryDate, Source, Description, ReferenceNo, ReferenceType, ReferenceId, PartyId, LocationId,
    TotalDebit, TotalCredit, IsReversed, ReversedEntryNo, IsPosted, PostedDate, PostedBy, IsAdjusted, AdjustmentEntryNo, FileAttachment, Notes,
    CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted,
    BaseCurrencyId, EnteredCurrencyId, ExchangeRate
)
VALUES
(
    @OrganizationId, @EntryNo1, '2026-01-02', 'MANUAL',
    'Margin on Sales - HWPI 2026-01-02',
    'J-MARGIN-20260102', 'JOURNAL', NULL, @PartyId, @LocationId,
    @MarginAmount, @MarginAmount, 0, NULL, 1, GETUTCDATE(), @UserId, 0, NULL, NULL, 'HWPI migration - Margin on Sales',
    @UserId, GETUTCDATE(), 'Migrate_HWPI_To_Sale_Invoices_SAMPLE.sql', @UserId, GETUTCDATE(), NULL, 0,
    @BaseCurrencyId, @EnteredCurrencyId, @ExchangeRate
);
SET @GLHeaderId1 = SCOPE_IDENTITY();

INSERT INTO GeneralLedgerDetail ( HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted )
VALUES ( @GLHeaderId1, 30, 'OPERATING EXPENSE - MARGIN ALLOCATION', @MarginAmount, 0, @PartyId, 1, @UserId, GETUTCDATE(), NULL, @UserId, GETUTCDATE(), NULL, 0 );
INSERT INTO GeneralLedgerDetail ( HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted )
VALUES ( @GLHeaderId1, 74, 'MARGIN ON SALES - HWPI 2026-01-02', 0, @MarginAmount, @PartyId, 2, @UserId, GETUTCDATE(), NULL, @UserId, GETUTCDATE(), NULL, 0 );

-- -------------------------------------------------------------------------
-- PART C: JOURNAL ENTRY 2 – Tax on Margin
-- Dr Operating Expense (30), Cr Tax on Margin (85)
-- -------------------------------------------------------------------------

SET @NextSeq = @NextSeq + 1;
SET @EntryNo2 = 'GL' + RIGHT('000000' + CAST(@NextSeq AS VARCHAR(10)), 6);

INSERT INTO GeneralLedgerHeader
(
    OrganizationId, EntryNo, EntryDate, Source, Description, ReferenceNo, ReferenceType, ReferenceId, PartyId, LocationId,
    TotalDebit, TotalCredit, IsReversed, ReversedEntryNo, IsPosted, PostedDate, PostedBy, IsAdjusted, AdjustmentEntryNo, FileAttachment, Notes,
    CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted,
    BaseCurrencyId, EnteredCurrencyId, ExchangeRate
)
VALUES
(
    @OrganizationId, @EntryNo2, '2026-01-02', 'MANUAL',
    'Tax on Margin - HWPI 2026-01-02',
    'J-TAX-20260102', 'JOURNAL', NULL, @PartyId, @LocationId,
    @TaxOnMarginAmount, @TaxOnMarginAmount, 0, NULL, 1, GETUTCDATE(), @UserId, 0, NULL, NULL, 'HWPI migration - Tax on Margin',
    @UserId, GETUTCDATE(), 'Migrate_HWPI_To_Sale_Invoices_SAMPLE.sql', @UserId, GETUTCDATE(), NULL, 0,
    @BaseCurrencyId, @EnteredCurrencyId, @ExchangeRate
);
SET @GLHeaderId2 = SCOPE_IDENTITY();

INSERT INTO GeneralLedgerDetail ( HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted )
VALUES ( @GLHeaderId2, 30, 'OPERATING EXPENSE - TAX ON MARGIN ALLOCATION', @TaxOnMarginAmount, 0, @PartyId, 1, @UserId, GETUTCDATE(), NULL, @UserId, GETUTCDATE(), NULL, 0 );
INSERT INTO GeneralLedgerDetail ( HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted )
VALUES ( @GLHeaderId2, 85, 'TAX ON MARGIN - HWPI 2026-01-02', 0, @TaxOnMarginAmount, @PartyId, 2, @UserId, GETUTCDATE(), NULL, @UserId, GETUTCDATE(), NULL, 0 );

-- -------------------------------------------------------------------------
-- Summary (one sample record for testing)
-- -------------------------------------------------------------------------
SELECT
    @InvoiceId          AS SaleInvoiceId,
    'INV-HWPI-20260102-001' AS InvoiceCode,
    @GrossRevenueAmount AS GrossRevenue_OnInvoice,
    @PaymentAccountId   AS PaymentMethod_AccountId_Cash,
    @GLHeaderId1        AS JournalId_MarginOnSales,
    @EntryNo1           AS JournalNo_Margin,
    @MarginAmount       AS MarginAmount,
    @GLHeaderId2        AS JournalId_TaxOnMargin,
    @EntryNo2           AS JournalNo_Tax,
    @TaxOnMarginAmount  AS TaxOnMarginAmount;

PRINT 'Done. Sale Invoice Id = ' + CAST(@InvoiceId AS VARCHAR(20)) + ' (Gross Revenue + Cash payment only).';
PRINT 'Journal 1 (Margin on Sales): ' + ISNULL(@EntryNo1, '') + ', HeaderId = ' + CAST(@GLHeaderId1 AS VARCHAR(20));
PRINT 'Journal 2 (Tax on Margin):   ' + ISNULL(@EntryNo2, '') + ', HeaderId = ' + CAST(@GLHeaderId2 AS VARCHAR(20));
PRINT 'Post invoice to GL via API: POST CreateGLFromInvoice/' + CAST(@InvoiceId AS VARCHAR(20));

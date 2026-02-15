-- =============================================
-- FULL: Sale invoices (Gross Revenue + payment methods) + GL journals for Margin and Tax
-- LocationId = 1 where required.
-- One invoice per day; payments split by Method; one journal per day for Margin, one for Tax.
-- =============================================
SET NOCOUNT ON;

DECLARE @OrganizationId     INT = 1;
DECLARE @UserId             INT = 1;
DECLARE @LocationId         INT = 1;
DECLARE @PartyId            INT = 2;
DECLARE @ARAccountId         INT = 12;
DECLARE @BaseCurrencyId     INT = 1;
DECLARE @EnteredCurrencyId  INT = 1;
DECLARE @ExchangeRate       DECIMAL(18,6) = 1.0;

-- Use LocationId = 1 only if Locations table has row 1 (otherwise NULL so inserts succeed)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Locations') OR NOT EXISTS (SELECT 1 FROM Locations WHERE Id = 1)
  SET @LocationId = NULL;

-- Source data (same as Migrate_HWPI_Revenue_Margin_Tax_To_GL.sql)
DECLARE @Data TABLE (Seq INT IDENTITY(1,1), TranDate DATE, OnAccount VARCHAR(50), Amount DECIMAL(18,2), Method VARCHAR(100), Details VARCHAR(500));

INSERT INTO @Data (TranDate, OnAccount, Amount, Method, Details) VALUES
('2026-01-02', 'Gross Revenue', 2885, 'Cash', NULL),
('2026-01-02', 'Marin on Sales', 224, 'Non Cash Transaction', NULL),
('2026-01-02', 'Tax on Margin', 403, 'Non Cash Transaction', NULL),
('2026-01-03', 'Gross Revenue', 955, 'Cash', NULL),
('2026-01-03', 'Marin on Sales', 135, 'Non Cash Transaction', NULL),
('2026-01-03', 'Tax on Margin', 62, 'Non Cash Transaction', NULL),
('2026-01-04', 'Gross Revenue', 22906, 'Cash', NULL),
('2026-01-04', 'Marin on Sales', 2510, 'Non Cash Transaction', NULL),
('2026-01-04', 'Tax on Margin', 182, 'Non Cash Transaction', NULL),
('2026-01-05', 'Gross Revenue', 29150, 'Cash', NULL),
('2026-01-05', 'Marin on Sales', 2875, 'Non Cash Transaction', NULL),
('2026-01-05', 'Tax on Margin', 1167, 'Non Cash Transaction', NULL),
('2026-01-06', 'Gross Revenue', 33054, 'Cash', NULL),
('2026-01-06', 'Marin on Sales', 4349, 'Non Cash Transaction', NULL),
('2026-01-06', 'Tax on Margin', 507, 'Non Cash Transaction', NULL),
('2026-01-07', 'Gross Revenue', 51335, 'Cash', NULL),
('2026-01-07', 'Marin on Sales', 6874, 'Non Cash Transaction', NULL),
('2026-01-07', 'Tax on Margin', 1748, 'Non Cash Transaction', NULL),
('2026-01-08', 'Gross Revenue', 33129, 'Cash', NULL),
('2026-01-08', 'Marin on Sales', 4831, 'Non Cash Transaction', NULL),
('2026-01-08', 'Tax on Margin', 1748, 'Non Cash Transaction', NULL),
('2026-01-09', 'Gross Revenue', 47084, 'Cash', NULL),
('2026-01-09', 'Marin on Sales', 6311, 'Non Cash Transaction', NULL),
('2026-01-09', 'Tax on Margin', 832, 'Non Cash Transaction', NULL),
('2026-01-10', 'Gross Revenue', 46893, 'Cash', NULL),
('2026-01-10', 'Marin on Sales', 7999, 'Non Cash Transaction', NULL),
('2026-01-10', 'Tax on Margin', 2325, 'Non Cash Transaction', NULL),
('2026-01-11', 'Gross Revenue', 31617, 'Cash', NULL),
('2026-01-11', 'Marin on Sales', 4225, 'Non Cash Transaction', NULL),
('2026-01-11', 'Tax on Margin', 372, 'Non Cash Transaction', NULL),
('2026-01-12', 'Gross Revenue', 93593, 'Cash', NULL),
('2026-01-12', 'Marin on Sales', 11893, 'Non Cash Transaction', NULL),
('2026-01-12', 'Tax on Margin', 3893, 'Non Cash Transaction', NULL),
('2026-01-13', 'Gross Revenue', 56253, 'Cash', NULL),
('2026-01-13', 'Marin on Sales', 10001, 'Non Cash Transaction', NULL),
('2026-01-13', 'Tax on Margin', 990, 'Non Cash Transaction', NULL),
('2026-01-14', 'Gross Revenue', 40784, 'Cash', NULL),
('2026-01-14', 'Gross Revenue', 19805, 'Jazz QR Code', NULL),
('2026-01-14', 'Marin on Sales', 9876, 'Non Cash Transaction', NULL),
('2026-01-14', 'Tax on Margin', 768, 'Non Cash Transaction', NULL),
('2026-01-15', 'Gross Revenue', 29685, 'Cash', NULL),
('2026-01-15', 'Gross Revenue', 2800, 'Jazz QR Code', NULL),
('2026-01-15', 'Gross Revenue', 2090, 'Bank - UBL', NULL),
('2026-01-15', 'Marin on Sales', 4636, 'Non Cash Transaction', NULL),
('2026-01-15', 'Tax on Margin', 598, 'Non Cash Transaction', NULL),
('2026-01-16', 'Gross Revenue', 64410, 'Cash', NULL),
('2026-01-16', 'Gross Revenue', 3604, 'Jazz QR Code', NULL),
('2026-01-16', 'Gross Revenue', 23025, 'Bank - UBL', NULL),
('2026-01-16', 'Marin on Sales', 13676, 'Non Cash Transaction', NULL),
('2026-01-16', 'Tax on Margin', 1963, 'Non Cash Transaction', NULL),
('2026-01-17', 'Gross Revenue', 16900, 'Cash', NULL),
('2026-01-17', 'Gross Revenue', 9063, 'Jazz QR Code', NULL),
('2026-01-17', 'Marin on Sales', 3558, 'Non Cash Transaction', NULL),
('2026-01-17', 'Tax on Margin', 542, 'Non Cash Transaction', NULL),
('2026-01-18', 'Gross Revenue', 16594, 'Cash', NULL),
('2026-01-18', 'Gross Revenue', 890, 'Jazz QR Code', NULL),
('2026-01-18', 'Gross Revenue', 9575, 'Bank - UBL', NULL),
('2026-01-18', 'Marin on Sales', 4175, 'Non Cash Transaction', NULL),
('2026-01-18', 'Tax on Margin', 0, 'Non Cash Transaction', NULL),
('2026-01-19', 'Gross Revenue', 72267, 'Cash', NULL),
('2026-01-19', 'Gross Revenue', 5190, 'Jazz QR Code', NULL),
('2026-01-19', 'Gross Revenue', 23539, 'POS', NULL),
('2026-01-19', 'Gross Revenue', 4160, 'Bank - UBL', NULL),
('2026-01-19', 'Marin on Sales', 14120, 'Non Cash Transaction', NULL),
('2026-01-19', 'Tax on Margin', 2962, 'Non Cash Transaction', NULL),
('2026-01-20', 'Gross Revenue', 42616, 'Cash', NULL),
('2026-01-20', 'Gross Revenue', 3985, 'Jazz QR Code', NULL),
('2026-01-20', 'Gross Revenue', 4020, 'POS', NULL),
('2026-01-20', 'Gross Revenue', 5240, 'Bank - UBL', NULL),
('2026-01-20', 'Marin on Sales', 10236, 'Non Cash Transaction', NULL),
('2026-01-20', 'Tax on Margin', 491, 'Non Cash Transaction', NULL),
('2026-01-21', 'Gross Revenue', 40906, 'Cash', NULL),
('2026-01-21', 'Gross Revenue', 1010, 'Jazz QR Code', NULL),
('2026-01-21', 'Gross Revenue', 2395, 'POS', NULL),
('2026-01-21', 'Gross Revenue', 240, 'Bank - UBL', NULL),
('2026-01-21', 'Marin on Sales', 6239, 'Non Cash Transaction', NULL),
('2026-01-21', 'Tax on Margin', 405, 'Non Cash Transaction', NULL),
('2026-01-22', 'Gross Revenue', 30409, 'Cash', NULL),
('2026-01-22', 'Gross Revenue', 5188, 'Jazz QR Code', NULL),
('2026-01-22', 'Gross Revenue', 19735, 'POS', NULL),
('2026-01-22', 'Gross Revenue', 6577, 'Bank - UBL', NULL),
('2026-01-22', 'Marin on Sales', 8729, 'Non Cash Transaction', NULL),
('2026-01-22', 'Tax on Margin', 1086, 'Non Cash Transaction', NULL),
('2026-01-23', 'Gross Revenue', 35919, 'Cash', NULL),
('2026-01-23', 'Gross Revenue', 1567, 'Jazz QR Code', NULL),
('2026-01-23', 'Gross Revenue', 22330, 'POS', NULL),
('2026-01-23', 'Gross Revenue', 70, 'Bank - UBL', NULL),
('2026-01-23', 'Marin on Sales', 6726, 'Non Cash Transaction', NULL),
('2026-01-23', 'Tax on Margin', 1690, 'Non Cash Transaction', NULL),
('2026-01-24', 'Gross Revenue', 25304, 'Cash', NULL),
('2026-01-24', 'Gross Revenue', 11592, 'Jazz QR Code', NULL),
('2026-01-24', 'Gross Revenue', 8110, 'POS', NULL),
('2026-01-24', 'Gross Revenue', 1090, 'Bank - UBL', NULL),
('2026-01-24', 'Marin on Sales', 6865, 'Non Cash Transaction', NULL),
('2026-01-24', 'Tax on Margin', 787, 'Non Cash Transaction', NULL),
('2026-01-25', 'Gross Revenue', 28751, 'Cash', NULL),
('2026-01-25', 'Gross Revenue', 650, 'Jazz QR Code', NULL),
('2026-01-25', 'Gross Revenue', 12657, 'POS', NULL),
('2026-01-25', 'Gross Revenue', 70, 'Bank - UBL', NULL),
('2026-01-25', 'Marin on Sales', 6505, 'Non Cash Transaction', NULL),
('2026-01-25', 'Tax on Margin', 353, 'Non Cash Transaction', NULL),
('2026-01-26', 'Gross Revenue', 47897, 'Cash', NULL),
('2026-01-26', 'Gross Revenue', 1330, 'Jazz QR Code', NULL),
('2026-01-26', 'Gross Revenue', 20825, 'POS', NULL),
('2026-01-26', 'Gross Revenue', 5026, 'Bank - UBL', NULL),
('2026-01-26', 'Marin on Sales', 12562, 'Non Cash Transaction', NULL),
('2026-01-26', 'Tax on Margin', 1125, 'Non Cash Transaction', NULL),
('2026-01-27', 'Gross Revenue', 42400, 'Cash', NULL),
('2026-01-27', 'Gross Revenue', 150, 'Jazz QR Code', NULL),
('2026-01-27', 'Gross Revenue', 24189, 'POS', NULL),
('2026-01-27', 'Gross Revenue', 100, 'Bank - UBL', NULL),
('2026-01-27', 'Marin on Sales', 9818, 'Non Cash Transaction', NULL),
('2026-01-27', 'Tax on Margin', 606, 'Non Cash Transaction', NULL),
('2026-01-28', 'Gross Revenue', 47050, 'Cash', NULL),
('2026-01-28', 'Gross Revenue', 3610, 'Jazz QR Code', NULL),
('2026-01-28', 'Gross Revenue', 14065, 'POS', NULL),
('2026-01-28', 'Marin on Sales', 8836, 'Non Cash Transaction', NULL),
('2026-01-28', 'Tax on Margin', 2197, 'Non Cash Transaction', NULL),
('2026-01-29', 'Gross Revenue', 46275, 'Cash', NULL),
('2026-01-29', 'Gross Revenue', 420, 'Jazz QR Code', NULL),
('2026-01-29', 'Gross Revenue', 15390, 'POS', NULL),
('2026-01-29', 'Gross Revenue', 870, 'Bank - UBL', NULL),
('2026-01-29', 'Marin on Sales', 7798, 'Non Cash Transaction', NULL),
('2026-01-29', 'Tax on Margin', 2782, 'Non Cash Transaction', NULL),
('2026-01-30', 'Gross Revenue', 32560, 'Cash', NULL),
('2026-01-30', 'Gross Revenue', 400, 'Jazz QR Code', NULL),
('2026-01-30', 'Gross Revenue', 45100, 'POS', NULL),
('2026-01-30', 'Marin on Sales', 11828, 'Non Cash Transaction', NULL),
('2026-01-30', 'Tax on Margin', 1713, 'Non Cash Transaction', NULL),
('2026-01-31', 'Gross Revenue', 45713, 'Cash', NULL),
('2026-01-31', 'Gross Revenue', 1440, 'Jazz QR Code', NULL),
('2026-01-31', 'Gross Revenue', 25804, 'POS', NULL),
('2026-01-31', 'Gross Revenue', 170, 'Bank - UBL', NULL),
('2026-01-31', 'Marin on Sales', 11067, 'Non Cash Transaction', NULL),
('2026-01-31', 'Tax on Margin', 166, 'Non Cash Transaction', NULL),
('2026-02-01', 'Gross Revenue', 44979, 'Cash', NULL),
('2026-02-01', 'Gross Revenue', 2900, 'Jazz QR Code', NULL),
('2026-02-01', 'Gross Revenue', 5262, 'POS', NULL),
('2026-02-01', 'Gross Revenue', 250, 'Bank - UBL', NULL),
('2026-02-01', 'Marin on Sales', 6562, 'Non Cash Transaction', NULL),
('2026-02-01', 'Tax on Margin', 502, 'Non Cash Transaction', NULL),
('2026-02-02', 'Gross Revenue', 62853, 'Cash', NULL),
('2026-02-02', 'Gross Revenue', 5588, 'Jazz QR Code', NULL),
('2026-02-02', 'Gross Revenue', 27370, 'POS', NULL),
('2026-02-02', 'Gross Revenue', 0, 'Bank - UBL', NULL),
('2026-02-02', 'Marin on Sales', 13164, 'Non Cash Transaction', NULL),
('2026-02-02', 'Tax on Margin', 1209, 'Non Cash Transaction', NULL),
('2026-02-03', 'Gross Revenue', 53828, 'Cash', NULL),
('2026-02-03', 'Gross Revenue', 2380, 'Jazz QR Code', NULL),
('2026-02-03', 'Gross Revenue', 17055, 'POS', NULL),
('2026-02-03', 'Gross Revenue', 2670, 'Bank - UBL', NULL),
('2026-02-03', 'Marin on Sales', 10094, 'Non Cash Transaction', NULL),
('2026-02-03', 'Tax on Margin', 942, 'Non Cash Transaction', NULL),
('2026-02-04', 'Gross Revenue', 77304, 'Cash', NULL),
('2026-02-04', 'Gross Revenue', 100, 'Jazz QR Code', NULL),
('2026-02-04', 'Gross Revenue', 13312, 'POS', NULL),
('2026-02-04', 'Gross Revenue', 1875, 'Bank - UBL', NULL),
('2026-02-04', 'Marin on Sales', 11310, 'Non Cash Transaction', NULL),
('2026-02-04', 'Tax on Margin', 840, 'Non Cash Transaction', NULL);

-- Per-day aggregates
DECLARE @Days TABLE (TranDate DATE PRIMARY KEY, GrossTotal DECIMAL(18,2), MarginAmount DECIMAL(18,2), TaxAmount DECIMAL(18,2));
INSERT INTO @Days (TranDate, GrossTotal, MarginAmount, TaxAmount)
SELECT d.TranDate,
       ISNULL((SELECT SUM(Amount) FROM @Data x WHERE x.TranDate = d.TranDate AND x.OnAccount = 'Gross Revenue'), 0),
       ISNULL((SELECT TOP 1 Amount FROM @Data x WHERE x.TranDate = d.TranDate AND x.OnAccount = 'Marin on Sales'), 0),
       ISNULL((SELECT TOP 1 Amount FROM @Data x WHERE x.TranDate = d.TranDate AND x.OnAccount = 'Tax on Margin'), 0)
FROM (SELECT DISTINCT TranDate FROM @Data) d;

-- Payments per day (Gross Revenue only; skip zero)
DECLARE @Payments TABLE (TranDate DATE, Method VARCHAR(100), Amount DECIMAL(18,2));
INSERT INTO @Payments (TranDate, Method, Amount)
SELECT TranDate, Method, Amount
FROM @Data
WHERE OnAccount = 'Gross Revenue' AND Amount > 0;

-- Payment method -> AccountId: scan ChartOfAccounts by InterfaceType + Name (COA seed: CASH=22, JAZZCASH=26, PAYMENT BANK=18, etc.)
DECLARE @PaymentAccountLookup TABLE (Method VARCHAR(100) PRIMARY KEY, AccountId INT);
DECLARE @FallbackPay INT = (SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'PAYMENT METHOD' AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY Code);
IF @FallbackPay IS NULL SET @FallbackPay = (SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'BANK' AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY Code);

INSERT INTO @PaymentAccountLookup (Method, AccountId)
SELECT 'Cash', COALESCE((SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'PAYMENT METHOD' AND UPPER(RTRIM(Name)) = 'CASH' AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY Code), @FallbackPay)
UNION ALL SELECT 'Jazz QR Code', COALESCE((SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'PAYMENT METHOD' AND (UPPER(RTRIM(Name)) LIKE '%JAZZ%') AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY Code), @FallbackPay)
UNION ALL SELECT 'Bank - UBL', COALESCE((SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'BANK' AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY Code), (SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'PAYMENT METHOD' AND UPPER(RTRIM(Name)) LIKE '%BANK%' AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY Code), @FallbackPay)
UNION ALL SELECT 'POS', COALESCE((SELECT TOP 1 Id FROM ChartOfAccounts WHERE InterfaceType = 'PAYMENT METHOD' AND (UPPER(RTRIM(Name)) = 'CASH' OR UPPER(RTRIM(Name)) LIKE '%DEBIT%') AND IsActive = 1 AND ISNULL(IsSoftDeleted,0) = 0 ORDER BY CASE WHEN UPPER(RTRIM(Name)) = 'CASH' THEN 1 ELSE 2 END, Code), @FallbackPay);

-- Any other Method from data (e.g. IBFT, EasyPaisa) -> fallback
INSERT INTO @PaymentAccountLookup (Method, AccountId)
SELECT DISTINCT p.Method, @FallbackPay FROM @Payments p
WHERE NOT EXISTS (SELECT 1 FROM @PaymentAccountLookup l WHERE l.Method = p.Method) AND @FallbackPay IS NOT NULL;

DECLARE @CurDate       DATE;
DECLARE @GrossTotal    DECIMAL(18,2);
DECLARE @MarginAmount  DECIMAL(18,2);
DECLARE @TaxAmount     DECIMAL(18,2);
DECLARE @InvoiceId     INT;
DECLARE @InvoiceCode   VARCHAR(50);
DECLARE @GLSeq         INT;
DECLARE @EntryNo       VARCHAR(50);
DECLARE @HeaderId      INT;
DECLARE @PayMethod     VARCHAR(100);
DECLARE @PayAmount     DECIMAL(18,2);
DECLARE @PayAccountId  INT;

-- Next GL sequence (ignore if GeneralLedgerHeader missing or no GL* entries)
SET @GLSeq = 0;
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GeneralLedgerHeader')
  SELECT @GLSeq = ISNULL(MAX(
    CASE WHEN LEN(EntryNo) >= 8 AND SUBSTRING(EntryNo, 3, 6) NOT LIKE '%[^0-9]%'
         THEN CAST(SUBSTRING(EntryNo, 3, 6) AS INT)
         ELSE NULL END
  ), 0)
  FROM GeneralLedgerHeader
  WHERE EntryNo LIKE 'GL%';

DECLARE curDays CURSOR LOCAL FAST_FORWARD FOR
SELECT TranDate, GrossTotal, MarginAmount, TaxAmount FROM @Days ORDER BY TranDate;

OPEN curDays;
FETCH NEXT FROM curDays INTO @CurDate, @GrossTotal, @MarginAmount, @TaxAmount;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF @GrossTotal <= 0
    BEGIN
        FETCH NEXT FROM curDays INTO @CurDate, @GrossTotal, @MarginAmount, @TaxAmount;
        CONTINUE;
    END

    SET @InvoiceCode = 'INV-HWPI-' + CONVERT(VARCHAR(8), @CurDate, 112) + '-001';

    -- Invoice header (LocationId = 1)
    INSERT INTO Invoice
    ( OrganizationId, Code, InvoiceType, Source, SalesId, LocationId, AccountId, PartyId, PartyName, PartyPhone, PartyEmail, PartyAddress,
      TranDate, Description, Status, ClientComments, Rating, IsPostedToGL, PostedToGLDate, PostedToGLBy, GLEntryNo,
      CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted, BaseCurrencyId, EnteredCurrencyId, ExchangeRate )
    VALUES
    ( @OrganizationId, @InvoiceCode, 'SALE', 'MIGRATION', NULL, @LocationId, @ARAccountId, @PartyId, 'AAMIR RASHID', NULL, NULL, NULL,
      @CurDate, 'HWPI migration - Gross Revenue ' + CONVERT(VARCHAR(10), @CurDate, 120), 'POSTED', NULL, NULL, 0, NULL, NULL, NULL,
      @UserId, GETUTCDATE(), 'Migrate_HWPI_To_Sale_Invoices_FULL.sql', @UserId, GETUTCDATE(), NULL, 0, @BaseCurrencyId, @EnteredCurrencyId, @ExchangeRate );
    SET @InvoiceId = SCOPE_IDENTITY();

    -- One line: Gross Revenue (70)
    INSERT INTO InvoiceDetail ( ItemId, StockCondition, ManualItem, AccountId, ServingSize, Qty, UnitPrice, DiscountAmount, InvoiceId, Description, Status, Rating, TranDate, IsSoftDeleted )
    VALUES ( NULL, NULL, 'Gross Revenue', 70, NULL, 1.0000, @GrossTotal, 0.00, @InvoiceId, 'Gross Revenue - HWPI ' + CONVERT(VARCHAR(10), @CurDate, 120), NULL, NULL, @CurDate, 0 );

    -- Payments by method: AccountId from ChartOfAccounts (InterfaceType = 'PAYMENT METHOD')
    DECLARE curPay CURSOR LOCAL FAST_FORWARD FOR
    SELECT p.Method, p.Amount, l.AccountId
    FROM @Payments p
    INNER JOIN @PaymentAccountLookup l ON l.Method = p.Method
    WHERE p.TranDate = @CurDate AND l.AccountId IS NOT NULL AND l.AccountId > 0;
    OPEN curPay;
    FETCH NEXT FROM curPay INTO @PayMethod, @PayAmount, @PayAccountId;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        INSERT INTO InvoicePayments ( InvoiceId, AccountId, PaymentRef, Amount, PaidOn, Notes, IsSoftDeleted )
        VALUES ( @InvoiceId, @PayAccountId, UPPER(REPLACE(@PayMethod, ' ', '-')) + '-HWPI-' + CONVERT(VARCHAR(8), @CurDate, 112), @PayAmount, @CurDate, 'HWPI migration', 0 );
        FETCH NEXT FROM curPay INTO @PayMethod, @PayAmount, @PayAccountId;
    END
    CLOSE curPay;
    DEALLOCATE curPay;

    -- Journal: Margin on Sales (Dr 30, Cr 74). LocationId = 1.
    IF @MarginAmount > 0
    BEGIN
        SET @GLSeq = @GLSeq + 1;
        SET @EntryNo = 'GL' + RIGHT('000000' + CAST(@GLSeq AS VARCHAR(10)), 6);
        INSERT INTO GeneralLedgerHeader
        ( OrganizationId, EntryNo, EntryDate, Source, Description, ReferenceNo, ReferenceType, ReferenceId, PartyId, LocationId,
          TotalDebit, TotalCredit, IsReversed, ReversedEntryNo, IsPosted, PostedDate, PostedBy, IsAdjusted, AdjustmentEntryNo, FileAttachment, Notes,
          CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted, BaseCurrencyId, EnteredCurrencyId, ExchangeRate )
        VALUES
        ( @OrganizationId, @EntryNo, @CurDate, 'MANUAL', 'Margin on Sales - HWPI ' + CONVERT(VARCHAR(10), @CurDate, 120), 'J-MARGIN-' + CONVERT(VARCHAR(8), @CurDate, 112), 'JOURNAL', NULL, @PartyId, @LocationId,
          @MarginAmount, @MarginAmount, 0, NULL, 1, GETUTCDATE(), @UserId, 0, NULL, NULL, 'HWPI migration - Margin on Sales',
          @UserId, GETUTCDATE(), 'Migrate_HWPI_To_Sale_Invoices_FULL.sql', @UserId, GETUTCDATE(), NULL, 0, @BaseCurrencyId, @EnteredCurrencyId, @ExchangeRate );
        SET @HeaderId = SCOPE_IDENTITY();
        INSERT INTO GeneralLedgerDetail ( HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted )
        VALUES ( @HeaderId, 30, 'OPERATING EXPENSE - MARGIN ALLOCATION', @MarginAmount, 0, @PartyId, 1, @UserId, GETUTCDATE(), NULL, @UserId, GETUTCDATE(), NULL, 0 );
        INSERT INTO GeneralLedgerDetail ( HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted )
        VALUES ( @HeaderId, 74, 'MARGIN ON SALES - HWPI ' + CONVERT(VARCHAR(10), @CurDate, 120), 0, @MarginAmount, @PartyId, 2, @UserId, GETUTCDATE(), NULL, @UserId, GETUTCDATE(), NULL, 0 );
    END

    -- Journal: Tax on Margin (Dr 30, Cr 85). LocationId = 1.
    IF @TaxAmount > 0
    BEGIN
        SET @GLSeq = @GLSeq + 1;
        SET @EntryNo = 'GL' + RIGHT('000000' + CAST(@GLSeq AS VARCHAR(10)), 6);
        INSERT INTO GeneralLedgerHeader
        ( OrganizationId, EntryNo, EntryDate, Source, Description, ReferenceNo, ReferenceType, ReferenceId, PartyId, LocationId,
          TotalDebit, TotalCredit, IsReversed, ReversedEntryNo, IsPosted, PostedDate, PostedBy, IsAdjusted, AdjustmentEntryNo, FileAttachment, Notes,
          CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted, BaseCurrencyId, EnteredCurrencyId, ExchangeRate )
        VALUES
        ( @OrganizationId, @EntryNo, @CurDate, 'MANUAL', 'Tax on Margin - HWPI ' + CONVERT(VARCHAR(10), @CurDate, 120), 'J-TAX-' + CONVERT(VARCHAR(8), @CurDate, 112), 'JOURNAL', NULL, @PartyId, @LocationId,
          @TaxAmount, @TaxAmount, 0, NULL, 1, GETUTCDATE(), @UserId, 0, NULL, NULL, 'HWPI migration - Tax on Margin',
          @UserId, GETUTCDATE(), 'Migrate_HWPI_To_Sale_Invoices_FULL.sql', @UserId, GETUTCDATE(), NULL, 0, @BaseCurrencyId, @EnteredCurrencyId, @ExchangeRate );
        SET @HeaderId = SCOPE_IDENTITY();
        INSERT INTO GeneralLedgerDetail ( HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted )
        VALUES ( @HeaderId, 30, 'OPERATING EXPENSE - TAX ON MARGIN ALLOCATION', @TaxAmount, 0, @PartyId, 1, @UserId, GETUTCDATE(), NULL, @UserId, GETUTCDATE(), NULL, 0 );
        INSERT INTO GeneralLedgerDetail ( HeaderId, AccountId, Description, DebitAmount, CreditAmount, PartyId, SeqNo, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted )
        VALUES ( @HeaderId, 85, 'TAX ON MARGIN - HWPI ' + CONVERT(VARCHAR(10), @CurDate, 120), 0, @TaxAmount, @PartyId, 2, @UserId, GETUTCDATE(), NULL, @UserId, GETUTCDATE(), NULL, 0 );
    END

    FETCH NEXT FROM curDays INTO @CurDate, @GrossTotal, @MarginAmount, @TaxAmount;
END

CLOSE curDays;
DEALLOCATE curDays;

PRINT 'FULL migration complete: Sale invoices (Gross Revenue + payments) + GL journals (Margin, Tax). LocationId = 1 used.';
PRINT 'Payment accounts resolved from ChartOfAccounts.InterfaceType = ''PAYMENT METHOD'' (name match).';
PRINT 'Post each invoice to GL via API: CreateGLFromInvoice/{InvoiceId}';

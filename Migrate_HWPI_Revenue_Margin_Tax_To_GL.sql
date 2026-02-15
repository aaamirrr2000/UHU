-- =============================================
-- Migrate Gross Revenue, Margin on Sales, Tax on Margin to General Ledger
-- Run on SQL Server. Uses GeneralLedgerHeader + GeneralLedgerDetail.
-- Chart of Accounts: 70=Gross Revenue, 74=Margin on Sales, 85=Tax on Margin,
--   22=Cash, 18=Bank/POS/IBFT, 26=JazzCash, 27=EasyPaisa, 30=Operating Expense, 78=Receipt clearing
-- =============================================
SET NOCOUNT ON;

DECLARE @OrganizationId INT = 1;
DECLARE @UserId INT = 1;
DECLARE @HeaderId INT;
DECLARE @EntryNo VARCHAR(50);
DECLARE @EntryDate DATE;
DECLARE @OnAccount VARCHAR(50);
DECLARE @Amount DECIMAL(18,2);
DECLARE @Method VARCHAR(50);
DECLARE @Details VARCHAR(500);
DECLARE @Seq INT = 0;
DECLARE @DebitAccountId INT;
DECLARE @CreditAccountId INT;

-- Source data: TranDate (yyyy-MM-dd), OnAccount, Amount, Method, Details
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

-- Cursor over @Data
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
SELECT Seq, TranDate, OnAccount, Amount, Method, Details FROM @Data ORDER BY Seq;

OPEN cur;

DECLARE @CurSeq INT, @CurTranDate DATE, @CurOnAccount VARCHAR(50), @CurAmount DECIMAL(18,2), @CurMethod VARCHAR(100), @CurDetails VARCHAR(500);

FETCH NEXT FROM cur INTO @CurSeq, @CurTranDate, @CurOnAccount, @CurAmount, @CurMethod, @CurDetails;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF @CurAmount = 0 AND @CurOnAccount = 'Gross Revenue' AND @CurMethod IN ('Cash','Jazz QR Code','POS','Bank - UBL')
    BEGIN
        FETCH NEXT FROM cur INTO @CurSeq, @CurTranDate, @CurOnAccount, @CurAmount, @CurMethod, @CurDetails;
        CONTINUE; -- skip zero-amount gross revenue
    END

    SET @EntryNo = 'GL-HWPI-' + CAST(@CurSeq AS VARCHAR(20));

    -- Debit/Credit account by type and method
    IF @CurOnAccount = 'Gross Revenue'
    BEGIN
        SET @DebitAccountId = CASE
            WHEN @CurMethod IN ('Cash') THEN 22
            WHEN @CurMethod IN ('IBFT','Bank - UBL','DD/PO/Cheque') THEN 18
            WHEN @CurMethod IN ('Jazz QR Code') THEN 26
            WHEN @CurMethod IN ('EasyPaisa') THEN 27
            WHEN @CurMethod IN ('POS') THEN 22
            ELSE 22
        END;
        SET @CreditAccountId = 70; -- Gross Revenue
    END
    ELSE IF @CurOnAccount = 'Marin on Sales'
    BEGIN
        SET @DebitAccountId = 78; -- Receipt clearing
        SET @CreditAccountId = 74; -- Margin on Sales
    END
    ELSE IF @CurOnAccount = 'Tax on Margin'
    BEGIN
        SET @DebitAccountId = 30; -- Operating Expense
        SET @CreditAccountId = 85; -- Tax on Margin (liability)
    END
    ELSE
    BEGIN
        FETCH NEXT FROM cur INTO @CurSeq, @CurTranDate, @CurOnAccount, @CurAmount, @CurMethod, @CurDetails;
        CONTINUE;
    END

    INSERT INTO dbo.GeneralLedgerHeader (
        OrganizationId, EntryNo, EntryDate, Source, Description, ReferenceNo,
        TotalDebit, TotalCredit, IsPosted, PostedDate, PostedBy,
        BaseCurrencyId, EnteredCurrencyId, ExchangeRate,
        CreatedBy, CreatedOn, UpdatedBy, UpdatedOn, IsSoftDeleted
    )
    VALUES (
        @OrganizationId, @EntryNo, @CurTranDate, 'HEALTHWIRE',
        ISNULL(@CurDetails, @CurOnAccount), NULL,
        @CurAmount, @CurAmount, 1, GETDATE(), @UserId,
        NULL, NULL, 1.000000,
        @UserId, GETDATE(), @UserId, GETDATE(), 0
    );

    SET @HeaderId = SCOPE_IDENTITY();

    INSERT INTO dbo.GeneralLedgerDetail (HeaderId, AccountId, Description, DebitAmount, CreditAmount, SeqNo, IsSoftDeleted)
    VALUES (@HeaderId, @DebitAccountId, ISNULL(@CurDetails, @CurOnAccount), @CurAmount, 0, 1, 0);

    INSERT INTO dbo.GeneralLedgerDetail (HeaderId, AccountId, Description, DebitAmount, CreditAmount, SeqNo, IsSoftDeleted)
    VALUES (@HeaderId, @CreditAccountId, ISNULL(@CurDetails, @CurOnAccount), 0, @CurAmount, 2, 0);

    FETCH NEXT FROM cur INTO @CurSeq, @CurTranDate, @CurOnAccount, @CurAmount, @CurMethod, @CurDetails;
END

CLOSE cur;
DEALLOCATE cur;

PRINT 'Migration complete: Gross Revenue, Margin on Sales, Tax on Margin.';
GO

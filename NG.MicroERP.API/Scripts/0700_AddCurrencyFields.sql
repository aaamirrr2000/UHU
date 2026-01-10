-- =============================================
-- Add Currency Fields to Transaction Tables
-- =============================================

-- Add Currency Fields to Invoice Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Invoice') AND name = 'BaseCurrencyId')
BEGIN
    ALTER TABLE dbo.Invoice
    ADD BaseCurrencyId INT NULL,
        EnteredCurrencyId INT NULL,
        ExchangeRate DECIMAL(18, 6) DEFAULT 1.000000;
    
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Currencies' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        ALTER TABLE dbo.Invoice
        ADD CONSTRAINT FK_Invoice_BaseCurrency FOREIGN KEY (BaseCurrencyId) REFERENCES dbo.Currencies(Id),
            CONSTRAINT FK_Invoice_EnteredCurrency FOREIGN KEY (EnteredCurrencyId) REFERENCES dbo.Currencies(Id);
    END
END
GO

-- Add Currency Fields to Cashbook Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Cashbook') AND name = 'BaseCurrencyId')
BEGIN
    ALTER TABLE dbo.Cashbook
    ADD BaseCurrencyId INT NULL,
        EnteredCurrencyId INT NULL,
        ExchangeRate DECIMAL(18, 6) DEFAULT 1.000000;
    
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Currencies' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        ALTER TABLE dbo.Cashbook
        ADD CONSTRAINT FK_Cashbook_BaseCurrency FOREIGN KEY (BaseCurrencyId) REFERENCES dbo.Currencies(Id),
            CONSTRAINT FK_Cashbook_EnteredCurrency FOREIGN KEY (EnteredCurrencyId) REFERENCES dbo.Currencies(Id);
    END
END
GO

-- Add Currency Fields to GeneralLedgerHeader Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.GeneralLedgerHeader') AND name = 'BaseCurrencyId')
BEGIN
    ALTER TABLE dbo.GeneralLedgerHeader
    ADD BaseCurrencyId INT NULL,
        EnteredCurrencyId INT NULL;
    
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Currencies' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        ALTER TABLE dbo.GeneralLedgerHeader
        ADD CONSTRAINT FK_GeneralLedgerHeader_BaseCurrency FOREIGN KEY (BaseCurrencyId) REFERENCES dbo.Currencies(Id),
            CONSTRAINT FK_GeneralLedgerHeader_EnteredCurrency FOREIGN KEY (EnteredCurrencyId) REFERENCES dbo.Currencies(Id);
    END
END
GO

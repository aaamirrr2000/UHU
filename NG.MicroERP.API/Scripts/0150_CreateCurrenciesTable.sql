CREATE TABLE Currencies
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Code            VARCHAR(10) NOT NULL UNIQUE,      -- e.g. PKR, USD, EUR
    Name            VARCHAR(100) NOT NULL,            -- e.g. Pakistani Rupee, US Dollar
    Symbol          VARCHAR(10) NULL,                 -- e.g. ₨, $, €
    Country         VARCHAR(100) NULL,                -- e.g. Pakistan, United States
    IsBaseCurrency  INT NOT NULL DEFAULT 0,           -- marks your system's default currency
    IsActive        INT NOT NULL DEFAULT 1,
	CreatedBy	    INT					NULL DEFAULT NULL,
	CreatedOn       DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom     VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy       INT					NULL DEFAULT NULL,
	UpdatedOn       DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom     VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted   SMALLINT			NOT NULL DEFAULT 0
);
GO

INSERT INTO Currencies (Code, Name, Symbol, Country, IsBaseCurrency)
VALUES
('PKR', 'Pakistani Rupee', '₨', 'Pakistan', 1),
('USD', 'US Dollar', '$', 'United States', 0),
('EUR', 'Euro', '€', 'European Union', 0),
('GBP', 'Pound Sterling', '£', 'United Kingdom', 0);
GO


CREATE TABLE ExchangeRates
(
    Id                 INT IDENTITY(1,1) PRIMARY KEY,

    BaseCurrencyId     INT NOT NULL,      -- e.g. PKR (system default)
    TargetCurrencyId   INT NOT NULL,      -- e.g. USD, EUR, GBP
    Rate               DECIMAL(18,6) NOT NULL,  -- e.g. 278.456000

    StartDate          DATE NOT NULL,     -- rate valid from
    EndDate            DATE NULL,         -- null = still valid

    Source             VARCHAR(100) NULL, -- e.g. "State Bank of Pakistan", "Manual Entry"
    Remarks            VARCHAR(255) NULL, -- optional notes

    -- Audit Fields
    CreatedBy          INT NULL,
    CreatedOn          DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom        VARCHAR(100) NULL,
    UpdatedBy          INT NULL,
    UpdatedOn          DATETIME NULL,
    UpdatedFrom        VARCHAR(100) NULL,
    IsSoftDeleted      INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ExchangeRates_BaseCurrency FOREIGN KEY (BaseCurrencyId) REFERENCES Currencies(Id),
    CONSTRAINT FK_ExchangeRates_TargetCurrency FOREIGN KEY (TargetCurrencyId) REFERENCES Currencies(Id)
);
GO

INSERT INTO ExchangeRates 
    (BaseCurrencyId, TargetCurrencyId, Rate, StartDate, EndDate, Source, Remarks, CreatedBy)
VALUES
    (1, 2, 278.450000, '2025-01-01', '2025-03-31', 'State Bank of Pakistan', 'Q1 official rate', 1),
    (1, 2, 282.750000, '2025-04-01', NULL, 'State Bank of Pakistan', 'Current valid rate', 1);

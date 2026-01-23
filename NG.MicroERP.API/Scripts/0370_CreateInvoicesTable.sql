CREATE TABLE Invoice
(
    Id                      INT              PRIMARY KEY IDENTITY(1,1),
    Guid                    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    OrganizationId          INT             NOT NULL,
    Code                    VARCHAR(50)      NOT NULL,
    InvoiceType             VARCHAR(50)      NOT NULL DEFAULT 'SALE',
    Source                  VARCHAR(50)      NOT NULL DEFAULT 'MANUAL',
    SalesId                 INT              NULL,
    LocationId              INT              NULL DEFAULT NULL,
    AccountId               INT              NULL,
    PartyId                 INT              NOT NULL,
    PartyName               VARCHAR(50)      NULL DEFAULT NULL,
    PartyPhone              VARCHAR(50)      NULL DEFAULT NULL,
    PartyEmail              VARCHAR(50)      NULL DEFAULT NULL,
    PartyAddress            VARCHAR(255)     NULL DEFAULT NULL,
    TranDate                DATETIME         NULL DEFAULT NULL,
    Description             VARCHAR(255)     NOT NULL,
    Status                  VARCHAR(50)      NULL,
    ClientComments          VARCHAR(255)     NULL DEFAULT NULL,
    Rating                  INT              NULL,
    IsPostedToGL            SMALLINT DEFAULT 0,
    PostedToGLDate          DATETIME NULL,
    PostedToGLBy            INT NULL,
    GLEntryNo               VARCHAR(50) NULL,
	CreatedBy               INT              NULL DEFAULT NULL,
	CreatedOn               DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom             VARCHAR(255)     NULL DEFAULT NULL,
	UpdatedBy               INT              NULL DEFAULT NULL,
	UpdatedOn               DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom             VARCHAR(255)     NULL DEFAULT NULL,
    IsSoftDeleted           SMALLINT         NOT NULL DEFAULT 0,
    BaseCurrencyId          INT              NULL,
    EnteredCurrencyId        INT              NULL,
    ExchangeRate             DECIMAL(18, 6)   NULL DEFAULT 1.000000,
    RowVersion              ROWVERSION,
    FOREIGN KEY (SalesId)           REFERENCES Employees(Id),
    FOREIGN KEY (LocationId)        REFERENCES Locations(Id),
    FOREIGN KEY (CreatedBy)         REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)         REFERENCES Users(Id),
    FOREIGN KEY (PartyId)           REFERENCES Parties(Id),
    FOREIGN KEY (OrganizationId)    REFERENCES Organizations(Id),
    FOREIGN KEY (AccountId) REFERENCES dbo.ChartOfAccounts(Id),
    FOREIGN KEY (BaseCurrencyId)   REFERENCES Currencies(Id),
    FOREIGN KEY (EnteredCurrencyId) REFERENCES Currencies(Id),
    FOREIGN KEY (PostedToGLBy) REFERENCES dbo.Users(Id)
);


CREATE TABLE InvoiceDetail
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    ItemId              INT              NULL,            
    StockCondition      VARCHAR(20)      NULL DEFAULT NULL,
    ManualItem          VARCHAR(255)     NULL DEFAULT NULL, 
    AccountId           INT              NULL,
    ServingSize         VARCHAR(50)      NULL DEFAULT NULL,
    Qty                 DECIMAL(16, 4)   NULL DEFAULT 0,
    UnitPrice           DECIMAL(16, 4)   NULL DEFAULT 0,
    DiscountAmount      DECIMAL(10, 2)   NULL DEFAULT 0.00,
    InvoiceId           INT              NOT NULL,
    Description         VARCHAR(255)     NOT NULL,
    Status              VARCHAR(50)      NULL,
    Rating              INT              NULL,    
    TranDate            DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,
    FOREIGN KEY (InvoiceId)        REFERENCES Invoice(Id),
    FOREIGN KEY (ItemId)        REFERENCES Items(Id),
    FOREIGN KEY (AccountId) REFERENCES dbo.ChartOfAccounts(Id),
);

CREATE TABLE InvoiceDetailTax
(
    Id                 INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceDetailId    INT NOT NULL,
    TaxId              INT NOT NULL,                  -- FK to TaxMaster.Id
    TaxRate            DECIMAL(18,4) NOT NULL,
    TaxableAmount      DECIMAL(18,4) NOT NULL,
    TaxAmount          DECIMAL(18,4) NOT NULL,
    CalculationOrder   INT NOT NULL DEFAULT 1,        -- order of tax application
    RoundingRule       VARCHAR(20) NULL,             -- optional, override from TaxMaster
    FOREIGN KEY (InvoiceDetailId) REFERENCES InvoiceDetail(Id),
    FOREIGN KEY (TaxId) REFERENCES TaxMaster(Id)
);

CREATE TABLE InvoiceCharges
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId       INT NOT NULL,
    RulesId         INT NOT NULL,
    AccountId       INT NOT NULL,
    ChargeCategory  VARCHAR(50) NOT NULL,
    AmountType      VARCHAR(50) NOT NULL CHECK (AmountType IN ('FLAT','PERCENTAGE')),
    Amount          DECIMAL(18,4) NOT NULL,        -- rule value at time
    AppliedAmount   DECIMAL(18,4) NOT NULL,        -- FINAL calculated value
    IsSoftDeleted   SMALLINT NOT NULL DEFAULT 0,
    FOREIGN KEY (InvoiceId) REFERENCES Invoice(Id),
    FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(Id),
    FOREIGN KEY (RulesId) REFERENCES InvoiceChargesRules(Id)
);


CREATE TABLE InvoicePayments
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId       INT NOT NULL,
    AccountId       INT NOT NULL,
    PaymentRef      VARCHAR(100) NULL, 
    Amount          DECIMAL(16, 2) NOT NULL,
    PaidOn          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Notes           VARCHAR(255) NULL,
    IsSoftDeleted   SMALLINT NOT NULL DEFAULT 0,
    FOREIGN KEY (InvoiceId)    REFERENCES Invoice(Id),
    FOREIGN KEY (AccountId)    REFERENCES ChartOfAccounts(Id)
);
CREATE TABLE Bill
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    OrganizationId      INT             NOT NULL,
    SeqNo               VARCHAR(50)      NOT NULL,
    BillType            VARCHAR(50)      NOT NULL DEFAULT 'SALE',
    Source              VARCHAR(50)      NOT NULL DEFAULT 'MANUAL',
    SalesId             INT              NULL,
    LocationId          INT              NULL DEFAULT NULL,
    PartyId             INT              NOT NULL,
    PartyName           VARCHAR(50)      NULL DEFAULT NULL,
    PartyPhone          VARCHAR(50)      NULL DEFAULT NULL,
    PartyEmail          VARCHAR(50)      NULL DEFAULT NULL,
    PartyAddress        VARCHAR(255)     NULL DEFAULT NULL,
    TableId             INT              NULL,
    TranDate            DATETIME         NULL DEFAULT NULL,
    DiscountAmount      DECIMAL(16, 2)   NULL DEFAULT 0.00,
    SubTotalAmount      DECIMAL(16, 2)   NULL DEFAULT 0.00,
    TotalChargeAmount   DECIMAL(16, 2)   NULL DEFAULT 0.00,
    BillAmount          DECIMAL(16, 2)   NULL DEFAULT 0.00,
    TotalPaidAmount     DECIMAL(16, 2)   NULL DEFAULT 0.00,
    Description         VARCHAR(255)     NOT NULL,
    Status              VARCHAR(50)      NULL,
    ServiceType         VARCHAR(50)      NULL,
    PreprationTime      INT,
    ClientComments      VARCHAR(255)     NULL DEFAULT NULL,
    Rating              INT              NULL,
	CreatedBy           INT              NULL DEFAULT NULL,
	CreatedOn           DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
	UpdatedBy           INT              NULL DEFAULT NULL,
	UpdatedOn           DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,
    RowVersion          ROWVERSION,
    FOREIGN KEY (SalesId)   REFERENCES Employees(Id),
    FOREIGN KEY (TableId)   REFERENCES RestaurantTables(Id),
    FOREIGN KEY (LocationId)   REFERENCES Locations(Id),
    FOREIGN KEY (CreatedBy)    REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)    REFERENCES Users(Id),
    FOREIGN KEY (PartyId)      REFERENCES Parties(Id),
    FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
);

CREATE TABLE BillDetail
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ItemId              INT              NOT NULL,
    StockCondition      VARCHAR(20)      NULL DEFAULT NULL,
    ServingSize         VARCHAR(50)      NULL DEFAULT NULL,
    Qty                 DECIMAL(16, 4)   NULL DEFAULT 0,
    UnitPrice           DECIMAL          NULL DEFAULT 0,
    DiscountAmount      DECIMAL(10, 2)   NULL DEFAULT 0.00,
    TaxAmount           DECIMAL(10, 2)   NULL DEFAULT 0.00,
    BillId              INT              NOT NULL,
    Description         VARCHAR(255)     NOT NULL,
    Status              VARCHAR(50)      NULL,
    Person              INT              DEFAULT 0,
    Rating              INT              NULL,    
    TranDate            DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,
    RowVersion          ROWVERSION,
    FOREIGN KEY (BillId)       REFERENCES Bill(Id),
    FOREIGN KEY (ItemId)       REFERENCES Items(Id)
);

CREATE TABLE BillCharges
(
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    BillId              INT             NOT NULL,
    ChargeRuleId        INT             NOT NULL,  
    RuleName            NVARCHAR(100)   NOT NULL,
    RuleType            VARCHAR(20)     NOT NULL CHECK (RuleType IN ('CHARGE', 'DISCOUNT')),
    AmountType          VARCHAR(20)     NOT NULL CHECK (AmountType IN ('FLAT', 'PERCENTAGE')),
    Rate                DECIMAL(18,4)   NOT NULL,
    CalculatedAmount    DECIMAL(18,4)   NOT NULL,
    SequenceOrder       INT             NOT NULL DEFAULT 0,
    CalculationBase     VARCHAR(50)     NOT NULL CHECK (CalculationBase IN ('BILLED_AMOUNT', 'AFTER_PREVIOUS_CHARGES')),

    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,
    RowVersion          ROWVERSION,
    FOREIGN KEY (BillId) REFERENCES Bill(Id),
);

CREATE TABLE BillPayments
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    BillId          INT NOT NULL,
    PaymentMethod   VARCHAR(50) NOT NULL,
    PaymentRef      VARCHAR(100) NULL, 
    AmountPaid      DECIMAL(16, 2) NOT NULL,
    PaidOn          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Notes           VARCHAR(255) NULL,
    IsSoftDeleted   SMALLINT NOT NULL DEFAULT 0,
    RowVersion      ROWVERSION,

    FOREIGN KEY (BillId) REFERENCES Bill(Id)
);
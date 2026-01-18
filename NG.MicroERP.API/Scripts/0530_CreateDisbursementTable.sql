CREATE TABLE Disbursement
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT              NOT NULL,
    Code                VARCHAR(50)      NOT NULL,
    EmployeeId          INT              NOT NULL,
    DepartmentId        INT,
    DisbursementDate    DATE             NOT NULL,
    DisbursementType    VARCHAR(30),     -- ADVANCE, LOAN, BONUS, ALLOWANCE, OVERTIME, OTHER
    Amount              DECIMAL(18, 2)   NOT NULL,
    Currency            VARCHAR(10),
    ExchangeRate        DECIMAL(18, 4)   DEFAULT 1.0,
    AmountInBaseCurrency DECIMAL(18, 2)  NOT NULL,
    PaymentMethod       VARCHAR(20),     -- CASH, BANK_TRANSFER, CHEQUE
    BankAccountId       INT,
    BankAccountNumber   VARCHAR(50),
    ChequeNumber        VARCHAR(50),
    ChequeDate          DATE,
    ReferenceNo         VARCHAR(50),
    Description         VARCHAR(500),
    Status              VARCHAR(20)      NOT NULL DEFAULT 'PENDING',  -- PENDING, APPROVED, PAID, CANCELLED
    ApprovedBy          INT,
    ApprovedDate        DATE,
    PaidBy              INT,
    PaidDate            DATE,
    Remarks             VARCHAR(1000),
    CreatedBy           INT              NULL DEFAULT NULL,
    CreatedOn           DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    UpdatedBy           INT              NULL DEFAULT NULL,
    UpdatedOn           DATETIME         NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,

    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    FOREIGN KEY (EmployeeId)     REFERENCES Employees(Id),
    FOREIGN KEY (DepartmentId)   REFERENCES Departments(Id),
    FOREIGN KEY (BankAccountId)  REFERENCES ChartOfAccounts(Id),
    FOREIGN KEY (ApprovedBy)     REFERENCES Users(Id),
    FOREIGN KEY (PaidBy)         REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy)      REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)      REFERENCES Users(Id)
);
GO

CREATE UNIQUE INDEX IX_Disbursement_Code ON Disbursement(Code) WHERE IsSoftDeleted = 0;
CREATE INDEX IX_Disbursement_EmployeeId ON Disbursement(EmployeeId);
CREATE INDEX IX_Disbursement_DisbursementDate ON Disbursement(DisbursementDate);
CREATE INDEX IX_Disbursement_Status ON Disbursement(Status);
GO

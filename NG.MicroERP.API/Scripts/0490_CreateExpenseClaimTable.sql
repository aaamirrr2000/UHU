CREATE TABLE ExpenseClaim
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT              NOT NULL,
    Code                VARCHAR(50)      NOT NULL,
    EmployeeId          INT              NOT NULL,
    DepartmentId        INT,
    ClaimDate           DATE             NOT NULL,
    ExpenseDate         DATE,
    ExpenseType         VARCHAR(30),     -- TRAVEL, MEAL, ACCOMMODATION, TRANSPORT, MISCELLANEOUS
    Category            VARCHAR(50),
    Description         VARCHAR(500),
    Amount              DECIMAL(18, 2)   NOT NULL,
    Currency            VARCHAR(10),
    ExchangeRate        DECIMAL(18, 4)   DEFAULT 1.0,
    AmountInBaseCurrency DECIMAL(18, 2)  NOT NULL,
    PaymentMethod       VARCHAR(20),     -- CASH, BANK_TRANSFER, COMPANY_ACCOUNT
    ReceiptNumber       VARCHAR(50),
    ReceiptAttachment   VARCHAR(500),
    Status              VARCHAR(20)      NOT NULL DEFAULT 'PENDING',  -- PENDING, SUBMITTED, APPROVED, REJECTED, PAID
    ApprovedBy          INT,
    ApprovedDate        DATE,
    PaidBy              INT,
    PaidDate            DATE,
    RejectionReason     VARCHAR(500),
    Remarks             VARCHAR(1000),
    CreatedBy           INT              NULL DEFAULT NULL,
    CreatedOn           DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    UpdatedBy           INT              NULL DEFAULT NULL,
    UpdatedOn           DATETIME         NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,

    CONSTRAINT FK_ExpenseClaim_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_ExpenseClaim_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_ExpenseClaim_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_ExpenseClaim_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Users(Id),
    CONSTRAINT FK_ExpenseClaim_PaidBy FOREIGN KEY (PaidBy) REFERENCES Users(Id),
    CONSTRAINT FK_ExpenseClaim_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_ExpenseClaim_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);
GO

CREATE UNIQUE INDEX IX_ExpenseClaim_Code ON ExpenseClaim(Code) WHERE IsSoftDeleted = 0;
CREATE INDEX IX_ExpenseClaim_EmployeeId ON ExpenseClaim(EmployeeId);
CREATE INDEX IX_ExpenseClaim_ClaimDate ON ExpenseClaim(ClaimDate);
CREATE INDEX IX_ExpenseClaim_Status ON ExpenseClaim(Status);
GO

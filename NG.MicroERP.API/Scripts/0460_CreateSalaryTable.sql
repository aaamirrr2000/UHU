CREATE TABLE Salary
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT              NOT NULL,
    Code                VARCHAR(50)      NOT NULL,
    SalaryMonth         VARCHAR(10)      NOT NULL,  -- Format: YYYY-MM
    EmployeeId          INT              NOT NULL,
    DepartmentId        INT,
    DesignationId       INT,
    PayDate             DATE,
    BasicSalary         DECIMAL(18, 2)   NOT NULL DEFAULT 0,
    Allowances          DECIMAL(18, 2)   DEFAULT 0,
    Bonuses             DECIMAL(18, 2)   DEFAULT 0,
    Overtime            DECIMAL(18, 2)   DEFAULT 0,
    GrossSalary         DECIMAL(18, 2)   NOT NULL DEFAULT 0,
    Tax                 DECIMAL(18, 2)   DEFAULT 0,
    ProvidentFund       DECIMAL(18, 2)   DEFAULT 0,
    Insurance           DECIMAL(18, 2)   DEFAULT 0,
    Loans               DECIMAL(18, 2)   DEFAULT 0,
    Deductions          DECIMAL(18, 2)   DEFAULT 0,
    TotalDeductions     DECIMAL(18, 2)   NOT NULL DEFAULT 0,
    NetSalary           DECIMAL(18, 2)   NOT NULL DEFAULT 0,
    PaymentMethod       VARCHAR(20),     -- CASH, BANK_TRANSFER, CHEQUE
    BankAccountId       INT,
    BankAccountNumber   VARCHAR(50),
    ChequeNumber        VARCHAR(50),
    ChequeDate          DATE,
    Status              VARCHAR(20)      NOT NULL DEFAULT 'PENDING',  -- PENDING, APPROVED, PAID, CANCELLED
    Notes               VARCHAR(1000),
    CreatedBy           INT              NULL DEFAULT NULL,
    CreatedOn           DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    UpdatedBy           INT              NULL DEFAULT NULL,
    UpdatedOn           DATETIME         NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,

    CONSTRAINT FK_Salary_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Salary_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_Salary_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_Salary_Designation FOREIGN KEY (DesignationId) REFERENCES Designations(Id),
    CONSTRAINT FK_Salary_BankAccount FOREIGN KEY (BankAccountId) REFERENCES ChartOfAccounts(Id),
    CONSTRAINT FK_Salary_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Salary_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);
GO

CREATE UNIQUE INDEX IX_Salary_Code ON Salary(Code) WHERE IsSoftDeleted = 0;
CREATE UNIQUE INDEX IX_Salary_EmployeeMonth ON Salary(EmployeeId, SalaryMonth) WHERE IsSoftDeleted = 0;
CREATE INDEX IX_Salary_SalaryMonth ON Salary(SalaryMonth);
CREATE INDEX IX_Salary_Status ON Salary(Status);
GO

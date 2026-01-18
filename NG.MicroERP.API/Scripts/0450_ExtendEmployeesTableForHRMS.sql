-- Extend Employees table with HRMS-specific fields
-- This script adds additional HRMS fields to the existing Employees table

ALTER TABLE Employees
ADD FirstName               VARCHAR(100)     NULL,
    LastName                VARCHAR(100)     NULL,
    Gender                  VARCHAR(10)      NULL,
    MaritalStatus           VARCHAR(20)      NULL,
    DateOfBirth             DATE             NULL,
    Mobile                  VARCHAR(20)      NULL,
    City                    VARCHAR(100)     NULL,
    Country                 VARCHAR(100)     NULL,
    PostalCode              VARCHAR(20)      NULL,
    HireDate                DATE             NULL,
    TerminationDate         DATE             NULL,
    BasicSalary             DECIMAL(18, 2)   DEFAULT 0,
    BankAccountId           INT              NULL,
    BankAccountNumber       VARCHAR(50)      NULL,
    EmergencyContactName    VARCHAR(100)     NULL,
    EmergencyContactPhone   VARCHAR(20)      NULL,
    EmergencyContactRelation VARCHAR(50)     NULL,
    Notes                   VARCHAR(1000)    NULL;
GO

-- Add foreign key for BankAccountId if ChartOfAccounts table exists
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ChartOfAccounts')
BEGIN
    ALTER TABLE Employees
    ADD CONSTRAINT FK_Employees_BankAccount FOREIGN KEY (BankAccountId) REFERENCES ChartOfAccounts(Id);
END
GO

-- Create index on HireDate for reporting
CREATE INDEX IX_Employees_HireDate ON Employees(HireDate);
GO

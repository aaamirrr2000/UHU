CREATE TABLE Termination
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT              NOT NULL,
    Code                VARCHAR(50)      NOT NULL,
    EmployeeId          INT              NOT NULL,
    DepartmentId        INT,
    TerminationDate     DATE             NOT NULL,
    LastWorkingDate     DATE             NOT NULL,
    TerminationType     VARCHAR(20),     -- RESIGNATION, TERMINATION, RETIREMENT, END_OF_CONTRACT
    TerminationReason   VARCHAR(500),
    NoticePeriod        VARCHAR(50),
    SeverancePay        DECIMAL(18, 2)   DEFAULT 0,
    OutstandingSalary   DECIMAL(18, 2)   DEFAULT 0,
    OutstandingLeaves   DECIMAL(18, 2)   DEFAULT 0,
    FinalSettlement     DECIMAL(18, 2)   DEFAULT 0,
    Status              VARCHAR(20)      NOT NULL DEFAULT 'PENDING',  -- PENDING, APPROVED, PROCESSED
    ApprovedBy          INT,
    ApprovedDate        DATE,
    HandoverNotes       VARCHAR(1000),
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
    FOREIGN KEY (ApprovedBy)     REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy)      REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)      REFERENCES Users(Id)
);
GO

CREATE UNIQUE INDEX IX_Termination_Code ON Termination(Code) WHERE IsSoftDeleted = 0;
CREATE INDEX IX_Termination_EmployeeId ON Termination(EmployeeId);
CREATE INDEX IX_Termination_TerminationDate ON Termination(TerminationDate);
CREATE INDEX IX_Termination_Status ON Termination(Status);
GO

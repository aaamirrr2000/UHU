CREATE TABLE DepartmentChange
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT              NOT NULL,
    Code                VARCHAR(50)      NOT NULL,
    EmployeeId          INT              NOT NULL,
    ChangeDate          DATE             NOT NULL,
    OldDepartmentId     INT              NOT NULL,
    NewDepartmentId     INT              NOT NULL,
    ChangeType          VARCHAR(30),     -- TRANSFER, RESTRUCTURE, REORGANIZATION
    Reason              VARCHAR(500),
    Remarks             VARCHAR(1000),
    Status              VARCHAR(20)      NOT NULL DEFAULT 'PENDING',  -- PENDING, APPROVED, EFFECTED
    ApprovedBy          INT,
    ApprovedDate        DATE,
    CreatedBy           INT              NULL DEFAULT NULL,
    CreatedOn           DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    UpdatedBy           INT              NULL DEFAULT NULL,
    UpdatedOn           DATETIME         NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,

    CONSTRAINT FK_DepartmentChange_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_DepartmentChange_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_DepartmentChange_OldDepartment FOREIGN KEY (OldDepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_DepartmentChange_NewDepartment FOREIGN KEY (NewDepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_DepartmentChange_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Users(Id),
    CONSTRAINT FK_DepartmentChange_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_DepartmentChange_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);
GO

CREATE UNIQUE INDEX IX_DepartmentChange_Code ON DepartmentChange(Code) WHERE IsSoftDeleted = 0;
CREATE INDEX IX_DepartmentChange_EmployeeId ON DepartmentChange(EmployeeId);
CREATE INDEX IX_DepartmentChange_ChangeDate ON DepartmentChange(ChangeDate);
CREATE INDEX IX_DepartmentChange_Status ON DepartmentChange(Status);
GO

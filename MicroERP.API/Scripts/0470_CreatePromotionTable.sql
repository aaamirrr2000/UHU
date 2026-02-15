CREATE TABLE Promotion
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT              NOT NULL,
    Code                VARCHAR(50)      NOT NULL,
    EmployeeId          INT              NOT NULL,
    PromotionDate       DATE             NOT NULL,
    OldDepartmentId     INT,
    NewDepartmentId     INT,
    OldDesignationId    INT,
    NewDesignationId    INT,
    OldBasicSalary      DECIMAL(18, 2)   DEFAULT 0,
    NewBasicSalary      DECIMAL(18, 2)   DEFAULT 0,
    PromotionType       VARCHAR(20),     -- PROMOTION, DEMOTION, TRANSFER
    Reason              VARCHAR(500),
    Remarks             VARCHAR(1000),
    Status              VARCHAR(20)      NOT NULL DEFAULT 'PENDING',  -- PENDING, APPROVED, REJECTED
    ApprovedBy          INT,
    ApprovedDate        DATE,
    CreatedBy           INT              NULL DEFAULT NULL,
    CreatedOn           DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    UpdatedBy           INT              NULL DEFAULT NULL,
    UpdatedOn           DATETIME         NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,

    CONSTRAINT FK_Promotion_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Promotion_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_Promotion_OldDepartment FOREIGN KEY (OldDepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_Promotion_NewDepartment FOREIGN KEY (NewDepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_Promotion_OldDesignation FOREIGN KEY (OldDesignationId) REFERENCES Designations(Id),
    CONSTRAINT FK_Promotion_NewDesignation FOREIGN KEY (NewDesignationId) REFERENCES Designations(Id),
    CONSTRAINT FK_Promotion_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Promotion_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Promotion_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);
GO

CREATE UNIQUE INDEX IX_Promotion_Code ON Promotion(Code) WHERE IsSoftDeleted = 0;
CREATE INDEX IX_Promotion_EmployeeId ON Promotion(EmployeeId);
CREATE INDEX IX_Promotion_PromotionDate ON Promotion(PromotionDate);
CREATE INDEX IX_Promotion_Status ON Promotion(Status);
GO

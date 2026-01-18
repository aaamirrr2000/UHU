CREATE TABLE Relocation
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT              NOT NULL,
    Code                VARCHAR(50)      NOT NULL,
    EmployeeId          INT              NOT NULL,
    RelocationDate      DATE             NOT NULL,
    OldLocationId       INT              NOT NULL,
    NewLocationId       INT              NOT NULL,
    RelocationType      VARCHAR(20),     -- PERMANENT, TEMPORARY, PROJECT_BASED
    Reason              VARCHAR(500),
    RelocationAllowance DECIMAL(18, 2)   DEFAULT 0,
    TravelExpenses      DECIMAL(18, 2)   DEFAULT 0,
    AccommodationAllowance DECIMAL(18, 2) DEFAULT 0,
    TotalRelocationCost DECIMAL(18, 2)   DEFAULT 0,
    Status              VARCHAR(20)      NOT NULL DEFAULT 'PENDING',  -- PENDING, APPROVED, PROCESSED, COMPLETED
    ApprovedBy          INT,
    ApprovedDate        DATE,
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
    FOREIGN KEY (OldLocationId)  REFERENCES Locations(Id),
    FOREIGN KEY (NewLocationId)  REFERENCES Locations(Id),
    FOREIGN KEY (ApprovedBy)     REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy)      REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)      REFERENCES Users(Id)
);
GO

CREATE UNIQUE INDEX IX_Relocation_Code ON Relocation(Code) WHERE IsSoftDeleted = 0;
CREATE INDEX IX_Relocation_EmployeeId ON Relocation(EmployeeId);
CREATE INDEX IX_Relocation_RelocationDate ON Relocation(RelocationDate);
CREATE INDEX IX_Relocation_Status ON Relocation(Status);
GO

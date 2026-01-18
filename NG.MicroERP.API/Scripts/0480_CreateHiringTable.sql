CREATE TABLE Hiring
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT              NOT NULL,
    Code                VARCHAR(50)      NOT NULL,
    JobTitle            VARCHAR(100),
    DepartmentId        INT,
    DesignationId       INT,
    LocationId          INT,
    CandidateName       VARCHAR(100)     NOT NULL,
    CandidateCNIC       VARCHAR(20),
    CandidateEmail      VARCHAR(100),
    CandidatePhone      VARCHAR(20),
    CandidateAddress    VARCHAR(500),
    OfferedSalary       DECIMAL(18, 2)   DEFAULT 0,
    ApplicationDate     DATE,
    InterviewDate       DATE,
    OfferDate           DATE,
    JoiningDate         DATE,
    Status              VARCHAR(20)      NOT NULL DEFAULT 'APPLIED',  -- APPLIED, SHORTLISTED, INTERVIEWED, OFFERED, ACCEPTED, REJECTED, HIRED
    HiringType          VARCHAR(20),     -- PERMANENT, CONTRACT, TEMPORARY, INTERN
    HiredEmployeeId     INT,             -- Links to Employees table when hired
    Remarks             VARCHAR(1000),
    CreatedBy           INT              NULL DEFAULT NULL,
    CreatedOn           DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    UpdatedBy           INT              NULL DEFAULT NULL,
    UpdatedOn           DATETIME         NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)     NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,

    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    FOREIGN KEY (DepartmentId)   REFERENCES Departments(Id),
    FOREIGN KEY (DesignationId)  REFERENCES Designations(Id),
    FOREIGN KEY (LocationId)     REFERENCES Locations(Id),
    FOREIGN KEY (HiredEmployeeId) REFERENCES Employees(Id),
    FOREIGN KEY (CreatedBy)      REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)      REFERENCES Users(Id)
);
GO

CREATE UNIQUE INDEX IX_Hiring_Code ON Hiring(Code) WHERE IsSoftDeleted = 0;
CREATE INDEX IX_Hiring_Status ON Hiring(Status);
CREATE INDEX IX_Hiring_HiredEmployeeId ON Hiring(HiredEmployeeId) WHERE HiredEmployeeId IS NOT NULL;
GO

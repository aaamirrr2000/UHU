CREATE TABLE Roster
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NULL DEFAULT 1,
    EmployeeId          INT NOT NULL,
    ShiftId             INT NOT NULL,
    RosterDate          DATE NOT NULL,
    LocationId          INT NULL,
    Notes               VARCHAR(500) NULL,
    IsActive            SMALLINT NOT NULL DEFAULT 1,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NULL,
    UpdatedFrom         VARCHAR(255) NULL,
    IsSoftDeleted       SMALLINT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Roster_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Roster_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_Roster_Shift FOREIGN KEY (ShiftId) REFERENCES Shifts(Id),
    CONSTRAINT FK_Roster_Location FOREIGN KEY (LocationId) REFERENCES Locations(Id),
    CONSTRAINT FK_Roster_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Roster_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Roster_EmployeeId ON Roster(EmployeeId);
CREATE INDEX IX_Roster_RosterDate ON Roster(RosterDate);
CREATE INDEX IX_Roster_ShiftId ON Roster(ShiftId);
CREATE INDEX IX_Roster_LocationId ON Roster(LocationId);
CREATE UNIQUE INDEX IX_Roster_EmployeeDate ON Roster(EmployeeId, RosterDate) WHERE IsSoftDeleted = 0;

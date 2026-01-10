CREATE TABLE Employees
(
    Id                   INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId       INT NULL DEFAULT 1,
    EmpId                VARCHAR(25) NOT NULL,
    Fullname             VARCHAR(50) NOT NULL,
    Pic                  VARCHAR(2000) NULL,
    Phone                VARCHAR(20) NOT NULL,
    Email                VARCHAR(50) NULL,
    Cnic                 VARCHAR(20) NOT NULL,
    Address              VARCHAR(255) NOT NULL,
    EmpType              VARCHAR(20) NOT NULL,
    DepartmentId         INT NOT NULL,
    DesignationId        INT NOT NULL,
    ShiftId              INT NOT NULL,
    LocationId           INT NOT NULL,
    ParentId             INT NOT NULL DEFAULT 0,
    ExcludeFromAttendance INT NOT NULL DEFAULT 0,
    IsActive             INT NOT NULL DEFAULT 1,
    CreatedBy            INT NULL,
    CreatedOn            DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom          VARCHAR(255) NULL,
    UpdatedBy            INT NULL,
    UpdatedOn            DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom          VARCHAR(255) NULL,
    IsSoftDeleted        INT NULL DEFAULT 0,
    CONSTRAINT FK_Employees_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Employees_Shifts FOREIGN KEY (ShiftId) REFERENCES Shifts(Id),
    CONSTRAINT FK_Employees_Locations FOREIGN KEY (LocationId) REFERENCES Locations(Id),
    CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_Employees_Designations FOREIGN KEY (DesignationId) REFERENCES Designations(Id)
);

INSERT INTO Employees
(
    OrganizationId,
    EmpId,
    Fullname,
    Pic,
    Phone,
    Email,
    Cnic,
    Address,
    EmpType,
    DepartmentId,
    DesignationId,
    ShiftId,
    LocationId,
    ParentId,
    ExcludeFromAttendance,
    IsActive,
    CreatedBy,
    CreatedOn,
    CreatedFrom,
    UpdatedBy,
    UpdatedOn,
    UpdatedFrom,
    IsSoftDeleted
)
VALUES
(
    1,                                      -- OrganizationId
    '00001',                             -- EmpId
    'System Administrator',                 -- Fullname
    NULL,                                   -- Pic
    '0000000000',                            -- Phone
    'admin@example.com',                     -- Email
    '00000-0000000-0',                       -- Cnic
    'Head Office, MOITT',                   -- Address
    'PERMANENT',                             -- EmpType
    1,                                      -- DepartmentId (e.g., MINISTER'S OFFICE)
    1,                                      -- DesignationId (e.g., Head of Department)
    1,                                      -- ShiftId (e.g., Morning Shift)
    1,                                      -- LocationId (e.g., MOITT Head Office)
    0,                                      -- ParentId
    0,                                      -- ExcludeFromAttendance
    1,                                      -- IsActive
    1,                                      -- CreatedBy
    GETDATE(),                               -- CreatedOn
    'SYSTEM',                                -- CreatedFrom
    1,                                      -- UpdatedBy
    GETDATE(),                               -- UpdatedOn
    'SYSTEM',                                -- UpdatedFrom
    0                                       -- IsSoftDeleted
);

CREATE TABLE Departments
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,          -- Auto-increment identity
    GUID            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    OrganizationId  INT NULL DEFAULT 1,
    DepartmentName  NVARCHAR(150) NOT NULL,
    ParentId        INT NULL,                                -- For hierarchical departments
    Description     NVARCHAR(250) NULL,
    IsActive        BIT NOT NULL DEFAULT 1,                 -- Use BIT instead of INT/BOOLEAN
    CreatedBy       INT NULL,
    CreatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom     NVARCHAR(250) NULL,
    UpdatedBy       INT NULL,
    UpdatedOn       DATETIME NULL,
    UpdatedFrom     NVARCHAR(250) NULL,
    IsSoftDeleted   BIT NOT NULL DEFAULT 0,
    RowVersion      ROWVERSION NOT NULL                      -- SQL Server's versioning column
);

INSERT INTO Departments
(
    DepartmentName,
    ParentId,
    Description,
    IsActive,
    CreatedBy,
    CreatedFrom
)
VALUES
(
    'Admin',        -- Department name
    NULL,           -- Top-level department, no parent
    'Administrative Department',  -- Description
    1,              -- IsActive = true
    1,              -- CreatedBy (e.g., admin user id)
    'System'        -- CreatedFrom
);
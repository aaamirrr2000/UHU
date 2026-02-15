CREATE TABLE Departments
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT NULL DEFAULT 1,
    DepartmentName  NVARCHAR(150) NOT NULL,
    ParentId        INT NULL,                                -- For hierarchical departments
    Description     NVARCHAR(250) NULL,
    IsActive        INT NOT NULL DEFAULT 1,                 -- Use BIT instead of INT/BOOLEAN
    CreatedBy       INT NULL,
    CreatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom     NVARCHAR(250) NULL,
    UpdatedBy       INT NULL,
    UpdatedOn       DATETIME NULL,
    UpdatedFrom     NVARCHAR(250) NULL,
    IsSoftDeleted   INT NOT NULL DEFAULT 0
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
('ADMIN',        NULL, 'ADMINISTRATIVE DEPARTMENT', 1, 1, 'SYSTEM'),
('ACCOUNTS',     NULL, 'ACCOUNTS / FINANCE DEPARTMENT', 1, 1, 'SYSTEM'),
('SALES',        NULL, 'SALES AND MARKETING DEPARTMENT', 1, 1, 'SYSTEM'),
('PURCHASE',     NULL, 'PROCUREMENT / PURCHASE DEPARTMENT', 1, 1, 'SYSTEM'),
('WAREHOUSE',    NULL, 'WAREHOUSE / INVENTORY DEPARTMENT', 1, 1, 'SYSTEM'),
('PRODUCTION',   NULL, 'PRODUCTION / MANUFACTURING DEPARTMENT', 1, 1, 'SYSTEM'),
('TECHNOLOGY',   NULL, 'INFORMATION TECHNOLOGY DEPARTMENT', 1, 1, 'SYSTEM'),
('HR',           NULL, 'HUMAN RESOURCES DEPARTMENT', 1, 1, 'SYSTEM'),
('R&D',          NULL, 'RESEARCH AND DEVELOPMENT', 1, 1, 'SYSTEM');

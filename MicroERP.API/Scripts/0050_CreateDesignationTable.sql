CREATE TABLE Designations
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT NULL DEFAULT 1,
    DesignationName VARCHAR(150) NOT NULL,
    ParentId        INT NULL,
    DepartmentId    INT NULL,
    Description     VARCHAR(250),
    IsActive        INT DEFAULT 1,
    CreatedBy       INT,
    CreatedOn       DATETIME DEFAULT GETDATE(),
    CreatedFrom     VARCHAR(250),
    UpdatedBy       INT,
    UpdatedOn       DATETIME NULL,
    UpdatedFrom     VARCHAR(250),
    IsSoftDeleted   INT DEFAULT 0,
    CONSTRAINT FK_Designations_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);

INSERT INTO Designations (DesignationName, DepartmentId, ParentId) VALUES
('ADMINISTRATOR', 1, NULL),
('PARTNER', 1, 1),
('CASHIER', 1, 2),
('PHARMASIST', 1, 4),
('SALESMAN', 1, 4);

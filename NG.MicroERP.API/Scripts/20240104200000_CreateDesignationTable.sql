CREATE TABLE Designations
(
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    GUID            CHAR(36) NOT NULL DEFAULT (UUID()),
    OrganizationId  INT NULL,
    DesignationName VARCHAR(150) NOT NULL,
    ParentId        INT NULL,
    DepartmentId    INT NULL,
    Description     VARCHAR(250),
    IsActive        INT DEFAULT 1,
    CreatedBy       INT,
    CreatedOn       DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(250),
    UpdatedBy       INT,
    UpdatedOn       DATETIME NULL,
    UpdatedFrom     VARCHAR(250),
    IsSoftDeleted   BOOLEAN DEFAULT FALSE,
    RowVersion      TIMESTAMP NULL,
    CONSTRAINT FK_Designations_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);

INSERT INTO Designations 
(GUID, OrganizationId, DesignationName, ParentId, DepartmentId, Description, IsActive, CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted)
VALUES
(UUID(), 1, 'Chief Executive Officer (CEO)', NULL, NULL, 'Top-level executive overseeing the organization', 1, 1, NOW(), 'System', 0),
(UUID(), 1, 'Chief Operating Officer (COO)', 1, NULL, 'Responsible for daily business operations', 1, 1, NOW(), 'System', 0),
(UUID(), 1, 'Chief Financial Officer (CFO)', 1, 2, 'Manages financial planning and risk management', 1, 1, NOW(), 'System', 0),
(UUID(), 1, 'HR Director', 1, 3, 'Leads the Human Resources Department', 1, 1, NOW(), 'System', 0),
(UUID(), 1, 'IT Manager', 2, 4, 'Oversees IT projects and infrastructure', 1, 1, NOW(), 'System', 0),
(UUID(), 1, 'Finance Manager', 3, 2, 'Leads finance and accounting functions', 1, 1, NOW(), 'System', 0),
(UUID(), 1, 'Accounts Officer', 6, 6, 'Manages daily accounting entries and ledgers', 1, 1, NOW(), 'System', 0),
(UUID(), 1, 'HR Executive', 4, 3, 'Assists in recruitment and payroll activities', 1, 1, NOW(), 'System', 0),
(UUID(), 1, 'Software Engineer', 5, 4, 'Develops and maintains applications', 1, 1, NOW(), 'System', 0),
(UUID(), 1, 'Sales Executive', 2, 5, 'Manages client relations and sales performance', 1, 1, NOW(), 'System', 0);


CREATE TABLE Departments
(
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    GUID            CHAR(36) NOT NULL DEFAULT (UUID()),
    OrganizationId  INT NULL,
    DepartmentName  VARCHAR(150) NOT NULL,
    ParentId        INT NULL,
    Description     VARCHAR(250),
    IsActive        INT DEFAULT 1,
    CreatedBy       INT,
    CreatedOn       DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(250),
    UpdatedBy       INT,
    UpdatedOn       DATETIME NULL,
    UpdatedFrom     VARCHAR(250),
    IsSoftDeleted   BOOLEAN DEFAULT FALSE,
    RowVersion      TIMESTAMP NULL
);

-- Departments INSERT statements
INSERT INTO Departments (OrganizationId, DepartmentName, ParentId, Description, IsActive) VALUES
(1, 'ADMINISTRATION SECTION', NULL, 'Administration Section', 1),
(1, 'ADDITIONAL SECRETARY OFFICE', NULL, 'Additional Secretary Office', 1),
(1, 'PROJECTS SECTION', NULL, 'Projects Section', 1),
(1, 'DEVELOPMENT SECTION', NULL, 'Development Section', 1),
(1, 'MINISTER,S OFFICE', NULL, 'Minister''s Office', 1),
(1, 'LEGAL WING', NULL, 'Legal Wing', 1),
(1, 'TELECOM SECTION', NULL, 'Telecom Section', 1),
(1, 'COORD & COUNCIL SECTION', NULL, 'Coordination & Council Section', 1),
(1, 'SPECIAL SECRETARY OFFICE', NULL, 'Special Secretary Office', 1),
(1, 'SECRETARY OFFICE', NULL, 'Secretary Office', 1),
(1, 'GENERAL SECTION', NULL, 'General Section', 1),
(1, 'TELECOM WING', NULL, 'Telecom Wing', 1),
(1, 'DDO-CASH SECTION', NULL, 'DDO-Cash Section', 1),
(1, 'FINANCE & ACCOUNTS SECTION', NULL, 'Finance & Accounts Section', 1),
(1, 'IT WING', NULL, 'IT Wing', 1),
(1, 'IC WING', NULL, 'IC Wing', 1),
(1, 'PARLIAMENTARY SECRETARY,S OFFICE', NULL, 'Parliamentary Secretary''s Office', 1),
(1, 'DT SECTION', NULL, 'DT Section', 1),
(1, 'USF-RND FUND SECRETARIAT', NULL, 'USF-RND Fund Secretariat', 1),
(1, 'PRO,S OFFICE', NULL, 'PRO''s Office', 1),
(1, 'CHIEF FINANCE AND ACCOUNTS OFFICER', NULL, 'Chief Finance and Accounts Officer', 1),
(1, 'DS (ADMIN)', NULL, 'DS (Admin)', 1),
(1, 'JS (ADMIN)', NULL, 'JS (Admin)', 1),
(1, 'DS (DEVELOPMENT)', NULL, 'DS (Development)', 1),
(1, 'JS (DEVELOPMENT)', NULL, 'JS (Development)', 1);


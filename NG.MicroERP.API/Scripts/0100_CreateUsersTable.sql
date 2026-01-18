-- CREATE TABLE for SQL Server
CREATE TABLE Users
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Username        VARCHAR(30) NOT NULL,
    [Password]      VARCHAR(100) NOT NULL,
    UserType        VARCHAR(50) NOT NULL,
    DarKLightTheme  INT DEFAULT 0,
    EmpId           INT NULL,
    GroupId         INT NULL,
    LocationId      INT NULL,
    IsActive        INT NOT NULL DEFAULT 1,
    CreatedBy       INT NULL,
    CreatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom     VARCHAR(255) NULL,
    UpdatedBy       INT NULL,
    UpdatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom     VARCHAR(255) NULL,
    IsSoftDeleted   INT NULL DEFAULT 0,
    CONSTRAINT FK_Users_Group FOREIGN KEY (GroupId) REFERENCES Groups(Id),
    CONSTRAINT FK_Users_Employee FOREIGN KEY (EmpId) REFERENCES Employees(Id)
);

INSERT INTO Users (Username, [Password], UserType, DarKLightTheme, EmpId, GroupId, LocationId, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted) VALUES 
('admin', 'LY4kZXq9FDNYuGO5xFRHFQ==', 'DESKTOP ADMIN', 0, 1, 1, 1, 1, NULL, 'System', NULL, 'System', 0),
('waiter', 'LY4kZXq9FDNYuGO5xFRHFQ==', 'WAITER', 0, 1, 1, 1, 1, NULL, 'System', NULL, 'System', 0);

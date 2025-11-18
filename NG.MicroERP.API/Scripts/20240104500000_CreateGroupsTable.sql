CREATE TABLE Groups
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT NULL DEFAULT 1,
    Name            VARCHAR(50) NOT NULL,
    Dashboard       VARCHAR(255) NOT NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedBy       INT NULL,
    CreatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom     VARCHAR(255) NULL,
    UpdatedBy       INT NULL,
    UpdatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom     VARCHAR(255) NULL,
    IsSoftDeleted   BIT NULL DEFAULT 0,
    CONSTRAINT FK_Groups_Organizations FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

INSERT INTO Groups 
(
    OrganizationId, 
    Name, 
    Dashboard, 
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
    1, 
    'ADMIN', 
    'AdminDashboardPage', 
    1, 
    NULL, 
    '2024-06-09 12:21:42', 
    NULL, 
    NULL, 
    '2024-06-09 12:21:42', 
    NULL, 
    0
),
(
    1, 
    'STANDARD', 
    'StandardDashboardPage', 
    1, 
    NULL, 
    '2024-06-09 12:21:42', 
    NULL, 
    NULL, 
    '2024-06-09 12:21:42', 
    NULL, 
    0
);

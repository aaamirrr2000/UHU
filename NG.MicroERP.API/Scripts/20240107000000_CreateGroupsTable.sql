CREATE TABLE Groups
(
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    OrganizationId      INT             NULL DEFAULT NULL,
    Name                VARCHAR(50)     NOT NULL,
    Dashboard           VARCHAR(255)    NOT NULL,
    IsActive            SMALLINT        NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT        NULL DEFAULT 0,
    RowVersion          ROWVERSION,
    FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
);

INSERT INTO Groups 
(
    Guid, 
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
    NEWID(), 
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
);

INSERT INTO Groups 
(
    Guid, 
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
    NEWID(), 
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

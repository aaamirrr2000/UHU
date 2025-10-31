CREATE TABLE Groups
(
    Id                  INT             PRIMARY KEY AUTO_INCREMENT,
    Guid                CHAR(36)        NOT NULL DEFAULT (UUID()),
    OrganizationId      INT             NULL DEFAULT NULL,
    Name                VARCHAR(50)     NOT NULL,
    Dashboard           VARCHAR(255)    NOT NULL,
    IsActive            TINYINT         NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       TINYINT         NULL DEFAULT 0,
    RowVersion          TIMESTAMP       NULL,
    FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
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
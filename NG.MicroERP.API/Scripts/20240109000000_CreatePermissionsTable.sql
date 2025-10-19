CREATE TABLE Permissions
(
    Id                  INT             PRIMARY KEY AUTO_INCREMENT,
    Guid                CHAR(36)        NOT NULL DEFAULT (UUID()),
    OrganizationId      INT             NULL DEFAULT NULL,
    GroupId             INT             NOT NULL,
    MenuId              INT             NULL DEFAULT NULL,
    Privilege           VARCHAR(20)     NULL DEFAULT NULL,
    IsActive            TINYINT         NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       TINYINT         NULL DEFAULT 0,
    RowVersion          TIMESTAMP       NULL,
    FOREIGN KEY (GroupId) REFERENCES Groups(Id),
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);
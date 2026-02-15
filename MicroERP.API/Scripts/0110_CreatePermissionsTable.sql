
CREATE TABLE Permissions
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT NULL DEFAULT 1,
    GroupId         INT NOT NULL,
    MenuId          INT NULL,
    Privilege       VARCHAR(20) NULL,
    IsActive        INT NOT NULL DEFAULT 1,
    CreatedBy       INT NULL,
    CreatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom     VARCHAR(255) NULL,
    UpdatedBy       INT NULL,
    UpdatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom     VARCHAR(255) NULL,
    IsSoftDeleted   INT NULL DEFAULT 0,
    CONSTRAINT FK_Permissions_Group FOREIGN KEY (GroupId) REFERENCES Groups(Id),
    CONSTRAINT FK_Permissions_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

CREATE TABLE Permissions
(
  Id                  INT             PRIMARY KEY IDENTITY(1,1),
  Guid                   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
  OrganizationId         INT             NULL DEFAULT NULL,
  GroupId                INT             NOT NULL,
  MenuId                 INT             NULL DEFAULT NULL,
  Privilege             VARCHAR(20)     NULL DEFAULT NULL,
  IsActive               SMALLINT        NOT NULL DEFAULT 1,
	CreatedBy           INT             NULL DEFAULT NULL,
	CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
	UpdatedBy           INT             NULL DEFAULT NULL,
	UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
  IsSoftDeleted          SMALLINT        NULL DEFAULT 0,
  RowVersion             ROWVERSION,
  FOREIGN KEY (GroupId) REFERENCES Groups(Id),
  FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
);


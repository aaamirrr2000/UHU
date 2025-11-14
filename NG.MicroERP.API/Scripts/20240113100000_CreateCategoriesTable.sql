CREATE TABLE Categories
(
	Id                  INT					PRIMARY KEY IDENTITY(1,1),
	Guid                UNIQUEIDENTIFIER	NOT NULL DEFAULT NEWID(),
	OrganizationId      INT					NOT NULL,
	Code                VARCHAR(50)			NOT NULL,
	Attribute           VARCHAR(255)		NULL,			--RAW MATERIAL, FINISHED GOODS, CATEGORY, SUB CATEGORY
	Name                VARCHAR(255)		NOT NULL,
	CategoryType        VARCHAR(50)			NOT NULL,		--Raw, Finished Goods
	ParentId            INT					NOT NULL DEFAULT 0,
	IsActive            SMALLINT			NOT NULL DEFAULT 1,
	CreatedBy			INT					NULL DEFAULT NULL,
	CreatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy           INT					NULL DEFAULT NULL,
	UpdatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT			NOT NULL DEFAULT 0,
	RowVersion          ROWVERSION,
	FOREIGN KEY (CreatedBy)        REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy)        REFERENCES Users(Id),
	FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
);

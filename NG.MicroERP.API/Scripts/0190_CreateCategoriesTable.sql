CREATE TABLE Categories
(
	Id                  INT					PRIMARY KEY IDENTITY(1,1),
	OrganizationId      INT					NOT NULL	DEFAULT 1,
	Code                VARCHAR(50)			NOT NULL,
	Attribute           VARCHAR(255)		NULL,			--RAW MATERIAL, FINISHED GOODS, CATEGORY, SUB CATEGORY
	Name                VARCHAR(255)		NOT NULL,
	CategoryType        VARCHAR(50)			NOT NULL,		--Raw, Finished Goods
	ParentId            INT					NOT NULL DEFAULT 0,
	TaxRuleId			INT					NULL,
	IsActive            SMALLINT			NOT NULL DEFAULT 1,
	CreatedBy			INT					NULL DEFAULT NULL,
	CreatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy           INT					NULL DEFAULT NULL,
	UpdatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT			NOT NULL DEFAULT 0,
	CONSTRAINT FK_Categories_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Categories_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Categories_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
	CONSTRAINT FK_Categories_TaxRule FOREIGN KEY (TaxRuleId) REFERENCES TaxRule(Id)
);

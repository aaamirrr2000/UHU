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

GO

SET IDENTITY_INSERT Categories ON;

-- Food menu categories for SwiftServe
INSERT INTO Categories (Id, Code, Attribute, Name, CategoryType, ParentId, TaxRuleId, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom)
VALUES
	(1, '000001', 'CATEGORY', 'Starters', 'FOOD', 0, 1, 1, 1, NULL, 1, NULL),
	(2, '000002', 'CATEGORY', 'Main Course', 'FOOD', 0, 1, 1, 1, NULL, 1, NULL),
	(3, '000003', 'CATEGORY', 'Desserts', 'FOOD', 0, 1, 1, 1, NULL, 1, NULL),
	(4, '000004', 'CATEGORY', 'Beverages', 'FOOD', 0, 1, 1, 1, NULL, 1, NULL),
	(5, '000005', 'CATEGORY', 'Sides', 'FOOD', 0, 1, 1, 1, NULL, 1, NULL);

SET IDENTITY_INSERT Categories OFF;
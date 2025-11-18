CREATE TABLE Bundles (
    Id                  INT PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT                 NOT NULL    DEFAULT 1,
    Code                VARCHAR(50)         NOT NULL,
    Name                VARCHAR(255)        NOT NULL,
    Description         VARCHAR(255)        NULL,
    BundleType          VARCHAR(20)         NOT NULL,     -- SALES / MANUFACTURING / BOTH
    RetailPrice         DECIMAL(10,2)       NOT NULL DEFAULT 0.00,
    IsActive            SMALLINT			NOT NULL DEFAULT 1,
	CreatedBy			INT					NULL DEFAULT NULL,
	CreatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy           INT					NULL DEFAULT NULL,
	UpdatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT			NOT NULL DEFAULT 0,
	FOREIGN KEY (CreatedBy)        REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy)        REFERENCES Users(Id),
	FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
);

CREATE TABLE BundleDetails (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    BundleId        INT NOT NULL,              
    ItemId          INT NOT NULL,              
    Quantity        DECIMAL(16,2) NOT NULL DEFAULT 1.00,
    WastePercent    DECIMAL(5,2)  NOT NULL DEFAULT 0.00,
    UnitCost        DECIMAL(16,2) NOT NULL DEFAULT 0.00,
    IsPrimaryItem   SMALLINT NOT NULL DEFAULT 0,
    FOREIGN KEY (BundleId) REFERENCES Bundles(Id),
    FOREIGN KEY (ItemId) REFERENCES Items(Id)
);

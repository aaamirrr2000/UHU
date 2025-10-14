-- Parties Table
CREATE TABLE Parties
(
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Guid				UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	OrganizationId		INT NOT NULL DEFAULT 1,
	Code				VARCHAR(50) NOT NULL,
	Pic					VARCHAR(255),
	Name				VARCHAR(100) NOT NULL,
	PartyType			VARCHAR(100) NULL,     -- CUSTOMER / SUPPLIER / BANK
	PartyTypeCode		VARCHAR(100) NULL,     -- DEALER / DISTRIBUTOR / RETAILER etc.
	ParentId			INT NULL,              -- Parent party link (optional)
	Address				VARCHAR(255) NULL,
	CityId				INT NULL,
	AccountId			INT NULL,
	Latitude			VARCHAR(30) NULL,
	Longitude			VARCHAR(30) NULL,
	Radius				INT NULL,
	IsActive            SMALLINT			NOT NULL DEFAULT 1,
	CreatedBy			INT					NULL DEFAULT NULL,
	CreatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy           INT					NULL DEFAULT NULL,
	UpdatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT			NOT NULL DEFAULT 0,
	RowVersion          ROWVERSION,
	FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
	FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(Id),
	FOREIGN KEY (CityId) REFERENCES Areas(Id)
);

-- Contacts
CREATE TABLE PartyContacts
(
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Guid				UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	PartyId				INT NOT NULL,
	ContactType			VARCHAR(50) NULL,       -- PHONE / MOBILE / EMAIL / WHATSAPP
	ContactValue		VARCHAR(255) NOT NULL,  -- e.g. '03001234567'
	IsPrimary			SMALLINT DEFAULT 0,
	PointOfContact		VARCHAR(100) NULL,
	IsActive            SMALLINT			NOT NULL DEFAULT 1,
	CreatedBy			INT					NULL DEFAULT NULL,
	CreatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy           INT					NULL DEFAULT NULL,
	UpdatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT			NOT NULL DEFAULT 0,
	RowVersion          ROWVERSION,
	FOREIGN KEY (PartyId) REFERENCES Parties(Id),
	FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- Financials
CREATE TABLE PartyFinancials
(
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Guid				UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	PartyId				INT NOT NULL,

	Description			VARCHAR(100) NULL,    -- e.g. 'Credit Limit', 'Trade Discount'
	ValueType			VARCHAR(20) NULL,     -- PERCENTAGE / AMOUNT / DAYS
	Value				DECIMAL(18,4) NULL,
	IsActive            SMALLINT			NOT NULL DEFAULT 1,
	CreatedBy			INT					NULL DEFAULT NULL,
	CreatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy           INT					NULL DEFAULT NULL,
	UpdatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT			NOT NULL DEFAULT 0,
	RowVersion          ROWVERSION,
	FOREIGN KEY (PartyId) REFERENCES Parties(Id),
	FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- Documents
CREATE TABLE PartyDocuments
(
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Guid				UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	PartyId				INT NOT NULL,
	DocumentType		VARCHAR(50) NOT NULL,   -- NTN / CNIC / GST / LICENSE / IMEI / GSM
	DocumentNumber		VARCHAR(50) NOT NULL,
	PercentageAmount	VARCHAR(100) NULL,
	IsActive            SMALLINT			NOT NULL DEFAULT 1,
	CreatedBy			INT					NULL DEFAULT NULL,
	CreatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy           INT					NULL DEFAULT NULL,
	UpdatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT			NOT NULL DEFAULT 0,
	RowVersion          ROWVERSION,
	FOREIGN KEY (PartyId) REFERENCES Parties(Id),
	FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- Vehicles
CREATE TABLE PartyVehicles
(
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Guid				UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	PartyId				INT NOT NULL,
	VehicleRegNo		VARCHAR(50) NULL,
	EngineNo			VARCHAR(50) NULL,
	ChasisNo			VARCHAR(50) NULL,
	VehicleType			VARCHAR(50) NULL,
	MakeType			VARCHAR(50) NULL,
	Model				VARCHAR(50) NULL,
	IsActive            SMALLINT			NOT NULL DEFAULT 1,
	CreatedBy			INT					NULL DEFAULT NULL,
	CreatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy           INT					NULL DEFAULT NULL,
	UpdatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT			NOT NULL DEFAULT 0,
	RowVersion          ROWVERSION,
	FOREIGN KEY (PartyId) REFERENCES Parties(Id),
	FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- Bank Details (Final)
CREATE TABLE PartyBankDetails
(
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Guid				UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	PartyId				INT NOT NULL,           -- e.g. Customer/Supplier
	BankId				INT NOT NULL,           -- e.g. MCB / HBL (record in Parties)
	AccountTitle		VARCHAR(100) NULL,
	AccountNumber		VARCHAR(50) NULL,
	IBAN				VARCHAR(34) NULL,
	BranchCode			VARCHAR(50) NULL,
	IsPrimary			BIT DEFAULT 0,
	IsActive            SMALLINT			NOT NULL DEFAULT 1,
	CreatedBy			INT					NULL DEFAULT NULL,
	CreatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy           INT					NULL DEFAULT NULL,
	UpdatedOn           DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT			NOT NULL DEFAULT 0,
	RowVersion          ROWVERSION,
	FOREIGN KEY (PartyId) REFERENCES Parties(Id),
	FOREIGN KEY (BankId) REFERENCES Parties(Id),
	FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);


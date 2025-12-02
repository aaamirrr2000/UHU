CREATE TABLE PartyBankDetails
(
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Guid				UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	PartyId				INT NOT NULL,
	BankId				INT NOT NULL,  
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
	FOREIGN KEY (BankId) REFERENCES Bank(Id),
	FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

INSERT INTO PartyBankDetails (PartyId, BankId, AccountTitle, AccountNumber, IBAN, BranchCode, IsPrimary)
SELECT 
    p.Id, 
    1, 
    UPPER(p.Name) + ' ACCOUNT',
    '1234567890',
    'PK36HABB1234567890123456',
    '0123',
    1
FROM Parties p 
WHERE p.Name = 'S&S ENTERPRISES, KARACHI (210-4)'
UNION ALL
SELECT 
    p.Id, 
    2,
    UPPER(p.Name) + ' CURRENT ACCOUNT',
    '9876543210',
    'PK36UNIL9876543210987654',
    '0456',
    1
FROM Parties p 
WHERE p.Name = 'TCS ONLINE SALE ACCOUNT (240-22)'
UNION ALL
SELECT 
    p.Id, 
    3,
    UPPER(p.Name),
    '5555555555',
    'PK36MUCB5555555555555555',
    '0789',
    1
FROM Parties p 
WHERE p.Name = 'ALI ENTERPRISE (240-1)';
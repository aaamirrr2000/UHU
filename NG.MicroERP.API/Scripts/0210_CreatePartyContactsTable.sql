CREATE TABLE PartyContacts
(
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Guid				UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	PartyId				INT NOT NULL,
	ContactType			VARCHAR(50) NULL,  
	ContactValue		VARCHAR(255) NOT NULL, 
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
	CONSTRAINT FK_PartyContacts_Party FOREIGN KEY (PartyId) REFERENCES Parties(Id),
	CONSTRAINT FK_PartyContacts_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_PartyContacts_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

INSERT INTO PartyContacts (PartyId, ContactType, ContactValue, IsPrimary, PointOfContact)
SELECT Id, 'PHONE', '1111111', 1, 'OFFICE'
FROM Parties WHERE Name = 'S&S ENTERPRISES, KARACHI (210-4)';

INSERT INTO PartyContacts (PartyId, ContactType, ContactValue, IsPrimary, PointOfContact)
SELECT Id, 'MOBILE', '3342437346', 1, 'PRIMARY CONTACT'
FROM Parties WHERE Name = 'TCS ONLINE SALE ACCOUNT (240-22)';

INSERT INTO PartyContacts (PartyId, ContactType, ContactValue, IsPrimary, PointOfContact)
SELECT Id, 'PHONE', '111111', 1, 'MAIN OFFICE'
FROM Parties WHERE Name = 'ALI ENTERPRISE (240-1)';
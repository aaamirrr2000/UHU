
CREATE TABLE PartyFinancials
(
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Guid				UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	PartyId				INT NOT NULL,
	Description			VARCHAR(100) NULL,
	ValueType			VARCHAR(20) NULL, 
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
	CONSTRAINT FK_PartyFinancials_Party FOREIGN KEY (PartyId) REFERENCES Parties(Id),
	CONSTRAINT FK_PartyFinancials_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_PartyFinancials_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- Insert 0 credit days for ALL parties (default)
INSERT INTO PartyFinancials (PartyId, Description, ValueType, Value)
SELECT Id, 'CREDIT DAYS', 'DAYS', 0
FROM Parties;

-- Then update specific parties that have 90 days
UPDATE pf
SET Value = 90
FROM PartyFinancials pf
INNER JOIN Parties p ON pf.PartyId = p.Id
WHERE pf.Description = 'CREDIT DAYS'
AND p.Name IN (
    'HYPERSTAR FORTRESS LAHORE (240-15)',
    'IMTIAZ SUPER MARKET AWAMI MARKAZ (240-14',
    'IMTIAZ SUPER MARKET NAZIMABADAD (240-13)',
    'METRO CASH AND CARRY (240-12)',
    'MOHAMMAD ANWAR (240-11)'
);
CREATE TABLE Parties
(
	Id                      INT             PRIMARY KEY IDENTITY(1,1),
	Guid                    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	OrganizationId          INT             NOT NULL,
	Code                    VARCHAR(50)     NOT NULL,
	Pic						VARCHAR(255),
	Name                    VARCHAR(50)     NOT NULL,
	PartyType               VARCHAR(100)    NULL DEFAULT NULL,
	Address                 VARCHAR(255)    NULL DEFAULT NULL,
	Phone                   VARCHAR(30)     NULL DEFAULT NULL,
	Email                   VARCHAR(255)    NULL DEFAULT NULL,
	NTN						VARCHAR(255)    NULL DEFAULT NULL,
	CNIC					VARCHAR(255)    NULL DEFAULT NULL,
	DiscountPercentage      DECIMAL(10, 4)  NULL DEFAULT NULL,
	AccountId               INT             NULL DEFAULT NULL,
	Latitude                VARCHAR(30)     NULL DEFAULT NULL,
	Longitude               VARCHAR(30)     NULL DEFAULT NULL,
	Radius                  INT             NULL DEFAULT NULL,
	Username                VARCHAR(50)     NOT NULL,
	Password                VARCHAR(255)    NOT NULL,
	IsActive                SMALLINT        NULL DEFAULT 1,
	CreatedBy				INT             NULL DEFAULT NULL,
	CreatedOn				DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom				VARCHAR(255)    NULL DEFAULT NULL,
	UpdatedBy				INT             NULL DEFAULT NULL,
	UpdatedOn				DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom				VARCHAR(255)    NULL DEFAULT NULL,
	IsSoftDeleted           SMALLINT        NULL DEFAULT 0,
	RowVersion              ROWVERSION,
	FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
	FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(Id),
	FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

INSERT INTO Parties 
    (Guid, OrganizationId, Code, Name, PartyType, Address, Phone, Email, NTN, CNIC, DiscountPercentage, AccountId, Latitude, Longitude, Radius, Username, Password, IsActive, CreatedOn, CreatedBy, CreatedFrom, UpdatedOn, UpdatedBy, UpdatedFrom)
VALUES 
    (NEWID(), 1, '000001', 'WALK IN CUSTOMER', 'CUSTOMER', 'WALK IN CUSTOMER', '00000000000', 'NO EMAIL', '00000000000', '00000000000', 0.00, 11, '33.56739413432688', '73.13653547252294', 10, 'walkinuser1', 'password123', 1, '2024-08-17 23:47:01', 1, NULL, '2024-08-18 15:33:58', 1, NULL),
    (NEWID(), 1, '000002', 'WALK IN SUPPLIER', 'SUPPLIER', 'WALK IN SUPPLIER', '00000000000', 'NO EMAIL', '00000000000', '00000000000', 0.00, 14, '33.56739413432688', '73.13653547252294', 10, 'walkinsupplier2', 'password456', 1, '2024-08-17 23:47:01', 1, NULL, '2024-08-18 15:33:58', 1, NULL),
    (NEWID(), 1, '000003', 'BANK', 'BANK', 'BANK', '00000000000', 'NO EMAIL', '00000000000', '00000000000', 0.00, 14, '33.5980495840254', '73.04993519824058', 10, 'bankuser3', 'password789', 1, '2024-08-17 23:47:01', 1, NULL, '2024-08-18 15:33:58', 1, NULL);

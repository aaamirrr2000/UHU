CREATE TABLE Bank
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT NOT NULL DEFAULT 1,
    Code            VARCHAR(50) NOT NULL,
    BankName        VARCHAR(150) NOT NULL,
    BranchCode      VARCHAR(50) NULL,
    BranchName      VARCHAR(150) NULL,
    AccountTitle    VARCHAR(150) NULL,
    AccountNumber   VARCHAR(50) NULL,
    IBAN            VARCHAR(50) NULL,
    Address         VARCHAR(255) NULL,
    CityId          INT,
    AccountId       INT,
    Phone           VARCHAR(50) NULL,
    Email           VARCHAR(100) NULL,
    IsActive        INT NOT NULL DEFAULT 1,
	CreatedBy	    INT					NULL DEFAULT NULL,
	CreatedOn       DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom     VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy       INT					NULL DEFAULT NULL,
	UpdatedOn       DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom     VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted   SMALLINT			NOT NULL DEFAULT 0,
	CONSTRAINT FK_Bank_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
	CONSTRAINT FK_Bank_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Bank_City FOREIGN KEY (CityId) REFERENCES Areas(Id),
    CONSTRAINT FK_Bank_Account FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(Id),
	CONSTRAINT FK_Bank_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

INSERT INTO Bank
(
    Code,
    BankName,
    BranchCode,
    BranchName,
    AccountTitle,
    AccountNumber,
    IBAN,
    Address,
    CityId,
    AccountId,
    Phone,
    Email
)
VALUES
('001', 'United Bank Limited', '', 'Main Branch Karachi', 'Healthwire Pharmacy Islamabad', '0012345678901', 'PK14HABB0001234567890101', 'I.I. Chundrigar Road, Karachi', 12, 9, '021-111111111', 'info@hbl.com');

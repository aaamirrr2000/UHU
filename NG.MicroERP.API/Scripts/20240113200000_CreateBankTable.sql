CREATE TABLE Bank
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Guid            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
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
    IsActive        BIT NOT NULL DEFAULT 1,
	CreatedBy	    INT					NULL DEFAULT NULL,
	CreatedOn       DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom     VARCHAR(255)		NULL DEFAULT NULL,
	UpdatedBy       INT					NULL DEFAULT NULL,
	UpdatedOn       DATETIME			NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom     VARCHAR(255)		NULL DEFAULT NULL,
	IsSoftDeleted   SMALLINT			NOT NULL DEFAULT 0,
	RowVersion      ROWVERSION,
	FOREIGN KEY (CreatedBy)         REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy)         REFERENCES Users(Id),
    FOREIGN KEY (CityId)            REFERENCES Areas(Id),
    FOREIGN KEY (AccountId)         REFERENCES ChartOfAccounts(Id),
	FOREIGN KEY (OrganizationId)    REFERENCES Organizations(Id)
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
('001', 'Habib Bank Limited', '1234', 'Main Branch Karachi', 'ABC Distributors', '0012345678901', 'PK14HABB0001234567890101', 'I.I. Chundrigar Road, Karachi', 12, 9, '021-111111111', 'info@hbl.com'),
('002', 'MCB Bank Limited', '5678', 'Main Branch Lahore', 'XYZ Distributors', '0023456789012', 'PK12MUCB0002345678901201', 'Shahrah-e-Quaid-e-Azam, Lahore', 9, 9, '042-111000622', 'info@mcb.com.pk'),
('003', 'United Bank Limited', '3456', 'Main Branch Islamabad', 'VVV Distributors', '0034567890123', 'PK55UNIL0003456789012301', 'Blue Area, Islamabad', 19, 9, '051-111825888', 'info@ubl.com.pk'),
('004', 'National Bank of Pakistan', '7890', 'Main Branch Karachi', 'CCC Distributors', '0045678901234', 'PK36NBPA0004567890123401', 'I.I. Chundrigar Road, Karachi', 12, 9, '021-99220100', 'info@nbp.com.pk'),
('005', 'Bank Alfalah Limited', '1122', 'Main Branch Lahore', 'BBB Distributors', '0056789012345', 'PK72ALFH0005678901234501', 'Gulberg III, Lahore', 9, 9, '042-111225226', 'info@bankalfalah.com'),
('006', 'Meezan Bank Limited', '3344', 'Main Branch Karachi', 'AAA Distributors', '0067890123456', 'PK94MEZN0006789012345601', 'Shahrah-e-Faisal, Karachi', 12, 9, '021-111331331', 'info@meezanbank.com'),
('007', 'JS Bank Limited', '5566', 'Main Branch Karachi', 'XXX Distributors', '0078901234567', 'PK43JSBL0007890123456701', 'Shaheed-e-Millat Road, Karachi', 12, 9, '021-111654321', 'info@jsbl.com'),
('008', 'Faysal Bank Limited', '7788', 'Main Branch Lahore', 'YYY Distributors', '0089012345678', 'PK11FAYS0008901234567801', 'Gulberg II, Lahore', 9, 9, '042-111606606', 'info@faysalbank.com'),
('009', 'Askari Bank Limited', '9911', 'Main Branch Rawalpindi', 'ZZZ Distributors', '0090123456789', 'PK62ASCM0009012345678901', 'The Mall, Rawalpindi', 11, 9, '051-111000787', 'info@askaribank.com'),
('010', 'Soneri Bank Limited', '2233', 'Main Branch Karachi', 'TTT Distributors', '0101234567890', 'PK22SONR0010123456789001', 'Shaheen Complex, Karachi', 12, 9, '021-111766374', 'info@soneribank.com');

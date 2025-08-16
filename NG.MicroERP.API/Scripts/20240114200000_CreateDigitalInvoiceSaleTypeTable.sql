CREATE TABLE DigitalInvoiceSaleType
(
    Id                      INT IDENTITY(1,1) PRIMARY KEY,
    Guid                    UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
	OrganizationId          INT                 NOT NULL,
    Code                    VARCHAR(15)         NOT NULL,
    SaleType                VARCHAR(50)         NOT NULL,
    Description             VARCHAR(Max)         NOT NULL,
    IsActive                SMALLINT            NOT NULL DEFAULT 1,
	CreatedBy               INT                 NULL DEFAULT NULL,
	CreatedOn               DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom             VARCHAR(255)        NULL DEFAULT NULL,
	UpdatedBy               INT                 NULL DEFAULT NULL,
	UpdatedOn               DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom             VARCHAR(255)        NULL DEFAULT NULL,
	IsSoftDeleted           SMALLINT            NOT NULL DEFAULT 0,
	RowVersion              ROWVERSION,
	FOREIGN KEY (CreatedBy)        REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy)        REFERENCES Users(Id),
	FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
);

INSERT INTO DigitalInvoiceSaleType 
	(OrganizationId, Code, SaleType, Description, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
VALUES
(1, 'G01', 'GOODS', 'SALE OF PHYSICAL GOODS', 1, NULL, 'SYSTEM', NULL, 'SYSTEM', 0),
(1, 'S01', 'SERVICES', 'SALE OF SERVICES', 1, NULL, 'SYSTEM', NULL, 'SYSTEM', 0),
(1, 'TM01', 'TOLL MANUFACTURING', 'TOLL MANUFACTURING OPERATIONS', 1, NULL, 'SYSTEM', NULL, 'SYSTEM', 0),
(1, 'SRO3', 'THIRD SCHEDULE GOODS', 'GOODS UNDER THIRD SCHEDULE', 1, NULL, 'SYSTEM', NULL, 'SYSTEM', 0),
(1, 'RED', 'GOODS AT REDUCED RATE', 'REDUCED-RATE GOODS PER SRO', 1, NULL, 'SYSTEM', NULL, 'SYSTEM', 0),
(1, 'EXM', 'EXEMPT GOODS', 'TAX-EXEMPT GOODS', 1, NULL, 'SYSTEM', NULL, 'SYSTEM', 0),
(1, 'ZRO', 'GOODS AT ZERO RATE', 'ZERO-RATED GOODS', 1, NULL, 'SYSTEM', NULL, 'SYSTEM', 0),
(1, 'FED', 'GOODS (FED IN ST MODE)', 'GOODS TAXED UNDER FED IN ST MODE', 1, NULL, 'SYSTEM', NULL, 'SYSTEM', 0),
(1, 'FED_SVC', 'SERVICES (FED IN ST MODE)', 'SERVICES TAXED UNDER FED IN ST MODE', 1, NULL, 'SYSTEM', NULL, 'SYSTEM', 0);

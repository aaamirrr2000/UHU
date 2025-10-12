CREATE TABLE Areas
(
	Id                  INT                 IDENTITY(1,1) PRIMARY KEY,
	Guid                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
	OrganizationId      INT                 NOT NULL DEFAULT 1,
	AreaName            VARCHAR(100)        NOT NULL,       -- City, Province, Region, Country name
	AreaType            VARCHAR(50)         NULL,           -- City, Province, Region, Country
	ParentId            INT                 NULL,           -- 0 for top-level (e.g., Country)
	IsActive            BIT                 NOT NULL DEFAULT 1,
	CreatedBy           INT                 NULL,
	CreatedOn           DATETIME            NOT NULL DEFAULT GETDATE(),
	CreatedFrom         VARCHAR(255)        NULL,
	UpdatedBy           INT                 NULL,
	UpdatedOn           DATETIME            NULL,
	UpdatedFrom         VARCHAR(255)        NULL,
	IsSoftDeleted       BIT                 NOT NULL DEFAULT 0,
	RowVersion          ROWVERSION,
	FOREIGN KEY (CreatedBy)      REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy)      REFERENCES Users(Id),
	FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

INSERT INTO Areas (AreaName, AreaType, ParentId, IsActive, IsSoftDeleted)
VALUES ('PAKISTAN', 'COUNTRY', 0, 1, 0);

INSERT INTO Areas (AreaName, AreaType, ParentId, IsActive, IsSoftDeleted)
VALUES
('PUNJAB', 'PROVINCE', 1, 1, 0),
('SINDH', 'PROVINCE', 1, 1, 0),
('KHYBER PAKHTUNKHWA', 'PROVINCE', 1, 1, 0),
('BALOCHISTAN', 'PROVINCE', 1, 1, 0),
('GILGIT-BALTISTAN', 'REGION', 1, 1, 0),
('ISLAMABAD CAPITAL TERRITORY', 'REGION', 1, 1, 0),
('AZAD JAMMU AND KASHMIR', 'REGION', 1, 1, 0);

INSERT INTO Areas (AreaName, AreaType, ParentId, IsActive, IsSoftDeleted)
VALUES
('LAHORE', 'CITY', 2, 1, 0),
('FAISALABAD', 'CITY', 2, 1, 0),
('RAWALPINDI', 'CITY', 2, 1, 0),
('KARACHI', 'CITY', 3, 1, 0),
('HYDERABAD', 'CITY', 3, 1, 0),
('PESHAWAR', 'CITY', 4, 1, 0),
('MARDAN', 'CITY', 4, 1, 0),
('QUETTA', 'CITY', 5, 1, 0),
('GWADAR', 'CITY', 5, 1, 0),
('MUZAFFARABAD', 'CITY', 7, 1, 0),
('ISLAMABAD', 'CITY', 6, 1, 0);

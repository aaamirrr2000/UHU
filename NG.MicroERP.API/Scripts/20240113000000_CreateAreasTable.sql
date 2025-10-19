CREATE TABLE Areas
(
    Id                  INT                 AUTO_INCREMENT PRIMARY KEY,
    Guid                CHAR(36)            NOT NULL DEFAULT (UUID()),
    OrganizationId      INT                 NOT NULL DEFAULT 1,
    AreaName            VARCHAR(100)        NOT NULL,       -- City, Province, Region, Country name
    AreaType            VARCHAR(50)         NULL,           -- City, Province, Region, Country
    ParentId            INT                 NULL,           -- 0 for top-level (e.g., Country)
    IsActive            TINYINT(1)          NOT NULL DEFAULT 1,
    CreatedBy           INT                 NULL,
    CreatedOn           DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)        NULL,
    UpdatedBy           INT                 NULL,
    UpdatedOn           DATETIME            NULL DEFAULT NULL,
    UpdatedFrom         VARCHAR(255)        NULL,
    IsSoftDeleted       TINYINT(1)          NOT NULL DEFAULT 0,
    RowVersion          TIMESTAMP           NULL,
    FOREIGN KEY (CreatedBy)      REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)      REFERENCES Users(Id),
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    FOREIGN KEY (ParentId)       REFERENCES Areas(Id)
);

INSERT INTO Areas (AreaName, AreaType, ParentId, IsActive, IsSoftDeleted)
VALUES ('PAKISTAN', 'COUNTRY', NULL, 1, 0);

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
('MUZAFFARABAD', 'CITY', 8, 1, 0),  -- Note: Changed to 8 (AZAD JAMMU AND KASHMIR)
('ISLAMABAD', 'CITY', 7, 1, 0);     -- Note: Changed to 7 (ISLAMABAD CAPITAL TERRITORY)
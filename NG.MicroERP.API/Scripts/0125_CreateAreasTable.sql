-- CREATE TABLE Areas for SQL Server
CREATE TABLE Areas
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT NULL DEFAULT 1,
    AreaName        VARCHAR(100) NOT NULL,        -- City, Province, Region, Country
    AreaType        VARCHAR(50)  NULL,            -- City, Province, Region, Country
    ParentId        INT NULL,                      -- 0 or NULL for top-level (e.g., Country)
    IsActive        INT NOT NULL DEFAULT 1,
    CreatedBy       INT NULL,
    CreatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom     VARCHAR(255) NULL,
    UpdatedBy       INT NULL,
    UpdatedOn       DATETIME NULL,
    UpdatedFrom     VARCHAR(255) NULL,
    IsSoftDeleted   INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Areas_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Areas_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Areas_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

-- Insert top-level country
INSERT INTO Areas (AreaName, AreaType, ParentId, IsActive, IsSoftDeleted)
VALUES ('PAKISTAN', 'COUNTRY', NULL, 1, 0);

-- Insert provinces/regions
INSERT INTO Areas (AreaName, AreaType, ParentId, IsActive, IsSoftDeleted)
VALUES 
('PUNJAB', 'PROVINCE', 1, 1, 0),
('SINDH', 'PROVINCE', 1, 1, 0),
('KHYBER PAKHTUNKHWA', 'PROVINCE', 1, 1, 0),
('BALOCHISTAN', 'PROVINCE', 1, 1, 0),
('GILGIT-BALTISTAN', 'REGION', 1, 1, 0),
('ISLAMABAD CAPITAL TERRITORY', 'REGION', 1, 1, 0),
('AZAD JAMMU AND KASHMIR', 'REGION', 1, 1, 0);

-- Insert cities
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
('MUZAFFARABAD', 'CITY', 8, 1, 0),  -- AZAD JAMMU AND KASHMIR
('ISLAMABAD', 'CITY', 7, 1, 0);     -- ISLAMABAD CAPITAL TERRITORY

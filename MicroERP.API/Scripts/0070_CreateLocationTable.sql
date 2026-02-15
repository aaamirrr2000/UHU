CREATE TABLE Locations
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT NULL DEFAULT 1,
    Code            VARCHAR(50) NOT NULL,
    Name            VARCHAR(50) NOT NULL,
    Address         VARCHAR(255) NOT NULL,
    PocName         VARCHAR(50) NOT NULL,
    PocEmail        VARCHAR(255) NOT NULL,
    PocPhone        VARCHAR(50) NOT NULL,
    LocationType    VARCHAR(50) NOT NULL,
    Latitude        VARCHAR(30) NULL,
    Longitude       VARCHAR(30) NULL,
    Radius          INT NULL,
    IsActive        INT NOT NULL DEFAULT 1,
    CreatedBy       INT NULL,
    CreatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom     VARCHAR(255) NULL,
    UpdatedBy       INT NULL,
    UpdatedOn       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedFrom     VARCHAR(255) NULL,
    IsSoftDeleted   INT NULL DEFAULT 0,
    CONSTRAINT FK_Locations_Organizations FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);
INSERT INTO Locations
(
    OrganizationId,
    Code,
    Name,
    Address,
    PocName,
    PocEmail,
    PocPhone,
    LocationType,
    Latitude,
    Longitude,
    Radius,
    IsActive,
    CreatedBy,
    UpdatedBy
)
VALUES
(
    1,
    '000001',
    'HEALTHWIRE PHARMACY ISLAMABAD',
    'HEALTHWIRE PHARMACY ISLAMABAD',
    'AAMIR RASHID',
    'NO EMAIL',
    'NO PHONE',
    'HEAD OFFICE',
    '33.71165089079584',
    '73.04146635447766',
    10,
    1,
    1,
    1
);

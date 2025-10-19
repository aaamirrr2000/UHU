CREATE TABLE Locations
(
    Id                  INT             PRIMARY KEY AUTO_INCREMENT,
    Guid                CHAR(36)        NOT NULL DEFAULT (UUID()),
    OrganizationId      INT             NULL DEFAULT NULL,
    Code                VARCHAR(50)     NOT NULL,
    Name                VARCHAR(50)     NOT NULL,
    Address             VARCHAR(255)    NOT NULL,
    PocName             VARCHAR(50)     NOT NULL,
    PocEmail            VARCHAR(255)    NOT NULL,
    PocPhone            VARCHAR(50)     NOT NULL,
    LocationType        VARCHAR(50)     NOT NULL,
    Latitude            VARCHAR(30)     NULL DEFAULT NULL,
    Longitude           VARCHAR(30)     NULL DEFAULT NULL,
    Radius              INT             NULL DEFAULT NULL,
    IsActive            TINYINT         NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       TINYINT         NULL DEFAULT 0,
    RowVersion          TIMESTAMP       NULL,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

INSERT INTO Locations 
    (OrganizationId, Code, Name, Address, PocName, PocEmail, PocPhone, LocationType, Latitude, Longitude, Radius, IsActive, CreatedBy, UpdatedBy)
VALUES 
    (NULL, 'LOC001', 'HEAD OFFICE', 'SADDAR', 'ZEESHAN RASHEED', 'NO EMAIL', 'NO PHONE', 'HEAD OFFICE', '33.56739413432688', '73.13653547252294', 10, 1, 1, 1),
    (NULL, 'LOC002', 'PWD SHOP', 'PWD ROAD', 'ZEESHAN RASHEED', 'NO EMAIL', 'NO PHONE', 'SALES & SERVICE CENTER', '33.56739413432688', '73.13653547252294', 10, 1, 1, 1),
    (NULL, 'LOC003', 'SADDAR SHOP', 'GHAKKAR PLAZA', 'ZEESHAN RASHEED', 'NO EMAIL', 'NO PHONE', 'SALES & SERVICE CENTER', '33.5980495840254', '73.04993519824058', 10, 1, 1, 1);
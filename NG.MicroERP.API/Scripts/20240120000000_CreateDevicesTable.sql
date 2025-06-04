CREATE TABLE Devices (
    Id                      INT             PRIMARY KEY IDENTITY(1,1),
    Guid                    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    OrganizationId          INT             NOT NULL,
    LocationId              INT             NULL DEFAULT NULL,
    DeviceName              VARCHAR(255)    NOT NULL,
    SerialNumber            VARCHAR(255)    NOT NULL,
    Model                   VARCHAR(255)    NOT NULL,
    DeviceType              VARCHAR(255)    NOT NULL,
    DeviceLocation          VARCHAR(255)    NULL DEFAULT NULL,
    Description             VARCHAR(255)    NULL DEFAULT NULL,
    IsActive                INT             NOT NULL DEFAULT 1,
    CreatedBy               INT             NULL DEFAULT NULL,
    CreatedOn               DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom             VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy               INT             NULL DEFAULT NULL,
    UpdatedOn               DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom             VARCHAR(255)    NULL DEFAULT NULL,
    RowVersion              ROWVERSION,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    FOREIGN KEY (LocationId) REFERENCES Locations(Id),
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

INSERT INTO Devices 
    (Guid, OrganizationId, LocationId, DeviceName, SerialNumber, Model, DeviceType, DeviceLocation, Description, IsActive, CreatedOn, CreatedBy, CreatedFrom, UpdatedOn, UpdatedBy, UpdatedFrom)
VALUES
    (NEWID(), 1, 1, 'ACCESS CONTROLLER', '4C03034PAZ9280B', 'ASI1212D', 'ACCESS CONTROLLER', 'MAIN DOOR', 'MAIN DOOR', 1, '2024-09-15 15:45:38', 1, NULL, '2024-10-21 10:51:39', 1, NULL),
    (NEWID(), 1, 2, 'NVR', '7B04690PAZ8FF04', 'DHI-NVR4108HS-4KS2/L', 'NVR', 'LAB DOOR', 'LAB DOOR', 1, '2024-09-15 15:45:38', 1, NULL, '2024-10-21 10:51:45', 1, NULL);

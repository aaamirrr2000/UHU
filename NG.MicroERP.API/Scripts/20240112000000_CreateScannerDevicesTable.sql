CREATE TABLE ScannerDevices
(
    Id                  INT AUTO_INCREMENT PRIMARY KEY,
    Guid                CHAR(36)        NOT NULL DEFAULT (UUID()),
    DeviceIpAddress     VARCHAR(100)    NOT NULL,
    UserName            VARCHAR(100)    NOT NULL,
    Password            VARCHAR(100)    NOT NULL,
    LocationId          INT,
    Make                VARCHAR(100)    NULL DEFAULT NULL,
    Model               VARCHAR(100)    NULL DEFAULT NULL,
    Serial              VARCHAR(100)    NULL DEFAULT NULL,
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
    FOREIGN KEY (LocationId)   REFERENCES Locations(Id)
);

INSERT INTO ScannerDevices 
(
    DeviceIpAddress, UserName, Password, LocationId, 
    Make, Model, Serial, IsActive, 
    CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom
)
VALUES 
(
    '172.16.1.202', 'admin', 'hik123456', 1,
    'Hikvision', 'DS-K1T671TMFW', 'DS-K1T671TMFW20250107V030720ENAG2372585', 1,
    1, 'SetupScript', 1, 'SetupScript'
),

(
    '172.16.1.203', 'admin', 'hik12345', 2,
    'Hikvision', 'DS-K1T671TMFW', 'DS-K1T671TMFW20250107V030720ENAG2372586', 1,
    1, 'SetupScript', 1, 'SetupScript'
);

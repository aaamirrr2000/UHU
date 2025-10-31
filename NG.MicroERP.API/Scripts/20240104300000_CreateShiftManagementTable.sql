CREATE TABLE Shifts
(
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    Guid            CHAR(36)        NOT NULL DEFAULT (UUID()),
    OrganizationId  INT NULL,
    ShiftName       VARCHAR(100) NOT NULL,
    StartTime       TIME NOT NULL,
    EndTime         TIME NOT NULL,
    FlexiTime       INT DEFAULT 0,
    IsActive        INT DEFAULT 1,
    CreatedBy       INT,
    CreatedOn       DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom     VARCHAR(250),
    UpdatedBy       INT,
    UpdatedOn       DATETIME NULL,
    UpdatedFrom     VARCHAR(250),
    IsSoftDeleted   BOOLEAN DEFAULT FALSE,
    RowVersion      TIMESTAMP NULL
);

INSERT INTO Shifts 
(
    OrganizationId,
    ShiftName,
    StartTime,
    EndTime,
    FlexiTime,
    IsActive,
    CreatedBy,
    CreatedOn,
    CreatedFrom,
    IsSoftDeleted
) 
VALUES 
(
    1,
    'Morning Shift',
    '08:30:00',
    '16:30:00',
    60,
    1,
    1,
    NOW(),
    'IP: 192.168.234.1, MAC: 005056C00001, DEVICE: AAMIR-DEV-PC',
    0
);

INSERT INTO Shifts 
(
    OrganizationId,
    ShiftName,
    StartTime,
    EndTime,
    FlexiTime,
    IsActive,
    CreatedBy,
    CreatedOn,
    CreatedFrom,
    IsSoftDeleted
) 
VALUES 
(
    1,
    'Evening Shift',
    '16:30:00',
    '11:59:00',
    60,
    1,
    1,
    NOW(),
    'IP: 192.168.234.1, MAC: 005056C00001, DEVICE: AAMIR-DEV-PC',
    0
);

CREATE TABLE Shifts
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId  INT NULL DEFAULT 1,
    ShiftName       VARCHAR(100) NOT NULL,
    StartTime       TIME NOT NULL,
    EndTime         TIME NOT NULL,
    FlexiTime       INT DEFAULT 0,
    IsActive        INT DEFAULT 1,
    CreatedBy       INT,
    CreatedOn       DATETIME DEFAULT GETDATE(),
    CreatedFrom     VARCHAR(250),
    UpdatedBy       INT,
    UpdatedOn       DATETIME NULL,
    UpdatedFrom     VARCHAR(250),
    IsSoftDeleted   INT DEFAULT 0
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
    GETDATE(),
    'IP: 192.168.234.1, MAC: 005056C00001, DEVICE: AAMIR-DEV-PC',
    0
),
(
    1,
    'Evening Shift',
    '16:30:00',
    '11:59:00',
    60,
    1,
    1,
    GETDATE(),
    'IP: 192.168.234.1, MAC: 005056C00001, DEVICE: AAMIR-DEV-PC',
    0
);

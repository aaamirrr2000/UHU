CREATE TABLE Users
(
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    Guid                   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Username               VARCHAR(30)     NOT NULL,
    Password               VARCHAR(100)    NOT NULL,
    UserType               VARCHAR(50)     NOT NULL,
    DarKLightTheme      SMALLINT        DEFAULT 0,
    EmpId             INT             NULL DEFAULT NULL,
    GroupId                INT             NULL DEFAULT NULL,
    LocationId            INT             NULL DEFAULT NULL,
    IsActive               SMALLINT        NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted          SMALLINT        NULL DEFAULT 0,
    RowVersion             ROWVERSION,
    FOREIGN KEY (GroupId) REFERENCES Groups(Id),
    FOREIGN KEY (EmpId) REFERENCES Employees(Id)
);

INSERT INTO Users (Username, Password, UserType, DarKLightTheme, EmpId, GroupId, LocationId, IsActive, CreatedBy, CreatedOn, CreatedFrom, UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted) 
VALUES ('admin', '12345', 'DESKTOP ADMIN', 1, 1, 1, 1, 1, NULL, '2024-06-09 12:21:43', NULL, NULL, '2024-06-09 12:21:43', NULL, 0),
VALUES ('waiter', '12345', 'WAITER', 0, 1, 2, 2, 1, NULL, '2024-06-09 12:30:00', NULL, NULL, '2024-06-09 12:30:00', NULL, 0),
VALUES ('kitchen', '12345', 'KITCHEN', 0, 1, 2, 2, 1, NULL, '2024-06-09 12:30:00.000', NULL, NULL, '2024-06-09 12:30:00.000', NULL, 0);

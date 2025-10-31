CREATE TABLE Users
(
    Id                  INT             PRIMARY KEY AUTO_INCREMENT,
    Guid                CHAR(36)        NOT NULL DEFAULT (UUID()),
    Username            VARCHAR(30)     NOT NULL,
    Password            VARCHAR(100)    NOT NULL,
    UserType            VARCHAR(50)     NOT NULL,
    DarKLightTheme      TINYINT         DEFAULT 0,
    EmpId               INT             NULL DEFAULT NULL,
    GroupId             INT             NULL DEFAULT NULL,
    LocationId          INT             NULL DEFAULT NULL,
    IsActive            TINYINT         NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       TINYINT         NULL DEFAULT 0,
    RowVersion          TIMESTAMP       NULL,
    FOREIGN KEY (GroupId) REFERENCES Groups(Id),
    FOREIGN KEY (EmpId) REFERENCES Employees(Id)
);

INSERT INTO Users (
    Username, Password, UserType, DarKLightTheme, EmpId, GroupId, LocationId,
    IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted
)
VALUES
    ('admin', 'ImK6TOORzwRQKFPto/ruwQ==', 'DESKTOP ADMIN', 1, 1, 1, 1, 1, NULL, NULL, NULL, NULL, 0);
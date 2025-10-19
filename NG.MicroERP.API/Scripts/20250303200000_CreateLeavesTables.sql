CREATE TABLE LeaveTypes
(
    Id                      INT AUTO_INCREMENT PRIMARY KEY,
    Guid                    CHAR(36)        NOT NULL DEFAULT (UUID()),
    OrganizationId          INT             NULL DEFAULT NULL,
    LeaveName               VARCHAR(50)     NOT NULL,
    Description             VARCHAR(255),
    IsPaid                  TINYINT         DEFAULT 0,
    MaxDaysPerYear          INT             DEFAULT 0,
    CarryForward            TINYINT         DEFAULT 0,
    IsActive                TINYINT         NOT NULL DEFAULT 1,
    CreatedBy               INT             NULL DEFAULT NULL,
    CreatedOn               DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom             VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy               INT             NULL DEFAULT NULL,
    UpdatedOn               DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UpdatedFrom             VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted           TINYINT         NULL DEFAULT 0,
    RowVersion              TIMESTAMP       NULL,
    
    FOREIGN KEY (CreatedBy)       REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)       REFERENCES Users(Id),
    FOREIGN KEY (OrganizationId)  REFERENCES Organizations(Id)
);

CREATE TABLE LeaveRequests
(
    Id                      INT AUTO_INCREMENT PRIMARY KEY,
    Guid                    CHAR(36)        NOT NULL DEFAULT (UUID()),
    OrganizationId          INT             NULL DEFAULT NULL,
    EmpId                   INT             NOT NULL,
    LeaveTypeId             INT             NOT NULL,
    StartDate               DATE            NOT NULL,
    EndDate                 DATE            NOT NULL,
    Reason                  VARCHAR(255),
    ContactAddress          VARCHAR(255),
    ContactNumber           VARCHAR(30),
    Status                  VARCHAR(15)     DEFAULT 'PENDING',
    AppliedDate             DATETIME        DEFAULT CURRENT_TIMESTAMP,
    ApprovedBy              INT             NULL,
    ApprovedDate            DATETIME        NULL,
    Remarks                 VARCHAR(255)    NULL,

    FOREIGN KEY (EmpId)         REFERENCES Employees(Id),
    FOREIGN KEY (LeaveTypeId)   REFERENCES LeaveTypes(Id),
    FOREIGN KEY (ApprovedBy)    REFERENCES Employees(Id),
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

CREATE TABLE HolidayCalendar
(
    Id                      INT AUTO_INCREMENT PRIMARY KEY,
    Guid                    CHAR(36)        NOT NULL DEFAULT (UUID()),
    OrganizationId          INT             NULL DEFAULT NULL,
    HolidayDate             DATE            NOT NULL,
    Description             VARCHAR(100),
    IsRecurring             BOOLEAN         DEFAULT FALSE,
    IsActive                TINYINT         NOT NULL DEFAULT 1,
    CreatedBy               INT             NULL DEFAULT NULL,
    CreatedOn               DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom             VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy               INT             NULL DEFAULT NULL,
    UpdatedOn               DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UpdatedFrom             VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted           TINYINT         NULL DEFAULT 0,
    RowVersion              TIMESTAMP       NULL,

    FOREIGN KEY (CreatedBy)       REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)       REFERENCES Users(Id),
    FOREIGN KEY (OrganizationId)  REFERENCES Organizations(Id)
);
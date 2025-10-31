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

INSERT INTO LeaveTypes 
(
    LeaveName, Description, IsPaid, MaxDaysPerYear, 
    CarryForward, IsActive, CreatedBy, CreatedOn, CreatedFrom, 
    UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted
)
VALUES
('Casual Leave', 'Short-term leave for personal reasons', 1, 10, 0, 1, 1, NOW(), 'System', 1, NOW(), 'System', 0),

('Sick Leave', 'Leave for medical or health-related issues', 1, 12, 1, 1, 1, NOW(), 'System', 1, NOW(), 'System', 0),

('Annual Leave', 'Planned vacation or rest leave', 1, 20, 1, 1, 1, NOW(), 'System', 1, NOW(), 'System', 0),

('Maternity Leave', 'Leave for childbirth and recovery period', 1, 90, 0, 1, 1, NOW(), 'System', 1, NOW(), 'System', 0),

('Paternity Leave', 'Leave for new fathers after childbirth', 1, 15, 0, 1, 1, NOW(), 'System', 1, NOW(), 'System', 0),

('Unpaid Leave', 'Leave without pay for personal matters', 0, 30, 0, 1, 1, NOW(), 'System', 1, NOW(), 'System', 0),

('Compensatory Leave', 'Leave granted for extra work or overtime', 1, 10, 0, 1, 1, NOW(), 'System', 1, NOW(), 'System', 0);


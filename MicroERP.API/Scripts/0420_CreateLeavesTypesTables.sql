CREATE TABLE LeaveTypes
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NULL DEFAULT 1,
    LeaveName           VARCHAR(50)     NOT NULL,
    Description         VARCHAR(255),
    IsPaid              INT             DEFAULT 0,
    MaxDaysPerYear      INT             DEFAULT 0,
    CarryForward        INT             DEFAULT 0,
    IsActive            INT             NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255)    NULL,
    UpdatedBy           INT             NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255)    NULL,
    IsSoftDeleted       INT             NULL DEFAULT 0,
    
    CONSTRAINT FK_LeaveTypes_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_LeaveTypes_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_LeaveTypes_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

INSERT INTO LeaveTypes
(
    LeaveName, Description, IsPaid, MaxDaysPerYear, 
    CarryForward, IsActive, CreatedBy, CreatedOn, CreatedFrom, 
    UpdatedBy, UpdatedOn, UpdatedFrom, IsSoftDeleted
)
VALUES
('Casual Leave', 'Short-term leave for personal reasons', 1, 10, 0, 1, 1, GETDATE(), 'System', 1, GETDATE(), 'System', 0),
('Sick Leave', 'Leave for medical or health-related issues', 1, 12, 1, 1, 1, GETDATE(), 'System', 1, GETDATE(), 'System', 0),
('Annual Leave', 'Planned vacation or rest leave', 1, 20, 1, 1, 1, GETDATE(), 'System', 1, GETDATE(), 'System', 0),
('Maternity Leave', 'Leave for childbirth and recovery period', 1, 90, 0, 1, 1, GETDATE(), 'System', 1, GETDATE(), 'System', 0),
('Paternity Leave', 'Leave for new fathers after childbirth', 1, 15, 0, 1, 1, GETDATE(), 'System', 1, GETDATE(), 'System', 0),
('Unpaid Leave', 'Leave without pay for personal matters', 0, 30, 0, 1, 1, GETDATE(), 'System', 1, GETDATE(), 'System', 0),
('Compensatory Leave', 'Leave granted for extra work or overtime', 1, 10, 0, 1, 1, GETDATE(), 'System', 1, GETDATE(), 'System', 0);

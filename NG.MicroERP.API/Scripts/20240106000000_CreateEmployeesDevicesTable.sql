CREATE TABLE EmployeesDevices
(
	Id                  INT             PRIMARY KEY IDENTITY(1,1),
	Guid                   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	OrganizationId         INT             NULL DEFAULT NULL,
	EmpId                  INT,
	Device                 VARCHAR(255)    NOT NULL,
	DeviceOs               VARCHAR(255),
	DeviceType             VARCHAR(255)    NOT NULL,
	DeviceSr               VARCHAR(255)    NOT NULL,
	IssuedOn               DATETIME,
	IssueBy                INT,
	DeviceLife             INT,
	IsActive               SMALLINT        NOT NULL DEFAULT 1,
	CreatedBy           INT             NULL DEFAULT NULL,
	CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
	UpdatedBy           INT             NULL DEFAULT NULL,
	UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
	IsSoftDeleted          SMALLINT        NULL DEFAULT 0,
	RowVersion             ROWVERSION,
    FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
);

INSERT INTO EmployeesDevices 
(
    Guid, 
    OrganizationId, 
    EmpId, 
    Device, 
    DeviceOs, 
    DeviceType, 
    DeviceSr, 
    IssuedOn, 
    IssueBy, 
    DeviceLife, 
    IsActive, 
    CreatedBy, 
    CreatedOn, 
    CreatedFrom, 
    UpdatedBy, 
    UpdatedOn, 
    UpdatedFrom, 
    IsSoftDeleted
) 
VALUES 
(
    NEWID(), 
    NULL, 
    1, 
    '88665A286A65', 
    'WINDOWS 11 PRO', 
    'HP 8470P', 
    '9BEB2B45-23B8-4BC3-AE42-ED7DC80E85CE', 
    '2024-04-12 17:45:56', 
    1, 
    365, 
    1, 
    NULL, 
    '2024-04-12 17:45:56', 
    NULL, 
    NULL, 
    '2024-04-12 17:45:56', 
    NULL, 
    0
);

CREATE TABLE Leaves
	(
	Id                   INT              PRIMARY KEY IDENTITY(1,1),
	Guid                 UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	OrganizationId          INT             NOT NULL,
	EmpId                INT,
	LeaveType            VARCHAR(50)      NOT NULL,
	FromDate             DATETIME,
	ToDate               DATETIME,
	Description          VARCHAR(50),
	Approved            DATETIME,
	ApprovedBy          INT,
	CreatedBy           INT             NULL DEFAULT NULL,
	CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
	UpdatedBy           INT             NULL DEFAULT NULL,
	UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
	IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,
	RowVersion          ROWVERSION,
  
	FOREIGN KEY (EmpId)            REFERENCES Employees(Id),
	FOREIGN KEY (ApprovedBy)       REFERENCES Employees(Id),
	FOREIGN KEY (CreatedBy)        REFERENCES Users(Id),
	FOREIGN KEY (UpdatedBy)        REFERENCES Users(Id),
	FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id)
);

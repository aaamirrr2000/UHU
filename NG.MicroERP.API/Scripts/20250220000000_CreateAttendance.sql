CREATE TABLE Attendance
(
    Id                      INT             PRIMARY KEY     IDENTITY(1,1),
    Guid                    UNIQUEIDENTIFIER DEFAULT NEWID(),
    OrganizationId          INT,
    TranDate                DATETIME,
    EmpId                   INT,
    TempLoc                 VARCHAR(100),
    Longitude              VARCHAR(30),
    Latitude               VARCHAR(30),
    InOutNa                VARCHAR(5),
    UserDefaultLocation     TINYINT,
    Source                 VARCHAR(1000),
    RowVersion             ROWVERSION,
    FOREIGN KEY (EmpId) REFERENCES Employees(Id)
);

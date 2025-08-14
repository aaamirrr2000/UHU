CREATE TABLE FbrSubmission
(
    Id                      INT IDENTITY(1,1) PRIMARY KEY,
    BillId                  INT NOT NULL,
    JsonPayload             NVARCHAR(MAX) NOT NULL,
    SubmissionDateTime      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ResponseCode            NVARCHAR(50) NULL,
    IRN                     NVARCHAR(100) NULL,
    DigitalInvoiceUrl       NVARCHAR(250) NULL,
    QRCodeData              NVARCHAR(MAX) NULL,
    ErrorMessage            NVARCHAR(MAX) NULL,
    RetryCount              INT NOT NULL DEFAULT 0,
    SubmissionMachineInfo   NVARCHAR(250) NULL,
    CreatedOn               DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (BillId) REFERENCES Bill(Id)
);

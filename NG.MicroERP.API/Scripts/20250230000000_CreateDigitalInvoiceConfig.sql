CREATE TABLE DigitalInvoiceConfig
(
    Id                      INT             PRIMARY KEY IDENTITY(1,1),
    Guid                    UNIQUEIDENTIFIER DEFAULT NEWID(),
    OrganizationId          INT NOT NULL,
    Country     	        VARCHAR(50) NOT NULL,          -- Country for the digital invoice
    
    PosId                   VARCHAR(100) NULL,             -- POS ID
    ClientId                VARCHAR(100) NOT NULL,         -- API client ID
    ClientSecret            VARCHAR(255) NOT NULL,         -- API client secret (store encrypted)
    Username                VARCHAR(50) NOT NULL,          -- Login username
    Password                VARCHAR(255) NOT NULL,         -- Login password (hashed & salted)
    
    Target                  VARCHAR(50) NOT NULL,          -- 'Production' or 'Sandbox'
    TargetApi               VARCHAR(255) NOT NULL,         -- API Base URL
    
    IsDefault               BIT NOT NULL DEFAULT 1,        

    CreatedBy               INT NULL,
    CreatedOn               DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom             VARCHAR(255) NULL,

    UpdatedBy               INT NULL,
    UpdatedOn               DATETIME NULL,                 -- Updated explicitly, not default
    UpdatedFrom             VARCHAR(255) NULL,

    IsSoftDeleted           BIT NOT NULL DEFAULT 0,        -- Soft delete flag
    RowVersion              ROWVERSION                     -- Concurrency control
);


INSERT INTO DigitalInvoiceConfig
(
    OrganizationId,
    Country,
    PosId,
    ClientId,
    ClientSecret,
    Username,
    Password,
    Target,
    TargetApi,
    IsDefault,
    CreatedBy,
    CreatedFrom
)
VALUES
-- Production
(1, 'PAKISTAN', '00000', 'PROD_CLIENT_001', 'ProdSecretKey123456789', 'produser', '12345', 
 'PRODUCTION', 'https://api.production-abc.gov.pk/invoice', 1, 101, 'SystemSetup'),

-- Sandbox
(1, 'PAKISTAN', '00000','SANDBOX_CLIENT_001', 'SandboxSecretKey987654321', 'testuser', '54321', 
 'SANDBOX', 'https://sandbox.abc.gov.pk/invoice', 1, 101, 'SystemSetup');
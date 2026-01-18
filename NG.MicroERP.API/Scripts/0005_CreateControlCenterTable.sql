
CREATE DATABASE ControlCenter;
GO
USE ControlCenter;
GO

-- =============================================
-- Control Center - Users Table
-- =============================================
CREATE TABLE Users
(
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    FullName      NVARCHAR(150) NOT NULL,
    Email         NVARCHAR(150) NOT NULL,
    Phone         NVARCHAR(50)  NULL,
    PasswordHash  NVARCHAR(500) NOT NULL,
    IsSuperAdmin  INT NOT NULL DEFAULT 0,              -- 1=SuperAdmin, 0=Regular User
    IsActive      INT NOT NULL DEFAULT 1,               -- 1=Active, 0=Inactive
    CreatedBy     INT NULL,
    CreatedOn     DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom   NVARCHAR(250) NULL,
    UpdatedBy     INT NULL,
    UpdatedOn     DATETIME NULL,
    UpdatedFrom   NVARCHAR(250) NULL,
    IsSoftDeleted INT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_ControlCenterUsers_Email UNIQUE (Email)
);
GO

-- =============================================
-- Control Center - Organizations Table
-- =============================================
CREATE TABLE Organizations
(
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    Code             NVARCHAR(50) NOT NULL,
    OrganizationName NVARCHAR(200) NOT NULL,
    OwnerUserId      INT NOT NULL,
    Email            NVARCHAR(150) NULL,
    Phone            NVARCHAR(50)  NULL,
    IsActive         INT NOT NULL DEFAULT 1,           -- 1=Active, 0=Inactive
    Expiry           DATETIME,
    CreatedBy        INT NULL,
    CreatedOn        DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom      NVARCHAR(250) NULL,
    UpdatedBy        INT NULL,
    UpdatedOn        DATETIME NULL,
    UpdatedFrom      NVARCHAR(250) NULL,
    IsSoftDeleted    INT NOT NULL DEFAULT 0,
    FOREIGN KEY (OwnerUserId) REFERENCES Users(Id)
);
GO

-- =============================================
-- Control Center - Projects Table
-- =============================================
CREATE TABLE Projects
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NOT NULL,
    DatabaseName        NVARCHAR(200) NOT NULL,
    ServerIP            NVARCHAR(100) NOT NULL,
    Port                NVARCHAR(10)  NULL,
    DbUsername          NVARCHAR(150) NOT NULL,
    DbPassword          NVARCHAR(500) NOT NULL,        -- Store encrypted
    IsActive            INT NOT NULL DEFAULT 0,        -- 1=Active, 0=Inactive
    PaymentInstructions NVARCHAR(1000) NULL,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         NVARCHAR(250) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NULL,
    UpdatedFrom         NVARCHAR(250) NULL,
    IsSoftDeleted       INT NOT NULL DEFAULT 0,
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);
GO

-- Insert default user
INSERT INTO Users
(
    FullName,
    Email,
    Phone,
    PasswordHash,
    IsSuperAdmin,
    IsActive,
    CreatedBy,
    CreatedFrom
)
VALUES
(
    'Aamir Rashid',                                    -- FullName
    'aamir.rashid.1973@gmail.com',                    -- Email
    '0335-1144440',                                    -- Phone
    'REPLACE_WITH_HASH',                               -- PasswordHash (replace with actual hash)
    1,                                                 -- IsSuperAdmin = true
    1,                                                 -- IsActive = true
    1,                                                 -- CreatedBy
    'System'                                           -- CreatedFrom
);
GO

-- Insert default organization
DECLARE @OwnerUserId INT;
SET @OwnerUserId = (SELECT TOP 1 Id FROM Users WHERE Email='aamir.rashid.1973@gmail.com');

INSERT INTO Organizations
(
    Code,
    OrganizationName,
    OwnerUserId,
    Email,
    Phone,
    IsActive,
    Expiry,
    CreatedBy,
    CreatedFrom
)
VALUES
(
    'REG002',
    'NexGen Tech Studios',                             -- OrganizationName
    @OwnerUserId,                                      -- OwnerUserId
    'mail@nexgentechstudios.com',                     -- Email
    '0335-1144440',                                    -- Phone
    1,                                                 -- IsActive = true
    '2025-01-31',                                       --Expiry
    1,                                                 -- CreatedBy
    'System'                                           -- CreatedFrom
);
GO

-- Insert default project
DECLARE @OrgId INT;
SET @OrgId = (SELECT TOP 1 Id FROM Organizations WHERE OrganizationName='NexGen Tech Studios');

INSERT INTO Projects
(
    OrganizationId,
    DatabaseName,
    ServerIP,
    Port,
    DbUsername,
    DbPassword,
    IsActive,
    PaymentInstructions,
    CreatedBy,
    CreatedFrom
)
VALUES
(
    @OrgId,                                            -- OrganizationId
    'MicroERP_UHU5',                                   -- DatabaseName
    '147.93.154.249',                                  -- ServerIP
    '1433',                                            -- Port
    'sa',                                              -- DbUsername
    'DingDong_300',                                    -- DbPassword
    1,                                                 -- IsActive = true
    'Payment required monthly. Contact accounts@nexgentechstudios.com for billing.',  -- PaymentInstructions
    1,                                                 -- CreatedBy
    'System'                                           -- CreatedFrom
);
GO

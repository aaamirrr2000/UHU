CREATE TABLE SystemConfiguration
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NULL DEFAULT 1,
    Category            NVARCHAR(50) NOT NULL,           -- Backup, Images, Email, FTP
    ConfigKey           NVARCHAR(100) NOT NULL,          -- Key name (e.g., BasePath, SmtpServer)
    ConfigValue         NVARCHAR(MAX) NULL,              -- Configuration value
    Description         NVARCHAR(255) NULL,              -- Description of the configuration
    IsActive            INT NOT NULL DEFAULT 1,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         NVARCHAR(250) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NULL,
    UpdatedFrom         NVARCHAR(250) NULL,
    IsSoftDeleted       INT NOT NULL DEFAULT 0,
    
    CONSTRAINT UQ_SystemConfiguration_Org_Category_Key UNIQUE (OrganizationId, Category, ConfigKey)
);

-- Insert default Backup configurations
INSERT INTO SystemConfiguration (OrganizationId, Category, ConfigKey, ConfigValue, Description, IsActive, CreatedBy, CreatedFrom)
VALUES
(1, 'Backup', 'BasePath', 'D:\Backups', 'Base path for backup files', 1, 1, 'System'),
(1, 'Backup', 'BaseFilePrefix', 'UHU', 'Prefix for backup file names', 1, 1, 'System'),
(1, 'Backup', 'SqlServerBackupPath', 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Backup', 'Path to SQL Server backup directory', 1, 1, 'System'),
(1, 'Backup', 'ZipPassword', '12345', 'Password for backup zip files', 1, 1, 'System'),
(1, 'Backup', 'BackupEmail', 'aamir.rashid.1973@gmail.com', 'Email address for backup notifications', 1, 1, 'System');

-- Insert default Images configuration
INSERT INTO SystemConfiguration (OrganizationId, Category, ConfigKey, ConfigValue, Description, IsActive, CreatedBy, CreatedFrom)
VALUES
(1, 'Images', 'FolderPath', 'C:\inetpub\Images', 'Folder path for storing images', 1, 1, 'System');

-- Insert default Email configurations
INSERT INTO SystemConfiguration (OrganizationId, Category, ConfigKey, ConfigValue, Description, IsActive, CreatedBy, CreatedFrom)
VALUES
(1, 'Email', 'SmtpServer', 'mail.nexgentechstudios.com', 'SMTP server address', 1, 1, 'System'),
(1, 'Email', 'SmtpPort', '587', 'SMTP server port', 1, 1, 'System'),
(1, 'Email', 'SmtpUsername', 'mail@nexgentechstudios.com', 'SMTP username', 1, 1, 'System'),
(1, 'Email', 'SmtpPassword', 'Solution_00', 'SMTP password', 1, 1, 'System'),
(1, 'Email', 'FromEmail', 'mail@nexgentechstudios.com', 'Default sender email address', 1, 1, 'System'),
(1, 'Email', 'FromName', 'Cybercom Support', 'Default sender name', 1, 1, 'System');

-- Insert default FTP configurations
INSERT INTO SystemConfiguration (OrganizationId, Category, ConfigKey, ConfigValue, Description, IsActive, CreatedBy, CreatedFrom)
VALUES
(1, 'FTP', 'ServerUrl', 'ftp://147.93.154.249:8998', 'FTP server URL', 1, 1, 'System'),
(1, 'FTP', 'Username', 'FTP_User', 'FTP username', 1, 1, 'System'),
(1, 'FTP', 'Password', 'Solution_00', 'FTP password', 1, 1, 'System');

GO

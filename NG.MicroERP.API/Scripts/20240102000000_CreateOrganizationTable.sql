CREATE TABLE Organizations
(
    Id                  INT             IDENTITY(1,1) PRIMARY KEY,
    Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Code                VARCHAR(15)     NOT NULL,
    EntraId             VARCHAR(255)    NULL,
    Logo                VARCHAR(2000)   NULL DEFAULT NULL,
    Wallpaper           VARCHAR(2000)   NULL DEFAULT NULL,
    ThemeColor          VARCHAR(20)     NULL DEFAULT NULL,
    MenuColor           VARCHAR(20)     NULL DEFAULT NULL,
    Name                VARCHAR(255)    NOT NULL,
    Description         VARCHAR(255)    NOT NULL,
    Phone               VARCHAR(50)     NULL DEFAULT NULL,
    Email               VARCHAR(255)    NULL DEFAULT NULL,
    Address             VARCHAR(255)    NOT NULL,
    MaxUsers            INT             NULL DEFAULT NULL,
    DbSize              FLOAT           NULL DEFAULT NULL,
    LoginPic            VARCHAR(255)    NULL DEFAULT NULL,
    Industry            VARCHAR(255)    NULL,
    Website             VARCHAR(255)    NULL,
    TimeZone            VARCHAR(100)    NULL,
    GMT                 DECIMAL(16,2)   DEFAULT 0,
    IsVerified          BIT             NOT NULL DEFAULT 0,
    Expiry              DATETIME        NULL DEFAULT NULL,
    ParentId            INT             NULL,
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       BIT             NULL DEFAULT 0,
    RowVersion          ROWVERSION
);

-- Insert statement
INSERT INTO Organizations 
    (Code, EntraId, Logo, ThemeColor, MenuColor, Name, Description, Phone, Email, Address, 
     MaxUsers, DbSize, LoginPic, Industry, Website, TimeZone, GMT, IsVerified, Expiry, ParentId, 
     IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted) 
VALUES 
    ('ORG001', 'SomeEntraId', 'images/logo.jpg', '#333333', '#222222', 'MoITT', 
     'Ministry of IT and Telecom', 'xxxxx', 'contact@miott.com', 'Islamabad', 
     100, 500.0, 'images/loginpic.jpg', 'Software', 'https://techcorp.com', 'Asia/Karachi', 5,
     1, '2026-12-31 23:59:59', NULL, 1, 101, 'System', NULL, 'System', 0);

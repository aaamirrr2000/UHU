CREATE TABLE Organizations
(
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Code                Varchar(15)     NOT NULL,
    EntraId             Varchar(255)    NULL,
    Logo                VARCHAR(2000)   NULL DEFAULT NULL,
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
    Industry            Varchar(255)    NULL,
    Website             Varchar(255)    NULL,
    TimeZone            Varchar(100)    NULL,
    IsVerified          SMALLINT        DEFAULT 0 NOT NULL,
    Expiry              DATETIME        NULL DEFAULT NULL,
    ParentId            INT             NULL,
    IsActive            SMALLINT        NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    CreatedSource       VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedSource       VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT        NULL DEFAULT 0,
    RowVersion          ROWVERSION
);

INSERT INTO Organizations 
    (Guid, Code, EntraId, Logo, ThemeColor, MenuColor, Name, Description, Phone, Email, Address, 
     MaxUsers, DbSize, LoginPic, Industry, Website, TimeZone, IsVerified, Expiry, ParentId, 
     IsActive, CreatedBy, CreatedOn, CreatedFrom, CreatedSource, UpdatedBy, UpdatedOn, UpdatedFrom, 
     UpdatedSource, IsSoftDeleted) 
VALUES 
    (NEWID(), 'ORG001', 'SomeEntraId', 'images/logo.jpg', '#333333', '#222222', 'TechCorp', 
     'Leading tech solutions provider', '03001234567', 'contact@techcorp.com', '123 Tech Street, Karachi', 
     100, 500.0, 'images/loginpic.jpg', 'Software', 'https://techcorp.com', 'Asia/Karachi', 
     1, '2025-12-31 23:59:59', NULL, 1, 101, CURRENT_TIMESTAMP, 'System', 'WebApp', NULL, 
     CURRENT_TIMESTAMP, 'System', 'WebApp', 0);
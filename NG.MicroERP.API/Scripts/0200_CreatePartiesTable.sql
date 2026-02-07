CREATE TABLE Parties
(
    Id                  INT PRIMARY KEY IDENTITY(1,1),
    
    OrganizationId      INT             NOT NULL DEFAULT 1,
    Code                VARCHAR(50)     NOT NULL,
    Name                VARCHAR(100)    NOT NULL,
    
    PartyType           VARCHAR(100)    NULL,
    ParentId            INT             NULL,
    
    CustomerRating      DECIMAL(5,2)    NULL,
    CustomerClass       VARCHAR(50)     NULL,
    CustomerSince       DATE            NULL,
    SalesPersonId       INT             NULL,
    
    CreditLimit         DECIMAL(18,4)   NULL,
    PaymentTermsId      INT             NULL,
    AccountId           INT             NULL,
    PriceListId         INT             NULL,  -- References TypeCode.Id where ListName='PRICE LIST'

    NTN                 VARCHAR(50)     NULL,
    STN                 VARCHAR(50)     NULL,

    IsRegistered        INT NULL,                    -- 1=Registered, 0=Unregistered, NULL=All
    IsFiler             INT NULL,                    -- 1=Filer, 0=Non-Filer, NULL=All
    
    Address             VARCHAR(255)    NULL,
    CityId              INT             NULL,
    Latitude            VARCHAR(30)     NULL,
    Longitude           VARCHAR(30)     NULL,
    Radius              INT             NULL,
    
    ContactPerson       VARCHAR(100)    NULL,
    ContactDesignation  VARCHAR(100)    NULL,
    ContactEmail        VARCHAR(255)    NULL,
    Pic                 VARCHAR(255)    NULL,
    
    IsActive            SMALLINT        NOT NULL DEFAULT 1,
    IsApproved          SMALLINT        NOT NULL DEFAULT 0,
    ApprovedBy          INT             NULL,
    ApprovedOn          DATETIME        NULL,
    
    CreatedBy           INT             NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL,
    UpdatedBy           INT             NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL,
    IsSoftDeleted       SMALLINT        NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_Parties_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Parties_Account FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(Id),
    CONSTRAINT FK_Parties_City FOREIGN KEY (CityId) REFERENCES Areas(Id),
    CONSTRAINT FK_Parties_SalesPerson FOREIGN KEY (SalesPersonId) REFERENCES Employees(Id),
    CONSTRAINT FK_Parties_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Parties_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Parties_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Parties_PaymentTerms FOREIGN KEY (PaymentTermsId) REFERENCES PaymentTerms(Id),
    CONSTRAINT FK_Parties_PriceList FOREIGN KEY (PriceListId) REFERENCES TypeCode(Id),
    
    CHECK (PartyType IN ('CUSTOMER', 'SUPPLIER', 'BANK', 'EMPLOYEE'))
);

INSERT INTO Parties
(
    Code,
    Name,
    PartyType,
    CustomerRating,
    CustomerClass,
    CustomerSince,
    AccountId,
    CityId
)
VALUES
('000001', 'SHAH M TALHA TAHIR', 'EMPLOYEE', NULL, NULL, '2021-01-01', NULL, 1),
('000002', 'AAMIR RASHID', 'EMPLOYEE', NULL, NULL, '2021-01-01', NULL, 1),
('000003', 'DR MAHNOOR', 'EMPLOYEE', NULL, NULL, '2021-01-01', NULL, 1),
('000004', 'ZUBAIR', 'EMPLOYEE', NULL, NULL, '2021-01-01', NULL, 1),
('000005', 'HASNAIN ARSHAD', 'EMPLOYEE', NULL, NULL, '2021-01-01', NULL, 1),
('000006', 'USMAN HABIB', 'EMPLOYEE', NULL, NULL, '2021-01-01', NULL, 1),
('000007', 'M MUBASHIR', 'EMPLOYEE', NULL, NULL, '2021-01-01', NULL, 1),
('000008', 'MALIK UMAIR', 'EMPLOYEE', NULL, NULL, '2021-01-01', NULL, 1),
('000009', 'PREMIER SALE PVT LTD', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000010', 'HA ENTERPRISE', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000011', 'BAY G PHARMA', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000012', 'FAWAD DISTRIBUTION', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000013', 'PREMIER', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000014', 'PHARMA NET', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000015', 'CHAUDHARY SONS', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000016', 'FAROOQ SURGICAL', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000017', 'ALNOOR TRADERS', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000018', 'ALI AND WALI TRADERS', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000019', 'AL SYED SURGICAL', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000020', 'BLUE BIRD MADI PLUS', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000021', 'WAQAS TRADERS', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000022', 'STANDARD MEDICAL STORE', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000023', 'NW TRADERS', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000024', 'CITY HERBAL CARE LAB', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000025', 'DRUG SERVICES', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000026', 'VISCOUNT DISTRIBUTOR', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000027', 'VISCOUNT', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000028', 'FAWAD MEDICOSE', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000029', 'HEALTHWIRE', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000030', 'ALAMGIR PHARMACY', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000031', 'ISLAMABAD PHARMACY', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000032', 'DANIYAL PHARMACY', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000033', 'ALAMGIR PHARMACY', 'CUSTOMER', NULL, NULL, '2021-01-01', 13, 12),
('000034', 'ISLAMABAD PHARMACY', 'CUSTOMER', NULL, NULL, '2021-01-01', 13, 12),
('000035', 'DANIYAL PHARMACY', 'CUSTOMER', NULL, NULL, '2021-01-01', 13, 12),
('000036', 'PHARMACY 24', 'SUPPLIER', NULL, NULL, '2021-01-01', 16, 12),
('000037', 'PHARMACY 24', 'CUSTOMER', NULL, NULL, '2021-01-01', 13, 12);

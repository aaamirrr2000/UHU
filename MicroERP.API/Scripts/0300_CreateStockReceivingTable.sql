CREATE TABLE StockReceiving
(
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    Guid            UNIQUEIDENTIFIER DEFAULT NEWID(),
    OrganizationId  INT             NOT NULL,
    FileAttachment  VARCHAR(255),
    TranDate        DATETIME        NULL DEFAULT NULL,
    ItemId          INT             NULL DEFAULT NULL,
    StockCondition  VARCHAR(50)     NULL DEFAULT 'NEW',
    Qty             DECIMAL(16, 4)  NULL DEFAULT NULL,
    Price           DECIMAL(16, 4)  NULL DEFAULT 0.0000,
    Description     VARCHAR(255)    NULL DEFAULT NULL,
    ExpDate         DATETIME        NULL DEFAULT NULL,
    PartyId         INT             NULL DEFAULT NULL,
    LocationId      INT             NULL DEFAULT NULL,
	CreatedBy           INT             NULL DEFAULT NULL,
	CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
	UpdatedBy           INT             NULL DEFAULT NULL,
	UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    RowVersion      ROWVERSION      NOT NULL,

    CONSTRAINT FK_StockReceiving_Party FOREIGN KEY (PartyId) REFERENCES Parties(Id),
    CONSTRAINT FK_StockReceiving_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),  
    CONSTRAINT FK_StockReceiving_Location FOREIGN KEY (LocationId) REFERENCES Locations(Id)
);

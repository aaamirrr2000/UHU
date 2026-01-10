CREATE TABLE PriceList 
(
    Id                    INT              IDENTITY(1,1) PRIMARY KEY,
    OrganizationId        INT              NOT NULL        DEFAULT 1,
    ItemId                INT              NOT NULL,
    PriceListName         VARCHAR(100)     NOT NULL,
    MinQuantity           INT              NOT NULL        DEFAULT 1,
    OneQuantityPrice      DECIMAL(18,2)    NOT NULL,
    MinQuantityPrice      DECIMAL(18,2)    NOT NULL,
    EffectiveDate         DATE             NOT NULL        DEFAULT CURRENT_TIMESTAMP,
    ExpiryDate            DATE             NULL,
    IsActive              SMALLINT         NOT NULL        DEFAULT 1,
    CreatedBy             INT              NULL            DEFAULT NULL,
    CreatedOn             DATETIME         NOT NULL        DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom           VARCHAR(255)     NULL            DEFAULT NULL,
    UpdatedBy             INT              NULL            DEFAULT NULL,
    UpdatedOn             DATETIME         NOT NULL        DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom           VARCHAR(255)     NULL            DEFAULT NULL,
    IsSoftDeleted         SMALLINT         NOT NULL        DEFAULT 0,
    CONSTRAINT FK_PriceList_Organization  FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id),
    CONSTRAINT FK_PriceList_Item          FOREIGN KEY (ItemID)           REFERENCES Items(Id),
    CONSTRAINT FK_PriceList_CreatedBy     FOREIGN KEY (CreatedBy)        REFERENCES Users(Id),
    CONSTRAINT FK_PriceList_UpdatedBy     FOREIGN KEY (UpdatedBy)        REFERENCES Users(Id)
);



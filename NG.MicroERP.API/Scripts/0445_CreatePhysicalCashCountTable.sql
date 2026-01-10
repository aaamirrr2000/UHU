CREATE TABLE PhysicalCashCount
(
    Id                  INT              PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT             NOT NULL,
    LocationId          INT             NOT NULL,
    Locker              VARCHAR(100)     NOT NULL,
    CountDate           DATE             NOT NULL,
    Denomination        DECIMAL(10, 2)   NOT NULL,
    Quantity            INT              NOT NULL DEFAULT 0,
    Amount              DECIMAL(18, 2)   NOT NULL DEFAULT 0,
    Notes               VARCHAR(500)     NULL,
    CountedBy           INT             NOT NULL,
    VerifiedBy          INT             NULL,
    VerifiedOn          DATETIME        NULL,
    Status              VARCHAR(20)      NOT NULL DEFAULT 'PENDING', -- PENDING, VERIFIED, DISPUTED
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT         NOT NULL DEFAULT 0,

    FOREIGN KEY (OrganizationId)   REFERENCES Organizations(Id),
    FOREIGN KEY (LocationId)      REFERENCES Locations(Id),
    FOREIGN KEY (CountedBy)       REFERENCES Users(Id),
    FOREIGN KEY (VerifiedBy)      REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy)       REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)       REFERENCES Users(Id)
);

CREATE INDEX IX_PhysicalCashCount_OrganizationId ON PhysicalCashCount(OrganizationId);
CREATE INDEX IX_PhysicalCashCount_LocationId ON PhysicalCashCount(LocationId);
CREATE INDEX IX_PhysicalCashCount_CountDate ON PhysicalCashCount(CountDate);
CREATE INDEX IX_PhysicalCashCount_Status ON PhysicalCashCount(Status);

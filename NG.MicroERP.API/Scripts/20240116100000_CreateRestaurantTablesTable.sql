CREATE TABLE RestaurantTables (
    Id                  INT             PRIMARY KEY IDENTITY(1,1),
    OrganizationId      INT NULL,
    TableNumber         VARCHAR(10)     NOT NULL UNIQUE,
    Capacity            INT             NOT NULL,
    IsAvailable         INT             NOT NULL DEFAULT 1,
    TableLocation       VARCHAR(50),    -- e.g., Indoor, Outdoor, Balcony
    Notes               VARCHAR(255),
    IsActive            SMALLINT        NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL DEFAULT NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL DEFAULT NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       SMALLINT        NULL DEFAULT 0
);

INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes,
    IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
VALUES
(1, 'T01', 4, 1, 'Indoor', 'Near the window', 1, 101, '192.168.1.10', 101, '192.168.1.10', 0),
(1, 'T02', 2, 1, 'Outdoor', 'Shaded area', 1, 102, '192.168.1.11', 102, '192.168.1.11', 0),
(1, 'T03', 6, 0, 'Balcony', 'Reserved for VIPs', 1, 103, '192.168.1.12', 103, '192.168.1.12', 0),
(1, 'T04', 4, 1, 'Indoor', 'Next to the bar', 1, 104, '192.168.1.13', 104, '192.168.1.13', 0),
(1, 'T05', 8, 1, 'Outdoor', 'Family table', 1, 105, '192.168.1.14', 105, '192.168.1.14', 0);


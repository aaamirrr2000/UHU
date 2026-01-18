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

-- Insert sample tables with NULL OrganizationId so they appear for all organizations
-- Only insert if they don't already exist
IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T01')
BEGIN
    INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes,
        IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
    VALUES
    (NULL, 'T01', 4, 1, 'Indoor', 'Near the window', 1, 1, 'System', 1, 'System', 0),
    (NULL, 'T02', 2, 1, 'Outdoor', 'Shaded area', 1, 1, 'System', 1, 'System', 0),
    (NULL, 'T03', 6, 1, 'Balcony', 'City view', 1, 1, 'System', 1, 'System', 0),
    (NULL, 'T04', 4, 1, 'Indoor', 'Next to the bar', 1, 1, 'System', 1, 'System', 0),
    (NULL, 'T05', 8, 1, 'Outdoor', 'Family table', 1, 1, 'System', 1, 'System', 0);
END


-- =============================================
-- Insert Sample Restaurant Tables
-- This script adds sample tables for restaurant management
-- OrganizationId is set to NULL so tables appear for all organizations
-- =============================================

-- Check if table exists before inserting
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RestaurantTables')
BEGIN
    -- Insert sample tables only if they don't already exist
    -- Using NULL OrganizationId so tables are available for all organizations
    
    -- Indoor Tables
    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T01')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T01', 4, 1, 'Indoor', 'Near the window', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T02')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T02', 2, 1, 'Indoor', 'Corner table', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T03')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T03', 4, 1, 'Indoor', 'Next to the bar', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T04')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T04', 6, 1, 'Indoor', 'Center area', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T05')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T05', 2, 1, 'Indoor', 'Quiet corner', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T06')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T06', 4, 1, 'Indoor', 'Main dining area', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T07')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T07', 8, 1, 'Indoor', 'Large group table', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T08')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T08', 4, 1, 'Indoor', 'Near entrance', 1, 1, 'System', 1, 'System', 0);
    END

    -- Outdoor Tables
    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T09')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T09', 4, 1, 'Outdoor', 'Shaded area', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T10')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T10', 2, 1, 'Outdoor', 'Garden view', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T11')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T11', 6, 1, 'Outdoor', 'Patio area', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T12')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T12', 8, 1, 'Outdoor', 'Family table', 1, 1, 'System', 1, 'System', 0);
    END

    -- Balcony Tables
    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T13')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T13', 4, 1, 'Balcony', 'City view', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T14')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T14', 2, 1, 'Balcony', 'Romantic setting', 1, 1, 'System', 1, 'System', 0);
    END

    IF NOT EXISTS (SELECT 1 FROM RestaurantTables WHERE TableNumber = 'T15')
    BEGIN
        INSERT INTO RestaurantTables (OrganizationId, TableNumber, Capacity, IsAvailable, TableLocation, Notes, IsActive, CreatedBy, CreatedFrom, UpdatedBy, UpdatedFrom, IsSoftDeleted)
        VALUES (NULL, 'T15', 6, 0, 'Balcony', 'Reserved for VIPs', 1, 1, 'System', 1, 'System', 0);
    END

    PRINT 'Sample restaurant tables inserted successfully.';
END
ELSE
BEGIN
    PRINT 'RestaurantTables table does not exist. Please run 0410_CreateRestaurantTablesTable.sql first.';
END

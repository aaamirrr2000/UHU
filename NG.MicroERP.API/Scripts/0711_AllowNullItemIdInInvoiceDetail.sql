-- =============================================
-- Script: Allow NULL ItemId in InvoiceDetail for Manual Line Items
-- Description: This script modifies the InvoiceDetail table to allow NULL values 
--              in the ItemId column to support manual line entries that don't 
--              reference an item from the Items table.
-- Date: Auto-generated
-- =============================================

-- Check if the foreign key constraint exists and drop it
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__InvoiceDe__ItemI__4707859D')
BEGIN
    ALTER TABLE dbo.InvoiceDetail
    DROP CONSTRAINT FK__InvoiceDe__ItemI__4707859D;
END

-- Check if there's a constraint with a different name (check by referenced table)
DECLARE @FKName NVARCHAR(128);
SELECT @FKName = fk.name 
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fk.parent_object_id = OBJECT_ID('dbo.InvoiceDetail') 
  AND fk.referenced_object_id = OBJECT_ID('dbo.Items')
  AND fkc.parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.InvoiceDetail') AND name = 'ItemId');

IF @FKName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.InvoiceDetail DROP CONSTRAINT ' + @FKName);
END

-- Alter the column to allow NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.InvoiceDetail') AND name = 'ItemId' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.InvoiceDetail
    ALTER COLUMN ItemId INT NULL;
END

-- Recreate the foreign key constraint (allowing NULL values)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys fk
               INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
               WHERE fk.parent_object_id = OBJECT_ID('dbo.InvoiceDetail') 
                 AND fk.referenced_object_id = OBJECT_ID('dbo.Items')
                 AND fkc.parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.InvoiceDetail') AND name = 'ItemId'))
BEGIN
    ALTER TABLE dbo.InvoiceDetail
    ADD CONSTRAINT FK_InvoiceDetail_ItemId FOREIGN KEY (ItemId) REFERENCES dbo.Items(Id);
END

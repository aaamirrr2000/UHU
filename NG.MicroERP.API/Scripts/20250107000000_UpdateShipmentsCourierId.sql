-- =============================================
-- Update Shipments Table: Replace CarrierName with CourierId
-- =============================================

-- Add CourierId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Shipments') AND name = 'CourierId')
BEGIN
    ALTER TABLE dbo.Shipments
    ADD CourierId INT NULL;
    
    -- Add foreign key constraint
    ALTER TABLE dbo.Shipments
    ADD CONSTRAINT FK_Shipments_Courier FOREIGN KEY (CourierId) REFERENCES dbo.Parties(Id);
END
GO

-- Remove CarrierName column if it exists (optional - keep for backward compatibility if needed)
-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Shipments') AND name = 'CarrierName')
-- BEGIN
--     ALTER TABLE dbo.Shipments
--     DROP COLUMN CarrierName;
-- END
-- GO


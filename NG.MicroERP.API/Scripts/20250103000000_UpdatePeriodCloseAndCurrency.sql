-- =============================================
-- Update PeriodClose Table for Module-Specific Periods
-- =============================================

-- Add ModuleType and Status columns if they don't exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PeriodClose' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'ModuleType')
    BEGIN
        -- Add as nullable first, then update, then make NOT NULL
        ALTER TABLE dbo.PeriodClose
        ADD ModuleType VARCHAR(50) NULL;
        
        -- Update existing rows
        UPDATE dbo.PeriodClose
        SET ModuleType = 'ALL'
        WHERE ModuleType IS NULL;
        
        -- Now make it NOT NULL
        ALTER TABLE dbo.PeriodClose
        ALTER COLUMN ModuleType VARCHAR(50) NOT NULL;
        
        -- Check if ModuleType column already has a default constraint
        DECLARE @ModuleTypeColumnId INT;
        DECLARE @ExistingModuleDefaultName NVARCHAR(200);
        
        SELECT @ModuleTypeColumnId = column_id 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'ModuleType';
        
        -- Find any existing default constraint on ModuleType column
        SELECT @ExistingModuleDefaultName = dc.name
        FROM sys.default_constraints dc
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.PeriodClose')
        AND dc.parent_column_id = @ModuleTypeColumnId;
        
        -- Only add named default constraint if no default exists at all
        -- (This would only happen if ModuleType was added in this script, not in CREATE TABLE)
        IF @ExistingModuleDefaultName IS NULL
        BEGIN
            -- No default exists, add our named one
            ALTER TABLE dbo.PeriodClose
            ADD CONSTRAINT DF_PeriodClose_ModuleType DEFAULT 'ALL' FOR ModuleType;
        END
        -- If a default already exists (from CREATE TABLE), leave it alone
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'Status')
    BEGIN
        -- Add Status column as nullable first
        ALTER TABLE dbo.PeriodClose
        ADD Status VARCHAR(50) NULL;
    END
END
GO

-- Migrate existing IsOpen to Status (in separate batch after column is created)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PeriodClose' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'Status')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'IsOpen')
    BEGIN
        -- Use dynamic SQL to avoid parse-time validation of IsOpen column
        DECLARE @MigrateSQL NVARCHAR(MAX);
        SET @MigrateSQL = N'UPDATE dbo.PeriodClose
                            SET Status = CASE WHEN IsOpen = 1 THEN ''OPEN'' ELSE ''CLOSE'' END
                            WHERE Status IS NULL;';
        EXEC sp_executesql @MigrateSQL;
        
        -- Set default for any remaining NULL values
        UPDATE dbo.PeriodClose
        SET Status = 'OPEN'
        WHERE Status IS NULL;
    END
    ELSE IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'Status')
    BEGIN
        -- If Status exists but IsOpen doesn't, set default for NULL values
        UPDATE dbo.PeriodClose
        SET Status = 'OPEN'
        WHERE Status IS NULL;
    END
END
GO

-- Now make Status NOT NULL if it exists and is still nullable
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PeriodClose' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'Status')
        AND (SELECT is_nullable FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'Status') = 1
    BEGIN
        -- Ensure no NULL values before making NOT NULL
        UPDATE dbo.PeriodClose
        SET Status = 'OPEN'
        WHERE Status IS NULL;
        
        -- Check if Status column already has a default constraint
        DECLARE @StatusColumnId INT;
        DECLARE @ExistingStatusDefaultName NVARCHAR(200);
        DECLARE @StatusSQL NVARCHAR(MAX);
        
        SELECT @StatusColumnId = column_id 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'Status';
        
        -- Find any existing default constraint on Status column
        SELECT @ExistingStatusDefaultName = dc.name
        FROM sys.default_constraints dc
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.PeriodClose')
        AND dc.parent_column_id = @StatusColumnId;
        
        -- Make column NOT NULL (this won't affect the default constraint)
        ALTER TABLE dbo.PeriodClose
        ALTER COLUMN Status VARCHAR(50) NOT NULL;
        
        -- Only add named default constraint if no default exists at all
        -- (This would only happen if Status was added in this script, not in CREATE TABLE)
        IF @ExistingStatusDefaultName IS NULL
        BEGIN
            -- No default exists, add our named one
            ALTER TABLE dbo.PeriodClose
            ADD CONSTRAINT DF_PeriodClose_Status DEFAULT 'OPEN' FOR Status;
        END
        -- If a default already exists (from CREATE TABLE), leave it alone
        -- Optionally, we could rename it, but it's not necessary
    END
END
GO

-- Drop CHECK constraints if they exist (to allow flexible ModuleType and Status values)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PeriodClose' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Drop ModuleType CHECK constraint if it exists
    IF EXISTS (SELECT * FROM sys.check_constraints 
               WHERE name = 'CHK_PeriodClose_ModuleType' 
               AND parent_object_id = OBJECT_ID(N'dbo.PeriodClose'))
    BEGIN
        ALTER TABLE dbo.PeriodClose
        DROP CONSTRAINT CHK_PeriodClose_ModuleType;
    END
    
    -- Drop Status CHECK constraint if it exists
    IF EXISTS (SELECT * FROM sys.check_constraints 
               WHERE name = 'CHK_PeriodClose_Status' 
               AND parent_object_id = OBJECT_ID(N'dbo.PeriodClose'))
    BEGIN
        ALTER TABLE dbo.PeriodClose
        DROP CONSTRAINT CHK_PeriodClose_Status;
    END
END
GO

-- Note: CHECK constraints removed to allow flexible ModuleType and Status values
-- The application layer handles validation instead of database constraints

-- Drop old unique constraint and add new one with ModuleType
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PeriodClose' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_PeriodClose_Org_Period' AND object_id = OBJECT_ID(N'dbo.PeriodClose'))
    BEGIN
        ALTER TABLE dbo.PeriodClose
        DROP CONSTRAINT UQ_PeriodClose_Org_Period;
    END
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'ModuleType')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_PeriodClose_Org_Period_Module' AND object_id = OBJECT_ID(N'dbo.PeriodClose'))
    BEGIN
        ALTER TABLE dbo.PeriodClose
        ADD CONSTRAINT UQ_PeriodClose_Org_Period_Module UNIQUE (OrganizationId, PeriodName, ModuleType);
    END
END
GO

-- =============================================
-- Add Currency Fields to Invoice Table
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoice' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Invoice') AND name = 'BaseCurrencyId')
    BEGIN
        ALTER TABLE dbo.Invoice
        ADD BaseCurrencyId INT NULL,
            EnteredCurrencyId INT NULL,
            ExchangeRate DECIMAL(18, 6) DEFAULT 1.000000;
        
        -- Add foreign key constraints only if Currencies table exists
        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Currencies' AND schema_id = SCHEMA_ID('dbo'))
        BEGIN
            ALTER TABLE dbo.Invoice
            ADD CONSTRAINT FK_Invoice_BaseCurrency FOREIGN KEY (BaseCurrencyId) REFERENCES dbo.Currencies(Id),
                CONSTRAINT FK_Invoice_EnteredCurrency FOREIGN KEY (EnteredCurrencyId) REFERENCES dbo.Currencies(Id);
        END
    END
END
GO

-- =============================================
-- Add Currency Fields to Cashbook Table
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Cashbook' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Cashbook') AND name = 'BaseCurrencyId')
    BEGIN
        ALTER TABLE dbo.Cashbook
        ADD BaseCurrencyId INT NULL,
            EnteredCurrencyId INT NULL,
            ExchangeRate DECIMAL(18, 6) DEFAULT 1.000000;
        
        -- Add foreign key constraints only if Currencies table exists
        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Currencies' AND schema_id = SCHEMA_ID('dbo'))
        BEGIN
            ALTER TABLE dbo.Cashbook
            ADD CONSTRAINT FK_Cashbook_BaseCurrency FOREIGN KEY (BaseCurrencyId) REFERENCES dbo.Currencies(Id),
                CONSTRAINT FK_Cashbook_EnteredCurrency FOREIGN KEY (EnteredCurrencyId) REFERENCES dbo.Currencies(Id);
        END
    END
END
GO

-- =============================================
-- Add Currency Fields to GeneralLedgerHeader Table
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'GeneralLedgerHeader' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.GeneralLedgerHeader') AND name = 'BaseCurrencyId')
    BEGIN
        ALTER TABLE dbo.GeneralLedgerHeader
        ADD BaseCurrencyId INT NULL,
            EnteredCurrencyId INT NULL;
        
        -- Add foreign key constraints only if Currencies table exists
        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Currencies' AND schema_id = SCHEMA_ID('dbo'))
        BEGIN
            ALTER TABLE dbo.GeneralLedgerHeader
            ADD CONSTRAINT FK_GeneralLedgerHeader_BaseCurrency FOREIGN KEY (BaseCurrencyId) REFERENCES dbo.Currencies(Id),
                CONSTRAINT FK_GeneralLedgerHeader_EnteredCurrency FOREIGN KEY (EnteredCurrencyId) REFERENCES dbo.Currencies(Id);
        END
    END
END
GO

-- Update indexes for period lookups
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PeriodClose' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PeriodClose_IsOpen' AND object_id = OBJECT_ID(N'dbo.PeriodClose'))
    BEGIN
        DROP INDEX IX_PeriodClose_IsOpen ON dbo.PeriodClose;
    END
END
GO

-- Drop IsOpen column after migration is complete (only if Status column exists and IsOpen still exists)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PeriodClose' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'Status')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'IsOpen')
    BEGIN
        -- Drop default constraints on IsOpen column using dynamic SQL to avoid validation errors
        DECLARE @ConstraintName NVARCHAR(200);
        DECLARE @SQL NVARCHAR(MAX);
        DECLARE @ColumnId INT;
        
        -- Get the column ID for IsOpen
        SELECT @ColumnId = column_id 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'IsOpen';
        
        IF @ColumnId IS NOT NULL
        BEGIN
            -- Drop default constraints
            DECLARE constraint_cursor CURSOR FOR
            SELECT name 
            FROM sys.default_constraints 
            WHERE parent_object_id = OBJECT_ID(N'dbo.PeriodClose') 
            AND parent_column_id = @ColumnId;
            
            OPEN constraint_cursor;
            FETCH NEXT FROM constraint_cursor INTO @ConstraintName;
            
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @SQL = 'ALTER TABLE dbo.PeriodClose DROP CONSTRAINT ' + QUOTENAME(@ConstraintName);
                EXEC sp_executesql @SQL;
                FETCH NEXT FROM constraint_cursor INTO @ConstraintName;
            END
            
            CLOSE constraint_cursor;
            DEALLOCATE constraint_cursor;
        END
        
        -- Drop the IsOpen column (SQL Server will automatically drop any remaining constraints)
        -- Use dynamic SQL to avoid parsing errors
        BEGIN TRY
            SET @SQL = 'ALTER TABLE dbo.PeriodClose DROP COLUMN IsOpen';
            EXEC sp_executesql @SQL;
        END TRY
        BEGIN CATCH
            -- If column is referenced by constraints, try to drop them first
            -- This is a fallback - most constraints should already be dropped above
            DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
            IF @ErrorMsg LIKE '%constraint%' OR @ErrorMsg LIKE '%IsOpen%'
            BEGIN
                -- Try to find and drop any remaining constraints by name pattern
                DECLARE drop_cursor CURSOR FOR
                SELECT name 
                FROM sys.objects 
                WHERE parent_object_id = OBJECT_ID(N'dbo.PeriodClose')
                AND (type = 'D' OR type = 'C')
                AND name LIKE '%IsOpen%';
                
                OPEN drop_cursor;
                FETCH NEXT FROM drop_cursor INTO @ConstraintName;
                
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    SET @SQL = 'ALTER TABLE dbo.PeriodClose DROP CONSTRAINT ' + QUOTENAME(@ConstraintName);
                    BEGIN TRY
                        EXEC sp_executesql @SQL;
                    END TRY
                    BEGIN CATCH
                        -- Ignore errors when dropping constraints
                    END CATCH
                    FETCH NEXT FROM drop_cursor INTO @ConstraintName;
                END
                
                CLOSE drop_cursor;
                DEALLOCATE drop_cursor;
                
                -- Try dropping the column again
                BEGIN TRY
                    SET @SQL = 'ALTER TABLE dbo.PeriodClose DROP COLUMN IsOpen';
                    EXEC sp_executesql @SQL;
                END TRY
                BEGIN CATCH
                    -- Log but don't fail - column might not exist or might be in use
                    PRINT 'Warning: Could not drop IsOpen column: ' + ERROR_MESSAGE();
                END CATCH
            END
        END CATCH
    END
END
GO

-- Create index if table exists and columns exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PeriodClose' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'Status')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PeriodClose') AND name = 'ModuleType')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PeriodClose_Module_Status' AND object_id = OBJECT_ID(N'dbo.PeriodClose'))
    BEGIN
        CREATE INDEX IX_PeriodClose_Module_Status ON dbo.PeriodClose(OrganizationId, ModuleType, Status, StartDate, EndDate);
    END
END
GO


-- Migration: Add TaxRuleId to Categories table
-- This allows tax rules to be applied at category level
-- Priority: Item-level TaxRuleId > Category-level TaxRuleId

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Categories') AND name = 'TaxRuleId')
BEGIN
    ALTER TABLE Categories
    ADD TaxRuleId INT NULL;
    
    -- Add foreign key constraint
    ALTER TABLE Categories
    ADD CONSTRAINT FK_Categories_TaxRule FOREIGN KEY (TaxRuleId) REFERENCES TaxRule(Id);
    
    PRINT 'TaxRuleId column added to Categories table successfully.';
END
ELSE
BEGIN
    PRINT 'TaxRuleId column already exists in Categories table.';
END
GO

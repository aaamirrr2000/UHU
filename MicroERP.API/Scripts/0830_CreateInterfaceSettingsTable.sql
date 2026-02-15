-- Migration: Create InterfaceSettings table
-- This table stores interface-related settings organized by category
-- Similar structure to SystemConfiguration but for UI/Interface settings

CREATE TABLE InterfaceSettings
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NULL DEFAULT 1,
    Category            NVARCHAR(50) NOT NULL,           -- Sale Invoice, Purchase Invoice, etc.
    SettingKey          NVARCHAR(100) NOT NULL,         -- Setting key name (e.g., TaxRuleAppliedOnInvoiceLevel)
    SettingValue        NVARCHAR(MAX) NULL,             -- Setting value (e.g., "true", "false", or other values)
    DataType            NVARCHAR(20) NOT NULL DEFAULT 'STRING', -- STRING, BOOLEAN, NUMBER, etc.
    Description         NVARCHAR(255) NULL,              -- Description of the setting
    IsActive            INT NOT NULL DEFAULT 1,
    CreatedBy           INT NULL,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedFrom         NVARCHAR(250) NULL,
    UpdatedBy           INT NULL,
    UpdatedOn           DATETIME NULL,
    UpdatedFrom         NVARCHAR(250) NULL,
    IsSoftDeleted       INT NOT NULL DEFAULT 0,
    
    CONSTRAINT UQ_InterfaceSettings_Org_Category_Key UNIQUE (OrganizationId, Category, SettingKey)
);
GO

-- Insert default Sale Invoice settings
INSERT INTO InterfaceSettings (OrganizationId, Category, SettingKey, SettingValue, DataType, Description, IsActive, CreatedBy, CreatedFrom)
VALUES
(1, 'Sale Invoice', 'TaxRuleAppliedOnInvoiceLevel', 'false', 'BOOLEAN', 'Apply tax rule at invoice level instead of item level. When enabled, taxes are calculated on the total invoice amount. When disabled, taxes are calculated per item.', 1, 1, 'System');

--(1, 'Sale Invoice', 'ShowDiscountColumn', 'true', 'BOOLEAN', 'Display discount column in invoice items grid. When enabled, users can apply discounts per line item.', 1, 1, 'System'),
--(1, 'Sale Invoice', 'AllowNegativeQuantity', 'false', 'BOOLEAN', 'Allow negative quantities in sale invoices. When enabled, users can enter negative quantities for returns or adjustments.', 1, 1, 'System'),
--(1, 'Sale Invoice', 'AutoCalculateTax', 'true', 'BOOLEAN', 'Automatically calculate taxes when items are added to invoice. When enabled, tax calculations happen automatically based on tax rules.', 1, 1, 'System'),
--(1, 'Sale Invoice', 'RequirePaymentTerms', 'false', 'BOOLEAN', 'Require payment terms selection before saving invoice. When enabled, payment terms field becomes mandatory.', 1, 1, 'System'),
--(1, 'Sale Invoice', 'DefaultInvoiceStatus', 'DRAFT', 'STRING', 'Default status for new sale invoices. Options: DRAFT, CONFIRMED, CANCELLED.', 1, 1, 'System'),
--(1, 'Sale Invoice', 'MaxInvoiceItems', '100', 'NUMBER', 'Maximum number of items allowed per invoice. Set to 0 for unlimited items.', 1, 1, 'System');

---- Insert default Purchase Invoice settings
--INSERT INTO InterfaceSettings (OrganizationId, Category, SettingKey, SettingValue, DataType, Description, IsActive, CreatedBy, CreatedFrom)
--VALUES
--(1, 'Purchase Invoice', 'TaxRuleAppliedOnInvoiceLevel', 'false', 'BOOLEAN', 'Apply tax rule at invoice level instead of item level. When enabled, taxes are calculated on the total invoice amount. When disabled, taxes are calculated per item.', 1, 1, 'System'),
--(1, 'Purchase Invoice', 'ShowDiscountColumn', 'true', 'BOOLEAN', 'Display discount column in purchase invoice items grid. When enabled, users can apply discounts per line item.', 1, 1, 'System'),
--(1, 'Purchase Invoice', 'AutoCalculateTax', 'true', 'BOOLEAN', 'Automatically calculate taxes when items are added to purchase invoice. When enabled, tax calculations happen automatically based on tax rules.', 1, 1, 'System'),
--(1, 'Purchase Invoice', 'RequireGRNReference', 'false', 'BOOLEAN', 'Require Goods Receipt Note (GRN) reference for purchase invoices. When enabled, GRN reference field becomes mandatory.', 1, 1, 'System'),
--(1, 'Purchase Invoice', 'DefaultInvoiceStatus', 'DRAFT', 'STRING', 'Default status for new purchase invoices. Options: DRAFT, CONFIRMED, CANCELLED.', 1, 1, 'System');

---- Insert default Inventory settings
--INSERT INTO InterfaceSettings (OrganizationId, Category, SettingKey, SettingValue, DataType, Description, IsActive, CreatedBy, CreatedFrom)
--VALUES
--(1, 'Inventory', 'AllowNegativeStock', 'false', 'BOOLEAN', 'Allow negative stock levels in inventory. When enabled, system allows stock to go below zero. When disabled, prevents transactions that would result in negative stock.', 1, 1, 'System'),
--(1, 'Inventory', 'AutoUpdateStockOnInvoice', 'true', 'BOOLEAN', 'Automatically update stock levels when invoices are saved. When enabled, stock is updated immediately upon invoice confirmation.', 1, 1, 'System'),
--(1, 'Inventory', 'RequireSerialNumber', 'false', 'BOOLEAN', 'Require serial number entry for serialized items. When enabled, serial number field becomes mandatory for items marked as serialized.', 1, 1, 'System'),
--(1, 'Inventory', 'ShowLowStockAlert', 'true', 'BOOLEAN', 'Display low stock alerts in inventory dashboard. When enabled, items below reorder level are highlighted.', 1, 1, 'System'),
--(1, 'Inventory', 'LowStockThreshold', '10', 'NUMBER', 'Minimum stock level threshold for low stock alerts. Items with quantity below this value will trigger alerts.', 1, 1, 'System'),
--(1, 'Inventory', 'EnableBatchTracking', 'false', 'BOOLEAN', 'Enable batch/lot number tracking for inventory items. When enabled, batch numbers can be assigned to stock movements.', 1, 1, 'System'),
--(1, 'Inventory', 'DefaultStockMovementType', 'IN', 'STRING', 'Default stock movement type for new transactions. Options: IN, OUT, TRANSFER, ADJUSTMENT.', 1, 1, 'System');

---- Insert default Accounting settings
--INSERT INTO InterfaceSettings (OrganizationId, Category, SettingKey, SettingValue, DataType, Description, IsActive, CreatedBy, CreatedFrom)
--VALUES
--(1, 'Accounting', 'AutoPostToGL', 'true', 'BOOLEAN', 'Automatically post transactions to General Ledger. When enabled, GL entries are created automatically when invoices are confirmed.', 1, 1, 'System'),
--(1, 'Accounting', 'RequireGLApproval', 'false', 'BOOLEAN', 'Require approval before posting to General Ledger. When enabled, GL entries require approval workflow.', 1, 1, 'System'),
--(1, 'Accounting', 'AllowBackdatedEntries', 'true', 'BOOLEAN', 'Allow backdated accounting entries. When enabled, users can post entries with dates in the past. When disabled, only current or future dates are allowed.', 1, 1, 'System'),
--(1, 'Accounting', 'DefaultAccountType', 'EXPENSE', 'STRING', 'Default account type for new chart of accounts entries. Options: ASSET, LIABILITY, EXPENSE, REVENUE, EQUITY.', 1, 1, 'System'),
--(1, 'Accounting', 'ShowAccountBalance', 'true', 'BOOLEAN', 'Display account balance in chart of accounts tree. When enabled, current balance is shown next to each account.', 1, 1, 'System'),
--(1, 'Accounting', 'EnableMultiCurrency', 'true', 'BOOLEAN', 'Enable multi-currency support in accounting. When enabled, transactions can be recorded in different currencies.', 1, 1, 'System'),
--(1, 'Accounting', 'DefaultCurrency', 'PKR', 'STRING', 'Default currency code for new transactions. Must match a currency defined in the system.', 1, 1, 'System'),
--(1, 'Accounting', 'FiscalYearStartMonth', '7', 'NUMBER', 'Starting month of fiscal year (1-12). Used for financial reporting periods. Example: 7 = July.', 1, 1, 'System');
--GO


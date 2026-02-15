-- Set default revenue and expense account IDs on Items where NULL (for InvoiceDetail.AccountId when placing orders).
-- 58 = REVENUE OF SALES (ITEM REVENUE), 57 = COST OF PURCHASES (ITEM EXPENSE) from ChartOfAccounts.
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Items')
BEGIN
    UPDATE Items
    SET RevenueAccountId = 58,
        ExpenseAccountId = 57,
        UpdatedOn = GETDATE()
    WHERE (RevenueAccountId IS NULL OR ExpenseAccountId IS NULL)
      AND EXISTS (SELECT 1 FROM ChartOfAccounts WHERE Id = 58)
      AND EXISTS (SELECT 1 FROM ChartOfAccounts WHERE Id = 57);
END
GO

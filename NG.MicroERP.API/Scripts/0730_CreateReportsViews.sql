-- =============================================
-- Create Reports Views
-- Note: Most reports are calculated dynamically in the service layer
-- This script is kept for future view-based optimizations if needed
-- =============================================

-- Reports are currently generated using dynamic SQL queries in ReportsService
-- Views can be added here later for performance optimization if needed

GO

-- Trial Balance View (uses existing vw_GeneralLedgerReport)
-- Account Analysis View (uses existing vw_AccountBalance)
-- P&L and Balance Sheet will be calculated from General Ledger data

GO

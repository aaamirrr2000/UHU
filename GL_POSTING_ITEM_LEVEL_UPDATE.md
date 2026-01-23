# GL Posting - Item-Level Account Mapping Update

## Overview
The General Ledger posting logic has been updated to support **item-level GL account mapping**. Each invoice item now posts to its own GL account based on the new hierarchy, enabling granular control over expense and revenue categorization.

## What Changed

### Previous Behavior (Single GL Entry)
```
PURCHASE Invoice: 100 units Paper + 50 units Pens
GL Entry:
  DEBIT:  Purchases - General   1,500  (total of all items)
  CREDIT: Accounts Payable                1,500
```

### New Behavior (Item-Level GL Entries)
```
PURCHASE Invoice: 100 units Paper + 50 units Pens
GL Entries (one per item):
  DEBIT:  Office Supplies (Paper)        1,200  (from Item.ExpenseAccountId)
  DEBIT:  Stationery Supplies (Pens)       300  (from Item.ExpenseAccountId)
  CREDIT: Accounts Payable                           1,500
```

## Account Selection Hierarchy

### For PURCHASE Invoices (Expense Accounts)
The system now follows this **4-level priority** for each invoice item:

**Level 1: Item's ExpenseAccountId** (First Priority)
- If the item has `ExpenseAccountId` configured in the Items table
- Each item type posts to its specific expense account
- Example: "Paper" → "Office Supplies", "Equipment" → "Fixed Assets"

**Level 2: Invoice's AccountId** (Second Priority)
- If the invoice has `AccountId` specified (invoice-level override)
- Used for items without configured ExpenseAccountId
- Allows per-invoice account override

**Level 3: ITEM_EXPENSE InterfaceType** (Third Priority)
- Looks for account with `InterfaceType = 'ITEM_EXPENSE'`
- Default account for items without specific mapping
- Example: "Purchases - General" account

**Level 4: EXPENSE Type** (Fourth Priority)
- Fallback to generic `Type = 'EXPENSE'` account
- Least specific, legacy behavior
- Used only if no better match found

**GL Entry Format (PURCHASE):**
```
DEBIT:   Item Expense Accounts (various)    InvoiceAmount
DEBIT:   Tax Account                         TaxAmount
DEBIT:   Service Charges Account             ChargesAmount
CREDIT:  Accounts Payable (AP)                           TotalInvoiceAmount
CREDIT:  Discount Account                                 DiscountAmount
```

### For SALE Invoices (Revenue Accounts)
The system follows the same **4-level priority** for revenue:

**Level 1: Item's RevenueAccountId** (First Priority)
- If the item has `RevenueAccountId` configured in the Items table
- Each item type posts to its specific revenue account
- Example: "Products" → "Sales Revenue", "Services" → "Service Income"

**Level 2: Invoice's AccountId** (Second Priority)
- If the invoice has `AccountId` specified (invoice-level override)
- Used for items without configured RevenueAccountId

**Level 3: ITEM_REVENUE InterfaceType** (Third Priority)
- Looks for account with `InterfaceType = 'ITEM_REVENUE'`
- Default account for items without specific mapping
- Example: "Sales Revenue - General" account

**Level 4: REVENUE Type** (Fourth Priority)
- Fallback to generic `Type = 'REVENUE'` account
- Least specific, legacy behavior

**GL Entry Format (SALE):**
```
DEBIT:   Accounts Receivable (AR)               TotalInvoiceAmount
DEBIT:   Discount Account                       DiscountAmount
CREDIT:  Item Revenue Accounts (various)                InvoiceAmount
CREDIT:  Tax Account                                     TaxAmount
CREDIT:  Service Charges Account                         ChargesAmount
```

## Database Requirements

### Items Table Updates
Ensure the Items table has these new fields:

```sql
ALTER TABLE Items ADD 
    ExpenseAccountId INT NULL,
    RevenueAccountId INT NULL;

ALTER TABLE Items ADD CONSTRAINT FK_Items_ExpenseAccount 
    FOREIGN KEY (ExpenseAccountId) REFERENCES ChartOfAccounts(Id);

ALTER TABLE Items ADD CONSTRAINT FK_Items_RevenueAccount 
    FOREIGN KEY (RevenueAccountId) REFERENCES ChartOfAccounts(Id);
```

### Chart of Accounts Requirements
Ensure these InterfaceType accounts exist:

```sql
-- ITEM_EXPENSE account (for PURCHASE invoices)
INSERT INTO ChartOfAccounts 
(Code, Name, Type, InterfaceType, OrganizationId, IsActive)
VALUES ('2610', 'Purchases - General', 'EXPENSE', 'ITEM_EXPENSE', 1, 1);

-- ITEM_REVENUE account (for SALE invoices)
INSERT INTO ChartOfAccounts 
(Code, Name, Type, InterfaceType, OrganizationId, IsActive)
VALUES ('4100', 'Sales Revenue - General', 'REVENUE', 'ITEM_REVENUE', 1, 1);
```

## Setup Instructions

### Step 1: Configure Item-Level GL Accounts
For each item, set the GL accounts:

**For purchase items:**
1. Open Items table
2. For each item, set `ExpenseAccountId` to the appropriate GL account
3. Example: "Office Paper" → ExpenseAccountId = 45 (Office Supplies account)

**For sale items:**
1. Open Items table
2. For each item, set `RevenueAccountId` to the appropriate GL account
3. Example: "Professional Services" → RevenueAccountId = 52 (Service Income account)

### Step 2: Create Fallback Accounts (Optional but Recommended)
Create default accounts for items without specific mapping:

- **ITEM_EXPENSE Account** (Code: 2610, InterfaceType: ITEM_EXPENSE)
- **ITEM_REVENUE Account** (Code: 4100, InterfaceType: ITEM_REVENUE)

### Step 3: Invoice-Level Override (Optional)
If an invoice needs to use a different account than the item's default:

1. In the invoice form, set the `AccountId` field
2. This will override the item's ExpenseAccountId/RevenueAccountId
3. Leave blank to use item-level defaults

## Examples

### Example 1: Multi-Item Purchase with Item-Level Accounts
**Invoice Details:**
- Item 1: "Office Paper" Qty=100, Amount=1,000
  - Item.ExpenseAccountId = 45 (Office Supplies)
- Item 2: "Desk Lamp" Qty=5, Amount=500
  - Item.ExpenseAccountId = 46 (Office Equipment)
- TaxAmount: 150
- InvoiceAccountId: (blank - use item-level accounts)

**GL Entry:**
```
DEBIT:   Office Supplies (Account 45)                     1,000
DEBIT:   Office Equipment (Account 46)                      500
DEBIT:   Sales Tax Payable (TAX InterfaceType)              150
CREDIT:  Accounts Payable (AP)                                        1,650
```

**Result:** Each item posts to its configured expense account!

### Example 2: Manual Item (No Item Record)
**Invoice Details:**
- Manual Item 1: Amount=800
  - No ItemId (manual entry)
  - InvoiceAccountId: (blank)
- TaxAmount: 80

**GL Entry Determination:**
1. No Item.ExpenseAccountId (no item record)
2. No Invoice.AccountId (not specified)
3. Uses ITEM_EXPENSE InterfaceType account (Purchases - General)

```
DEBIT:   Purchases - General (ITEM_EXPENSE)               800
DEBIT:   Sales Tax Payable (TAX)                           80
CREDIT:  Accounts Payable (AP)                                       880
```

### Example 3: Invoice-Level Account Override
**Invoice Details:**
- Item 1: "Office Paper" Qty=100, Amount=1,000
  - Item.ExpenseAccountId = 45 (Office Supplies)
- Item 2: "Pens" Qty=50, Amount=200
  - Item.ExpenseAccountId = 47 (Stationery)
- **InvoiceAccountId: 50 (Special Project Account)** ← Override!

**GL Entry:**
```
DEBIT:   Special Project Account (50)                     1,200  (both items use override)
CREDIT:  Accounts Payable (AP)                                       1,200
```

**Result:** Invoice-level account overrides item-level accounts!

### Example 4: Mixed Items and Manual Items
**Invoice Details:**
- Item 1: "Laptop" Qty=2, Amount=4,000
  - Item.ExpenseAccountId = 48 (Computer Equipment)
- Manual Item 2: Amount=500 (no item record)
- TaxAmount: 450

**GL Entry:**
```
DEBIT:   Computer Equipment (Account 48)                  4,000
DEBIT:   Purchases - General (ITEM_EXPENSE - fallback)      500
DEBIT:   Sales Tax Payable (TAX)                            450
CREDIT:  Accounts Payable (AP)                                       4,950
```

**Result:** Each item type uses its appropriate account!

## Technical Implementation Details

### Code Changes in GeneralLedgerService.cs

The GL posting logic now:

1. **Fetches invoice items** with their GL account mappings:
   ```sql
   SELECT ii.Id, ii.ItemId, ii.Amount, i.ExpenseAccountId, i.RevenueAccountId
   FROM InvoiceItems ii
   LEFT JOIN Items i ON ii.ItemId = i.Id
   WHERE ii.InvoiceId = @invoiceId
   ```

2. **Posts each item separately** with its GL account:
   ```csharp
   foreach (var item in invoiceItems)
   {
       // Determine expense account using 4-level priority
       int accountId = item.ExpenseAccountId ?? invoice.AccountId ?? 
                       ITEM_EXPENSE_InterfaceType ?? EXPENSE_Type;
       
       // Add GL entry for this item
       glHeader.Details.Add(new GeneralLedgerDetailModel
       {
           AccountId = accountId,
           DebitAmount = item.Amount,
           Description = $"Purchase - {item.ItemCode} - Invoice {invoice.Code}"
       });
   }
   ```

3. **Applies tax/charges/discounts** at invoice level (as before)

### GL Entry Description Format

**PURCHASE Items:**
```
"Purchase - [ItemCode] - Invoice INV-001"
Example: "Purchase - PAPER-100 - Invoice INV-001"
```

**SALE Items:**
```
"Sales - [ItemCode] - Invoice INV-001"
Example: "Sales - SERVICE-PRO - Invoice INV-001"
```

**Manual Items:**
```
"Purchase - Manual Item - Invoice INV-001"
"Sales - Manual Item - Invoice INV-001"
```

## Migration Guide (If Upgrading)

### For Existing Invoices
**Items already posted to GL will show:**
- ✅ Single GL entry per item type (new behavior)
- ✅ Correct GL accounts (based on item mapping)
- ✅ Better GL reporting and audit trail

**To re-post existing invoices:**
1. Reverse the GL entry (if needed)
2. Update Item.ExpenseAccountId/RevenueAccountId values
3. Re-post to GL

### For New Invoices
**All new invoices will automatically use:**
- ✅ Item-level GL accounts (if configured)
- ✅ Invoice-level account override (if specified)
- ✅ Fallback to ITEM_EXPENSE/ITEM_REVENUE (if item not configured)

## Backward Compatibility

**If Item GL accounts are not configured:**
- System falls back to ITEM_EXPENSE/ITEM_REVENUE accounts
- If those don't exist, uses generic EXPENSE/REVENUE
- Existing functionality continues to work
- Highly recommended to configure item accounts for better control

## Troubleshooting

### GL Entry showing wrong account
**Check the priority order (in sequence):**
1. Does the item have ExpenseAccountId/RevenueAccountId? → This is used
2. Does the invoice have AccountId? → This overrides item account
3. Do ITEM_EXPENSE/ITEM_REVENUE accounts exist? → These are next
4. Is there a generic EXPENSE/REVENUE account? → This is fallback

### GL entry not posting
**Possible causes:**
1. No valid GL account found in any priority level
2. GL account is inactive (IsActive = 0)
3. Debit ≠ Credit after posting

**Solution:**
1. Verify ITEM_EXPENSE/ITEM_REVENUE accounts exist and are active
2. Verify item-level GL accounts are valid
3. Check invoice tax/charges/discount calculations

### Different items posting to same account
**This is correct if:**
1. Items have the same ExpenseAccountId/RevenueAccountId
2. Items don't have GL accounts configured (using fallback)
3. All items use invoice-level account override

## Version Information
- **Updated:** GeneralLedgerService.cs
- **Change Date:** January 21, 2026
- **Affects:** All invoice GL posting (PURCHASE and SALE)
- **Backward Compatible:** Yes (with fallback accounts)

## Key Benefits

✅ **Item-Level Control** - Each item posts to correct GL account  
✅ **Professional GL Entries** - No more "Purchases - General" for everything  
✅ **Better Financial Reporting** - Clear categorization by item type  
✅ **Audit Trail** - GL descriptions include item codes  
✅ **Flexibility** - Item override or invoice-level override available  
✅ **Backward Compatible** - Works with or without item accounts configured  

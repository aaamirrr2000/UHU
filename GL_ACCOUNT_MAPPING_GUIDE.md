# GL Account Mapping Guide

## Overview
The General Ledger posting now uses an intelligent account selection hierarchy based on the `InterfaceType` field in the Chart of Accounts table. This provides flexibility for organizations while maintaining backwards compatibility with existing setups.

## InterfaceType Values

### Required InterfaceType Values
Create accounts in Chart of Accounts with these `InterfaceType` values:

| InterfaceType | Purpose | Used For | Example Account |
|---------------|---------|----------|-----------------|
| `ITEM_EXPENSE` | Manual purchase item expenses | PURCHASE invoices | Cost of Manual Purchases |
| `ITEM_REVENUE` | Manual sale item revenue | SALE invoices | Revenue from Manual Sales |
| `TAX` | Tax liability/expense | Both PURCHASE & SALE | Sales Tax Payable |
| `SERVICE` | Service charges | Both PURCHASE & SALE | Service Charges Expense |
| `DISCOUNT` | Discount given/received | Both PURCHASE & SALE | Discounts Allowed |

## Account Selection Priority

### For PURCHASE Invoices (Manual Item Expenses)
The system now uses this priority order to select the expense account:

1. **Invoice AccountId** (First Priority)
   - Uses the `AccountId` field from the invoice if specified
   - Allows flexibility for different expense accounts per invoice
   - Example: One invoice uses "Office Supplies", another uses "Equipment"

2. **ITEM_EXPENSE InterfaceType** (Second Priority)
   - Looks for an account with `InterfaceType = 'ITEM_EXPENSE'`
   - Serves as default for all manual purchase items
   - Example: Single account for all miscellaneous purchase expenses

3. **EXPENSE Type** (Third Priority - Fallback)
   - Falls back to generic `Type = 'EXPENSE'` accounts
   - Legacy behavior for backward compatibility
   - Least specific, used if no better match found

**GL Entry Format (PURCHASE):**
```
DEBIT:   Expense (InvoiceAmount) + Tax (TaxAmount) + Charges (ChargesAmount)
CREDIT:  AP (TotalInvoiceAmount) + Discount (DiscountAmount)
```

### For SALE Invoices (Manual Item Revenue)
The system uses the same priority order for revenue account selection:

1. **Invoice AccountId** (First Priority)
   - Allows different revenue accounts per invoice
   - Example: One invoice uses "Product Sales", another uses "Service Revenue"

2. **ITEM_REVENUE InterfaceType** (Second Priority)
   - Looks for an account with `InterfaceType = 'ITEM_REVENUE'`
   - Default for all manual sale items
   - Example: Single account for miscellaneous manual sales

3. **REVENUE Type** (Third Priority - Fallback)
   - Falls back to generic `Type = 'REVENUE'` accounts
   - Legacy behavior for backward compatibility

**GL Entry Format (SALE):**
```
DEBIT:   AR (TotalInvoiceAmount) + Discount (DiscountAmount)
CREDIT:  Revenue (InvoiceAmount) + Tax (TaxAmount) + Charges (ChargesAmount)
```

## Setup Instructions

### Step 1: Create ITEM_EXPENSE and ITEM_REVENUE Accounts

In your Chart of Accounts, create:

**ITEM_EXPENSE Account:**
- Code: `2600` (or your convention)
- Name: `Cost of Manual Purchases`
- Type: `EXPENSE`
- InterfaceType: `ITEM_EXPENSE` ← **Important**
- Description: `Default account for manual purchase items in invoices`

**ITEM_REVENUE Account:**
- Code: `4100` (or your convention)
- Name: `Revenue from Manual Sales`
- Type: `REVENUE`
- InterfaceType: `ITEM_REVENUE` ← **Important**
- Description: `Default account for manual sale items in invoices`

### Step 2: (Optional) Use Per-Invoice Account Override

If you want to use a different account for a specific invoice:

1. In the invoice form, select the desired account in the `AccountId` field
2. When GL posting occurs, this account will be used instead of ITEM_EXPENSE/ITEM_REVENUE
3. Leave blank to use the default ITEM_EXPENSE/ITEM_REVENUE account

### Step 3: Verify GL Posting

After creating accounts:
1. Create a test invoice with manual items
2. Post the invoice to GL
3. Verify GL entry uses the correct expense/revenue account
4. Check that total debits = total credits

## Examples

### Example 1: Standard Manual Purchase (using defaults)
**Invoice Details:**
- Invoice Code: INV-001
- InvoiceAmount: 1,000
- TaxAmount: 100
- AccountId: (blank - will use default)

**GL Entry:**
```
DEBIT:   Cost of Manual Purchases (ITEM_EXPENSE)   1,000
DEBIT:   Sales Tax Payable (TAX)                      100
CREDIT:  Accounts Payable (AP)                                1,100
```

### Example 2: Equipment Purchase (using specific account)
**Invoice Details:**
- Invoice Code: INV-002
- InvoiceAmount: 5,000
- TaxAmount: 500
- AccountId: 45 (Office Equipment account)

**GL Entry:**
```
DEBIT:   Office Equipment (Account 45)               5,000
DEBIT:   Sales Tax Payable (TAX)                        500
CREDIT:  Accounts Payable (AP)                                5,500
```

### Example 3: Service Sale with Discount
**Invoice Details:**
- Invoice Code: INV-003
- InvoiceAmount: 2,000
- DiscountAmount: 200
- AccountId: (blank - will use default)

**GL Entry:**
```
DEBIT:   Accounts Receivable (AR)                    2,000
DEBIT:   Discounts Allowed (DISCOUNT)                  200
CREDIT:  Revenue from Manual Sales (ITEM_REVENUE)              2,000
CREDIT:  Discounts Received (DISCOUNT)                           200
```

## Backward Compatibility

If you don't create ITEM_EXPENSE and ITEM_REVENUE accounts:
- The system will fall back to generic EXPENSE/REVENUE accounts
- Existing functionality will continue to work
- Consider creating these accounts for better control and clarity

## Database Migration

If migrating from an older system, you can run this SQL to create the default accounts:

```sql
-- Create ITEM_EXPENSE account (if needed)
INSERT INTO ChartOfAccounts 
(Code, Name, Type, InterfaceType, OrganizationId, IsActive)
VALUES ('2600', 'Cost of Manual Purchases', 'EXPENSE', 'ITEM_EXPENSE', 1, 1);

-- Create ITEM_REVENUE account (if needed)
INSERT INTO ChartOfAccounts 
(Code, Name, Type, InterfaceType, OrganizationId, IsActive)
VALUES ('4100', 'Revenue from Manual Sales', 'REVENUE', 'ITEM_REVENUE', 1, 1);
```

## Troubleshooting

### GL Entry not posting
**Symptom:** GL entry shows debit ≠ credit

**Possible causes:**
1. Account not found for selected InterfaceType
2. AccountId invalid or inactive
3. No fallback account exists

**Solution:**
1. Verify ITEM_EXPENSE/ITEM_REVENUE accounts exist
2. Verify accounts are marked as Active
3. Check Account IDs are valid

### Wrong account being used
**Symptom:** GL entry posts to unexpected account

**Check order (in sequence):**
1. Does invoice have AccountId specified? → This takes priority
2. Does organization have ITEM_EXPENSE/ITEM_REVENUE account? → This is used next
3. Generic EXPENSE/REVENUE account exists? → Used as fallback

### Tax not posting correctly
**Symptom:** Tax amount not appearing in GL

**Solution:**
1. Verify TAX InterfaceType account exists
2. Check account is Active
3. Verify invoice TaxAmount > 0

## Key Differences from Previous Version

| Aspect | Previous | Current |
|--------|----------|---------|
| Account Selection | Hardcoded generic EXPENSE lookup | Flexible priority hierarchy |
| Manual Items | Always used EXPENSE/REVENUE | Can use ITEM_EXPENSE/ITEM_REVENUE |
| Invoice Override | Not possible | Use AccountId field |
| Backward Compatible | N/A | Yes, falls back to generic |

## Version Information
- Updated: GeneralLedgerService.cs
- Change Date: January 21, 2026
- Affects: PURCHASE and SALE invoice GL posting

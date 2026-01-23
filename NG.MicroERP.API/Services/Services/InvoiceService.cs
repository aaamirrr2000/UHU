using Newtonsoft.Json;
using Serilog;

using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

namespace NG.MicroERP.API.Services;

public interface IInvoiceService
{
    Task<(bool, List<InvoicesAllModel>)>? Search(string Criteria = "");
    Task<(bool, InvoicesModel?)>? Get(int id);
    Task<(bool, InvoiceModel, string)> Put(InvoicesModel obj);
    Task<(bool, InvoiceModel, string)> Post(InvoicesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(InvoiceModel obj);
    Task<(bool, List<BillMasterReportModel>)>? GetInvoiceReport(int id);
    Task<(bool, InvoiceCompleteReportModel?)>? GetInvoiceCompleteReport(int id);
	Task<(bool, string)> ClientComments(InvoicesModel obj);
    Task<(bool, string)> InvoiceStatus(int InvoiceId, string Status, int SoftDelete = 0);
}


public class InvoiceService : IInvoiceService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<InvoicesAllModel>)>? Search(string Criteria = "")
    {
        string SQL = @"SELECT 
            i.Id AS ID,
            i.Code,
            i.InvoiceType,
            i.Source,
            i.SalesId,
            ISNULL(e.Fullname, '') AS Fullname,
            0 AS TableId,
            '' AS TableName,
            i.LocationId,
            ISNULL(loc.Name, '') AS Location,
            i.PartyId,
            ISNULL(p.Name, '') AS Party,
            ISNULL(i.PartyName, ISNULL(p.Name, '')) AS PartyName,
            NULL AS ScenarioId,
            ISNULL(i.TranDate, i.CreatedOn) AS TranDate,
            i.AccountId,
            ISNULL(coa.Name, '') AS AccountName,
            0 AS PreprationTime,
            ISNULL((
                SELECT SUM((id.UnitPrice * id.Qty) - id.DiscountAmount + ISNULL(tax.TaxAmount, 0))
                FROM InvoiceDetail id
                LEFT JOIN (SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount FROM InvoiceDetailTax GROUP BY InvoiceDetailId) tax ON id.Id = tax.InvoiceDetailId
                WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0
            ), 0) AS SubTotalAmount,
            ISNULL((
                SELECT SUM(ic.AppliedAmount)
                FROM InvoiceCharges ic
                WHERE ic.InvoiceId = i.Id AND ic.ChargeCategory = 'SERVICE' AND ic.IsSoftDeleted = 0
            ), 0) AS TotalChargeAmount,
            ISNULL((
                SELECT SUM(ic.AppliedAmount)
                FROM InvoiceCharges ic
                WHERE ic.InvoiceId = i.Id AND ic.ChargeCategory = 'DISCOUNT' AND ic.IsSoftDeleted = 0
            ), 0) AS DiscountAmount,
            (ISNULL((
                SELECT SUM((id.UnitPrice * id.Qty) - id.DiscountAmount + ISNULL(tax.TaxAmount, 0))
                FROM InvoiceDetail id
                LEFT JOIN (SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount FROM InvoiceDetailTax GROUP BY InvoiceDetailId) tax ON id.Id = tax.InvoiceDetailId
                WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0
            ), 0) 
            + ISNULL((
                SELECT SUM(ic.AppliedAmount)
                FROM InvoiceCharges ic
                WHERE ic.InvoiceId = i.Id AND ic.ChargeCategory = 'SERVICE' AND ic.IsSoftDeleted = 0
            ), 0)
            - ISNULL((
                SELECT SUM(ic.AppliedAmount)
                FROM InvoiceCharges ic
                WHERE ic.InvoiceId = i.Id AND ic.ChargeCategory = 'DISCOUNT' AND ic.IsSoftDeleted = 0
            ), 0)) AS BillAmount,
            ISNULL((SELECT SUM(Amount) FROM InvoicePayments WHERE InvoiceId = i.Id AND IsSoftDeleted = 0), 0) AS TotalPaidAmount,
            ((ISNULL((
                SELECT SUM((id.UnitPrice * id.Qty) - id.DiscountAmount + ISNULL(tax.TaxAmount, 0))
                FROM InvoiceDetail id
                LEFT JOIN (SELECT InvoiceDetailId, SUM(TaxAmount) AS TaxAmount FROM InvoiceDetailTax GROUP BY InvoiceDetailId) tax ON id.Id = tax.InvoiceDetailId
                WHERE id.InvoiceId = i.Id AND id.IsSoftDeleted = 0
            ), 0) 
            + ISNULL((
                SELECT SUM(ic.AppliedAmount)
                FROM InvoiceCharges ic
                WHERE ic.InvoiceId = i.Id AND ic.ChargeCategory = 'SERVICE' AND ic.IsSoftDeleted = 0
            ), 0)
            - ISNULL((
                SELECT SUM(ic.AppliedAmount)
                FROM InvoiceCharges ic
                WHERE ic.InvoiceId = i.Id AND ic.ChargeCategory = 'DISCOUNT' AND ic.IsSoftDeleted = 0
            ), 0))
            - ISNULL((SELECT SUM(Amount) FROM InvoicePayments WHERE InvoiceId = i.Id AND IsSoftDeleted = 0), 0)) AS BalanceAmount,
            i.Description,
            i.CreatedBy,
            i.CreatedOn,
            ISNULL(i.Status, '') AS Status,
            ISNULL(u.Username, '') AS Username,
            ISNULL(i.ClientComments, '') AS ClientComments,
            i.BaseCurrencyId,
            i.EnteredCurrencyId,
            ISNULL(i.ExchangeRate, 1.0) AS ExchangeRate,
            ISNULL(c.Code, '') AS CurrencyCode,
            ISNULL(c.Name, '') AS CurrencyName
        FROM Invoice i
        LEFT JOIN Parties p ON p.Id = i.PartyId
        LEFT JOIN Locations loc ON loc.Id = i.LocationId
        LEFT JOIN Employees e ON e.Id = i.SalesId
        LEFT JOIN Users u ON u.Id = i.CreatedBy
        LEFT JOIN Currencies c ON c.Id = i.EnteredCurrencyId
        LEFT JOIN ChartOfAccounts coa ON coa.Id = i.AccountId
        WHERE i.IsSoftDeleted = 0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " AND " + Criteria;

        SQL += " ORDER BY i.Id DESC";

        List<InvoicesAllModel> result = (await dapper.SearchByQuery<InvoicesAllModel>(SQL)) ?? new List<InvoicesAllModel>();
        // Always return true with the result (even if empty) so API returns Ok with empty list instead of NotFound
        return (true, result);
    }

    // ===== Helpers to read properties safely from model objects =====
    private static int GetIntProp(object? obj, params string[] names)
    {
        if (obj == null) return 0;
        foreach (var n in names)
        {
            var p = obj.GetType().GetProperty(n);
            if (p == null) continue;
            var v = p.GetValue(obj);
            if (v == null) continue;
            if (v is int i) return i;
            if (int.TryParse(v.ToString(), out i)) return i;
        }
        return 0;
    }

    private static double GetDoubleProp(object? obj, params string[] names)
    {
        if (obj == null) return 0;
        foreach (var n in names)
        {
            var p = obj.GetType().GetProperty(n);
            if (p == null) continue;
            var v = p.GetValue(obj);
            if (v == null) continue;
            if (v is double d) return d;
            if (v is float f) return Convert.ToDouble(f);
            if (v is decimal dec) return Convert.ToDouble(dec);
            if (double.TryParse(v.ToString(), out d)) return d;
        }
        return 0;
    }

    private static string GetStringProp(object? obj, params string[] names)
    {
        if (obj == null) return string.Empty;
        foreach (var n in names)
        {
            var p = obj.GetType().GetProperty(n);
            if (p == null) continue;
            var v = p.GetValue(obj);
            if (v == null) continue;
            return v.ToString() ?? string.Empty;
        }
        return string.Empty;
    }

    // Helper to sanitize strings for inline SQL (existing codebase uses this pattern).
    private static string S(string? input) => (input ?? string.Empty).Replace("'", "''").ToUpper();

    public async Task<(bool, InvoicesModel?)> Get(int id)
    {
        try
        {
            InvoicesModel result = new();

            // Invoice header
            List<InvoiceModel> invoiceHeader = await dapper.SearchByQuery<InvoiceModel>($"SELECT * FROM Invoice WHERE Id={id}") ?? new List<InvoiceModel>();

            // Invoice details - first get all details, then populate ItemName
            List<InvoiceItemReportModel> invoiceDetails = await dapper.SearchByQuery<InvoiceItemReportModel>($"SELECT * FROM InvoiceDetail WHERE InvoiceId={id} AND IsSoftDeleted=0 ORDER BY Id") ?? new List<InvoiceItemReportModel>();
            
            // Populate ItemName and other fields from Items table
            if (invoiceDetails != null && invoiceDetails.Count > 0)
            {
                // Get all unique item IDs
                var itemIds = invoiceDetails.Where(d => d.ItemId > 0).Select(d => d.ItemId).Distinct().ToList();
                
                if (itemIds.Any())
                {
                    // Fetch all items in one query
                    string itemIdsList = string.Join(",", itemIds);
                    var items = await dapper.SearchByQuery<ItemsModel>($"SELECT Id, Name, StockType, Code FROM Items WHERE Id IN ({itemIdsList})") ?? new List<ItemsModel>();
                    
                    // Create a dictionary for quick lookup
                    var itemsDict = items.ToDictionary(i => i.Id, i => i);
                    
                    // Populate ItemName and other fields for each detail
                    foreach (var detail in invoiceDetails)
                    {
                        if (detail.ItemId > 0 && itemsDict.ContainsKey(detail.ItemId))
                        {
                            var item = itemsDict[detail.ItemId];
                            if (string.IsNullOrWhiteSpace(detail.ItemName))
                            {
                                detail.ItemName = item.Name ?? detail.ItemName ?? string.Empty;
                            }
                            detail.IsInventoryItem = (item.StockType?.Equals("ITEM", StringComparison.OrdinalIgnoreCase) ?? false);
                            if (string.IsNullOrWhiteSpace(detail.SeqNo))
                            {
                                detail.SeqNo = item.Code ?? string.Empty;
                            }
                        }
                        else if (detail.ItemId == 0 || string.IsNullOrWhiteSpace(detail.ItemName))
                        {
                            // For manual items (ItemId = 0), use ManualItem as ItemName
                            // For catalog items not found, also use ManualItem as fallback
                            if (!string.IsNullOrWhiteSpace(detail.ManualItem))
                            {
                                detail.ItemName = detail.ManualItem;
                            }
                            else if (string.IsNullOrWhiteSpace(detail.ItemName))
                            {
                                detail.ItemName = string.Empty;
                            }
                        }
                        
                        // Calculate TaxAmount if not already set
                        if (detail.TaxAmount == 0)
                        {
                            int detailId = GetIntProp(detail, "Id", "InvoiceDetailId");
                            if (detailId > 0)
                            {
                                var taxes = await dapper.SearchByQuery<InvoiceDetailTaxesModel>($"SELECT SUM(TaxAmount) AS TaxAmount FROM InvoiceDetailTax WHERE InvoiceDetailId={detailId}");
                                if (taxes != null && taxes.Any())
                                {
                                    detail.TaxAmount = GetDoubleProp(taxes.First(), "TaxAmount");
                                }
                            }
                        }
                        
                        // Calculate ItemTotalAmount if not already set
                        if (detail.ItemTotalAmount == 0)
                        {
                            detail.ItemTotalAmount = (detail.Qty * detail.UnitPrice) - detail.DiscountAmount + detail.TaxAmount;
                        }
                    }
                }
            }

            // Charges
            List<InvoiceChargesModel> invoiceCharges = await dapper.SearchByQuery<InvoiceChargesModel>($"SELECT * FROM InvoiceCharges WHERE InvoiceId={id}") ?? new List<InvoiceChargesModel>();

            // Payments
            List<InvoicePaymentModel> invoicePayments = await dapper.SearchByQuery<InvoicePaymentModel>($"SELECT * FROM InvoicePayments WHERE InvoiceId={id}") ?? new List<InvoicePaymentModel>();

            // Taxes for all details (InvoiceDetailTax)
            List<InvoiceDetailTaxesModel> invoiceDetailTaxes = new();
            foreach (var det in invoiceDetails)
            {
                // use whichever id field exists on the detail model
                int detailId = GetIntProp(det, "Id", "InvoiceDetailId");
                if (detailId == 0) continue;

                var taxes = await dapper.SearchByQuery<InvoiceDetailTaxesModel>($"SELECT * FROM InvoiceDetailTax WHERE InvoiceDetailId={detailId}") ?? new List<InvoiceDetailTaxesModel>();
                if (taxes.Any())
                    invoiceDetailTaxes.AddRange(taxes);
            }

            var invoice = invoiceHeader.FirstOrDefault();
            
            // If invoice doesn't exist, return false
            if (invoice == null || invoice.Id == 0)
            {
                return (false, null);
            }
            
            result.Invoice = invoice;
            result.InvoiceDetails = new ObservableCollection<InvoiceItemReportModel>(invoiceDetails);
            result.InvoiceCharges = new ObservableCollection<InvoiceChargesModel>(invoiceCharges);
            result.InvoicePayments = new ObservableCollection<InvoicePaymentModel>(invoicePayments);
            result.InvoiceTaxes = new ObservableCollection<InvoiceDetailTaxesModel>(invoiceDetailTaxes);

            return (true, result);
        }
        catch (Exception ex)
        {
            // Consider logging ex.Message
            return (false, null);
        }
    }

    public async Task<(bool, InvoiceModel, string)> Post(InvoicesModel obj)
    {
        try
        {
            // Validate period for invoice creation - use SALE or PURCHASE module type
            if (obj.Invoice.TranDate.HasValue)
            {
                string moduleType = obj.Invoice.InvoiceType?.ToUpper() == "PURCHASE" ? "PURCHASE" : "SALE";
                var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.Invoice.OrganizationId, obj.Invoice.TranDate.Value, moduleType);
                if (!periodCheck.Item1)
                {
                    return (false, null, periodCheck.Item2);
                }
            }

            // Check stock availability for sale invoices (skip if user confirmed to proceed)
            if (obj.Invoice.InvoiceType?.ToUpper() == "SALE" && obj.Invoice.LocationId > 0 && !obj.Invoice.SkipStockValidation)
            {
                var itemsToCheck = obj.InvoiceDetails?.Select(d => (
                    GetIntProp(d, "ItemId"),
                    GetDoubleProp(d, "Qty", "Quantity"),
                    S(GetStringProp(d, "StockCondition"))
                )).ToList() ?? new List<(int, double, string)>();

                var stockCheck = await Helper.InventoryHelper.CheckStockAvailability(dapper, obj.Invoice.OrganizationId, obj.Invoice.LocationId, itemsToCheck);
                if (!stockCheck.Item1)
                {
                    return (false, null, stockCheck.Item2);
                }
            }

            // Generate invoice code using DB helper; use column "Code"
            string code = dapper.GetCode("", "Invoice", "Code", 9) ?? string.Empty;

            // Insert invoice header (use columns that exist in DB)
            string tranDateStr = obj.Invoice.TranDate.HasValue 
                ? obj.Invoice.TranDate.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                : DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            
            int baseCurrencyId = GetIntProp(obj.Invoice, "BaseCurrencyId");
            int enteredCurrencyId = GetIntProp(obj.Invoice, "EnteredCurrencyId");
            double exchangeRate = GetDoubleProp(obj.Invoice, "ExchangeRate");
            if (exchangeRate == 0) exchangeRate = 1.0; // Default to 1.0 if not provided
            
            int accountId = GetIntProp(obj.Invoice, "AccountId");
            
            // Convert SalesId and LocationId to NULL if 0 or invalid
            string salesIdValue = (obj.Invoice.SalesId > 0) ? obj.Invoice.SalesId.ToString() : "NULL";
            string locationIdValue = (obj.Invoice.LocationId > 0) ? obj.Invoice.LocationId.ToString() : "NULL";
            
            string SQLInsert = $@"
                INSERT INTO Invoice (OrganizationId, Code, InvoiceType, Source, SalesId, LocationId, PartyId, AccountId, PartyName, PartyPhone, PartyEmail,
                PartyAddress, TranDate, Description, Status, CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted, BaseCurrencyId, EnteredCurrencyId, ExchangeRate)
                VALUES ({obj.Invoice.OrganizationId}, '{S(code)}', '{S(obj.Invoice.InvoiceType)}', '{S(obj.Invoice.Source)}', {salesIdValue},
                {locationIdValue}, {obj.Invoice.PartyId}, {(accountId > 0 ? accountId.ToString() : "NULL")}, '{S(obj.Invoice.PartyName)}', '{S(obj.Invoice.PartyPhone)}',
                '{S(obj.Invoice.PartyEmail)}', '{S(obj.Invoice.PartyAddress)}',
                '{tranDateStr}',
                '{S(obj.Invoice.Description)}',
                '{S(obj.Invoice.Status)}', {obj.Invoice.CreatedBy}, '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}',
                '{S(obj.Invoice.CreatedFrom)}', {obj.Invoice.IsSoftDeleted}, 
                {(baseCurrencyId > 0 ? baseCurrencyId.ToString() : "NULL")}, 
                {(enteredCurrencyId > 0 ? enteredCurrencyId.ToString() : "NULL")}, 
                {exchangeRate.ToString(CultureInfo.InvariantCulture)})";

            var res = await dapper.Insert(SQLInsert);
            if (!res.Item1)
            {
                string errorMsg = res.Item3 ?? "Invoice Save Error";
                return (false, null, errorMsg);
            }

            int insertedInvoiceId = res.Item2;

            // Insert invoice details and capture inserted ids
            var detailMappings = new List<(InvoiceItemReportModel detail, int insertedId)>();
            foreach (var detail in obj.InvoiceDetails ?? new ObservableCollection<InvoiceItemReportModel>())
            {
                // read fields defensively from the detail object
                int itemId = GetIntProp(detail, "ItemId");
                double qty = GetDoubleProp(detail, "Qty", "Quantity");
                double unitPrice = GetDoubleProp(detail, "UnitPrice", "Price");
                double discountAmt = GetDoubleProp(detail, "DiscountAmount");
                int rating = GetIntProp(detail, "Rating");
                int isSoftDeleted = GetIntProp(detail, "IsSoftDeleted");

                string descr = S(GetStringProp(detail, "Description", "Instructions"));
                string stockCond = S(GetStringProp(detail, "StockCondition"));
                string manualItem = S(GetStringProp(detail, "ManualItem"));
                string serving = S(GetStringProp(detail, "ServingSize"));
                string status = S(GetStringProp(detail, "Status"));
                
                // For manual items (ItemId = 0), save ItemName to ManualItem (convert to uppercase)
                if (itemId == 0)
                {
                    string itemName = S(GetStringProp(detail, "ItemName"));
                    if (!string.IsNullOrWhiteSpace(itemName))
                    {
                        manualItem = itemName.ToUpperInvariant();
                    }
                }
                
                // Convert ManualItem to uppercase if it exists
                if (!string.IsNullOrWhiteSpace(manualItem))
                {
                    manualItem = manualItem.ToUpperInvariant();
                }

                // Convert ItemId = 0 to NULL for manual entries (foreign key constraint)
                string itemIdValue = itemId > 0 ? itemId.ToString() : "NULL";

                string detailInsert = $@"
                    INSERT INTO InvoiceDetail (ItemId, StockCondition, ManualItem, ServingSize, Qty, UnitPrice, DiscountAmount,
                    InvoiceId, Description, Status, Rating, TranDate, IsSoftDeleted)
                    VALUES ({itemIdValue}, '{stockCond}', '{manualItem}', '{serving}', {qty.ToString(CultureInfo.InvariantCulture)},
                    {unitPrice.ToString(CultureInfo.InvariantCulture)}, {discountAmt.ToString(CultureInfo.InvariantCulture)}, {insertedInvoiceId}, '{descr}',
                    '{status}', {rating}, '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {isSoftDeleted})";

                var detRes = await dapper.Insert(detailInsert);
                if (!detRes.Item1)
                {
                    string errorMsg = detRes.Item3 ?? $"Failed to insert invoice detail for item {itemId}";
                    return (false, null, errorMsg);
                }
                int insertedDetailId = detRes.Item2;
                detailMappings.Add((detail, insertedDetailId));
            }

            // Insert charges (ensure InvoiceId references inserted invoice)
            foreach (var charge in obj.InvoiceCharges ?? new ObservableCollection<InvoiceChargesModel>())
            {
                // Validate charge data before inserting
                if (charge.RulesId == 0 || charge.AccountId == 0)
                {
                    return (false, null, $"Invalid charge data: RulesId={charge.RulesId}, AccountId={charge.AccountId}. Charge category: {S(charge.ChargeCategory)}");
                }
                
                if (string.IsNullOrWhiteSpace(charge.ChargeCategory) || string.IsNullOrWhiteSpace(charge.AmountType))
                {
                    return (false, null, $"Invalid charge data: ChargeCategory or AmountType is missing. Charge category: {S(charge.ChargeCategory)}");
                }
                
                string chargeInsert = $@"
                    INSERT INTO InvoiceCharges
                    (InvoiceId, RulesId, AccountId, ChargeCategory, AmountType, Amount, AppliedAmount, IsSoftDeleted)
                    VALUES ({insertedInvoiceId}, {charge.RulesId}, {charge.AccountId}, '{S(charge.ChargeCategory)}',
                    '{S(charge.AmountType)}', {charge.Amount.ToString(CultureInfo.InvariantCulture)}, {charge.AppliedAmount.ToString(CultureInfo.InvariantCulture)}, {charge.IsSoftDeleted});";
                var chargeRes = await dapper.Insert(chargeInsert);
                if (!chargeRes.Item1)
                {
                    string errorMsg = chargeRes.Item3 ?? $"Failed to insert invoice charge: {S(charge.ChargeCategory)}";
                    return (false, null, errorMsg);
                }
            }

            // Insert payments (use AccountId and Amount)
            foreach (var payment in obj.InvoicePayments ?? new ObservableCollection<InvoicePaymentModel>())
            {
                // defensive: skip zero/empty payments
                double amount = Convert.ToDouble(payment.Amount);
                if (Math.Abs(amount) < 0.0000001) continue;

                int paymentAccountId = GetIntProp(payment, "AccountId");
                // If paymentAccountId is missing, fail fast and return a helpful error instead of attempting invalid insert
                if (paymentAccountId == 0)
                {
                    return (false, null, $"Payment requires a valid AccountId. PaymentRef='{GetStringProp(payment, "PaymentRef")}', Amount={amount:F2}");
                }

                string paymentRef = S(GetStringProp(payment, "PaymentRef"));
                string notes = S(GetStringProp(payment, "Notes"));
                string paidOnStr = payment.PaidOn.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                string paymentInsert = $@"
                    INSERT INTO InvoicePayments (InvoiceId, AccountId, PaymentRef, Amount, PaidOn, Notes, IsSoftDeleted)
                    VALUES ({insertedInvoiceId}, {paymentAccountId}, '{paymentRef}', {amount.ToString("F2", CultureInfo.InvariantCulture)},
                    '{paidOnStr}', '{notes}', {GetIntProp(payment, "IsSoftDeleted")})";
                var payRes = await dapper.Insert(paymentInsert);
                if (!payRes.Item1)
                {
                    string errorMsg = payRes.Item3 ?? $"Failed to insert invoice payment. Amount: {amount:F2}, AccountId: {paymentAccountId}";
                    return (false, null, errorMsg);
                }
            }

            // Insert taxes mapped to inserted detail ids. Resolve numeric TaxId when possible.
            foreach (var (detail, insertedDetailId) in detailMappings)
            {
                if (insertedDetailId <= 0) continue;

                var taxesForThisDetail = (obj.InvoiceTaxes ?? new ObservableCollection<InvoiceDetailTaxesModel>())
                    .Where(t => GetIntProp(t, "InvoiceDetailId") == GetIntProp(detail, "ItemId") 
                                || GetIntProp(t, "InvoiceDetailId") == GetIntProp(detail, "Id")
                                || GetIntProp(t, "InvoiceDetailId") == insertedDetailId)
                    .ToList();

                foreach (var tax in taxesForThisDetail)
                {
                    string taxIdRaw = GetStringProp(tax, "TaxId");
                    if (string.IsNullOrWhiteSpace(taxIdRaw)) continue;

                    int numericTaxId = 0;
                    if (!int.TryParse(taxIdRaw, out numericTaxId))
                    {
                        // attempt lookup in TaxMaster by name
                        var found = (await dapper.SearchByQuery<TaxMasterModel>($"SELECT * FROM TaxMaster WHERE UPPER(TaxName) = '{S(taxIdRaw)}'")) ?? new List<TaxMasterModel>();
                        if (found.Any()) numericTaxId = found.First().Id;
                    }
                    if (numericTaxId == 0) continue; // skip if cannot resolve

                    double rate = GetDoubleProp(tax, "Rate", "TaxRate");
                    double taxable = GetDoubleProp(tax, "TaxableAmount", "TaxableAmount");
                    double taxAmt = GetDoubleProp(tax, "TaxAmount", "TaxAmount");

                    string taxesInsert = $@"
                        INSERT INTO InvoiceDetailTax (InvoiceDetailId, TaxId, TaxRate, TaxableAmount, TaxAmount)
                        VALUES ({insertedDetailId}, {numericTaxId}, {rate.ToString(CultureInfo.InvariantCulture)}, {taxable.ToString(CultureInfo.InvariantCulture)}, {taxAmt.ToString(CultureInfo.InvariantCulture)});";
                    var taxRes = await dapper.Insert(taxesInsert);
                    if (!taxRes.Item1)
                    {
                        string errorMsg = taxRes.Item3 ?? $"Failed to insert invoice detail tax. DetailId: {insertedDetailId}, TaxId: {numericTaxId}";
                        return (false, null, errorMsg);
                    }
                }
            }

            // Update inventory if this is a purchase or sale invoice
            if (obj.Invoice.InvoiceType?.ToUpper() == "PURCHASE" && obj.Invoice.LocationId > 0)
            {
                await Helper.InventoryHelper.UpdateInventoryFromPurchaseInvoice(dapper, insertedInvoiceId, obj.Invoice.OrganizationId, obj.Invoice.LocationId);
            }
            else if (obj.Invoice.InvoiceType?.ToUpper() == "SALE" && obj.Invoice.LocationId > 0)
            {
                await Helper.InventoryHelper.UpdateInventoryFromSaleInvoice(dapper, insertedInvoiceId, obj.Invoice.OrganizationId, obj.Invoice.LocationId);
            }

            // Build return model using DB read
            var created = (await dapper.SearchByQuery<InvoiceModel>($"SELECT * FROM Invoice WHERE Id={insertedInvoiceId}")) ?? new List<InvoiceModel>();
            InvoiceModel resultModel = created.FirstOrDefault() ?? new InvoiceModel { Id = insertedInvoiceId, Code = code };

            return (true, resultModel, "OK");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool, InvoiceModel, string)> Put(InvoicesModel obj)
    {
        try
        {
            // Check if invoice exists
            var existingInvoice = await dapper.SearchByQuery<InvoiceModel>($"SELECT * FROM Invoice WHERE Id = {obj.Invoice.Id}");
            if (existingInvoice == null || !existingInvoice.Any())
            {
                return (false, null!, "Invoice not found.");
            }
            
            // Check if invoice is posted to GL - prevent updates
            if (existingInvoice.First().IsPostedToGL == 1)
            {
                return (false, null!, "Cannot update invoice that is already posted to General Ledger.");
            }

            // Get old invoice data to reverse inventory impact
            var oldInvoice = existingInvoice?.FirstOrDefault();
            string oldInvoiceType = oldInvoice?.InvoiceType ?? "";
            int oldLocationId = oldInvoice?.LocationId ?? 0;

            // Reverse old inventory impact if invoice type or location changed, or if it was a stock-affecting invoice
            if (oldInvoice != null && oldLocationId > 0 && 
                (oldInvoiceType?.ToUpper() == "PURCHASE" || oldInvoiceType?.ToUpper() == "SALE"))
            {
                await Helper.InventoryHelper.ReverseInventoryFromInvoice(dapper, obj.Invoice.Id, obj.Invoice.OrganizationId, oldLocationId, oldInvoiceType);
            }

            // Validate period for invoice update - use SALE or PURCHASE module type
            if (obj.Invoice.TranDate.HasValue)
            {
                string moduleType = obj.Invoice.InvoiceType?.ToUpper() == "PURCHASE" ? "PURCHASE" : "SALE";
                var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.Invoice.OrganizationId, obj.Invoice.TranDate.Value, moduleType);
                if (!periodCheck.Item1)
                {
                    return (false, null!, periodCheck.Item2);
                }
            }

            // Check stock availability for sale invoices (skip if user confirmed to proceed)
            if (obj.Invoice.InvoiceType?.ToUpper() == "SALE" && obj.Invoice.LocationId > 0 && !obj.Invoice.SkipStockValidation)
            {
                var itemsToCheck = obj.InvoiceDetails?.Select(d => (
                    GetIntProp(d, "ItemId"),
                    GetDoubleProp(d, "Qty", "Quantity"),
                    S(GetStringProp(d, "StockCondition"))
                )).ToList() ?? new List<(int, double, string)>();

                var stockCheck = await Helper.InventoryHelper.CheckStockAvailability(dapper, obj.Invoice.OrganizationId, obj.Invoice.LocationId, itemsToCheck);
                if (!stockCheck.Item1)
                {
                    return (false, null!, stockCheck.Item2);
                }
            }

            // Update invoice header
            string tranDateStr = obj.Invoice.TranDate.HasValue 
                ? obj.Invoice.TranDate.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                : DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            
            int baseCurrencyId = GetIntProp(obj.Invoice, "BaseCurrencyId");
            int enteredCurrencyId = GetIntProp(obj.Invoice, "EnteredCurrencyId");
            double exchangeRate = GetDoubleProp(obj.Invoice, "ExchangeRate");
            if (exchangeRate == 0) exchangeRate = 1.0; // Default to 1.0 if not provided
            int accountId = GetIntProp(obj.Invoice, "AccountId");
            
            // Convert SalesId and LocationId to NULL if 0 or invalid
            string salesIdValue = (obj.Invoice.SalesId > 0) ? obj.Invoice.SalesId.ToString() : "NULL";
            string locationIdValue = (obj.Invoice.LocationId > 0) ? obj.Invoice.LocationId.ToString() : "NULL";
            
            string SQLUpdate = $@"
                UPDATE Invoice SET OrganizationId = {obj.Invoice.OrganizationId}, Code = '{S(obj.Invoice.Code)}',
                InvoiceType = '{S(obj.Invoice.InvoiceType)}', Source = '{S(obj.Invoice.Source)}', SalesId = {salesIdValue},
                LocationId = {locationIdValue}, PartyId = {obj.Invoice.PartyId}, AccountId = {(accountId > 0 ? accountId.ToString() : "NULL")}, PartyName = '{S(obj.Invoice.PartyName)}',
                PartyPhone = '{S(obj.Invoice.PartyPhone)}', PartyEmail = '{S(obj.Invoice.PartyEmail)}',
                PartyAddress = '{S(obj.Invoice.PartyAddress)}',
                TranDate = '{tranDateStr}',
                Description = '{S(obj.Invoice.Description)}', Status = '{S(obj.Invoice.Status)}',
                ClientComments = '{S(obj.Invoice.ClientComments)}', Rating = {obj.Invoice.Rating}, UpdatedBy = {obj.Invoice.UpdatedBy},
                UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', UpdatedFrom = '{S(obj.Invoice.UpdatedFrom)}',
                IsSoftDeleted = {obj.Invoice.IsSoftDeleted},
                BaseCurrencyId = {(baseCurrencyId > 0 ? baseCurrencyId.ToString() : "NULL")},
                EnteredCurrencyId = {(enteredCurrencyId > 0 ? enteredCurrencyId.ToString() : "NULL")},
                ExchangeRate = {exchangeRate.ToString(CultureInfo.InvariantCulture)}
                WHERE Id = {obj.Invoice.Id};";

            var updateResult = await dapper.Update(SQLUpdate);
            if (!updateResult.Item1)
            {
                string errorMsg = updateResult.Item2 ?? "Failed to update invoice header";
                return (false, null!, errorMsg);
            }

            // Delete existing related records in batch
            string deleteBatch = $@"
                DELETE FROM InvoiceDetailTax WHERE InvoiceDetailId IN (SELECT Id FROM InvoiceDetail WHERE InvoiceId = {obj.Invoice.Id});
                DELETE FROM InvoiceDetail WHERE InvoiceId = {obj.Invoice.Id};
                DELETE FROM InvoiceCharges WHERE InvoiceId = {obj.Invoice.Id};
                DELETE FROM InvoicePayments WHERE InvoiceId = {obj.Invoice.Id};";
            var deleteResult = await dapper.ExecuteQuery(deleteBatch);
            if (!deleteResult.Item1)
            {
                return (false, null!, $"Failed to delete existing invoice details: {deleteResult.Item2}");
            }

            // Batch insert details (need IDs for taxes, so use individual inserts but optimize)
            var details = obj.InvoiceDetails ?? new ObservableCollection<InvoiceItemReportModel>();
            var insertedDetails = new List<(InvoiceItemReportModel detail, int insertedId)>();
            
            // Build batch detail insert if possible, otherwise use individual inserts
            if (details.Count > 0)
            {
                var detailValues = new List<string>();
                foreach (var detail in details)
                {
                    int itemId = GetIntProp(detail, "ItemId");
                    double qty = GetDoubleProp(detail, "Qty", "Quantity");
                    double unitPrice = GetDoubleProp(detail, "UnitPrice", "Price");
                    double discountAmt = GetDoubleProp(detail, "DiscountAmount");
                    int rating = GetIntProp(detail, "Rating");
                    int isSoftDeleted = GetIntProp(detail, "IsSoftDeleted");
                    string descr = S(GetStringProp(detail, "Description", "Instructions"));
                    string stockCond = S(GetStringProp(detail, "StockCondition"));
                    string manualItem = S(GetStringProp(detail, "ManualItem"));
                    string serving = S(GetStringProp(detail, "ServingSize"));
                    string status = S(GetStringProp(detail, "Status"));
                    
                    // For manual items (ItemId = 0), save ItemName to ManualItem (convert to uppercase)
                    if (itemId == 0)
                    {
                        string itemName = S(GetStringProp(detail, "ItemName"));
                        if (!string.IsNullOrWhiteSpace(itemName))
                        {
                            manualItem = itemName.ToUpperInvariant();
                        }
                    }
                    
                    // Convert ManualItem to uppercase if it exists
                    if (!string.IsNullOrWhiteSpace(manualItem))
                    {
                        manualItem = manualItem.ToUpperInvariant();
                    }
                    
                    // Convert ItemId = 0 to NULL for manual entries (foreign key constraint)
                    string itemIdValue = itemId > 0 ? itemId.ToString() : "NULL";
                    
                    detailValues.Add($"({itemIdValue}, '{stockCond}', '{manualItem}', '{serving}', {qty.ToString(CultureInfo.InvariantCulture)}, " +
                                   $"{unitPrice.ToString(CultureInfo.InvariantCulture)}, {discountAmt.ToString(CultureInfo.InvariantCulture)}, " +
                                   $"{obj.Invoice.Id}, '{descr}', '{status}', {rating}, " +
                                   $"'{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {isSoftDeleted})");
                }
                
                // Insert all details in one batch
                string batchDetailInsert = $@"
                    INSERT INTO InvoiceDetail (ItemId, StockCondition, ManualItem, ServingSize, Qty, UnitPrice, DiscountAmount, 
                    InvoiceId, Description, Status, Rating, TranDate, IsSoftDeleted)
                    VALUES {string.Join(",", detailValues)};";
                
                var detailInsertResult = await dapper.ExecuteQuery(batchDetailInsert);
                if (!detailInsertResult.Item1)
                {
                    return (false, null!, $"Failed to insert invoice details: {detailInsertResult.Item2}");
                }
                
                var insertedDetailList = await dapper.SearchByQuery<InvoiceItemReportModel>(
                    $"SELECT Id AS InvoiceDetailId, InvoiceId, ItemId, StockCondition, ManualItem, ServingSize, Qty, UnitPrice, DiscountAmount, " +
                    $"Description, Status, Rating, TranDate, IsSoftDeleted FROM InvoiceDetail WHERE InvoiceId = {obj.Invoice.Id} ORDER BY Id");
                if (insertedDetailList != null && insertedDetailList.Count == details.Count)
                {
                    for (int i = 0; i < details.Count; i++)
                    {
                        insertedDetails.Add((details[i], insertedDetailList[i].InvoiceDetailId));
                    }
                }
            }

            // Batch insert charges
            var charges = obj.InvoiceCharges ?? new ObservableCollection<InvoiceChargesModel>();
            if (charges.Any())
            {
                // Validate all charges before batch insert
                foreach (var charge in charges)
                {
                    if (charge.RulesId == 0 || charge.AccountId == 0)
                    {
                        return (false, null!, $"Invalid charge data: RulesId={charge.RulesId}, AccountId={charge.AccountId}. Charge category: {S(charge.ChargeCategory)}");
                    }
                    
                    if (string.IsNullOrWhiteSpace(charge.ChargeCategory) || string.IsNullOrWhiteSpace(charge.AmountType))
                    {
                        return (false, null!, $"Invalid charge data: ChargeCategory or AmountType is missing. Charge category: {S(charge.ChargeCategory)}");
                    }
                }
                
                var chargeValues = new List<string>();
                foreach (var charge in charges)
                {
                    chargeValues.Add($"({obj.Invoice.Id}, {charge.RulesId}, {charge.AccountId}, '{S(charge.ChargeCategory)}', " +
                                   $"'{S(charge.AmountType)}', {charge.Amount.ToString(CultureInfo.InvariantCulture)}, " +
                                   $"{charge.AppliedAmount.ToString(CultureInfo.InvariantCulture)}, {charge.IsSoftDeleted})");
                }
                
                string batchChargeInsert = $@"
                    INSERT INTO InvoiceCharges (InvoiceId, RulesId, AccountId, ChargeCategory, AmountType, Amount, AppliedAmount, IsSoftDeleted)
                    VALUES {string.Join(",", chargeValues)};";
                var chargeInsertResult = await dapper.ExecuteQuery(batchChargeInsert);
                if (!chargeInsertResult.Item1)
                {
                    return (false, null!, $"Failed to insert invoice charges: {chargeInsertResult.Item2}");
                }
            }

            // Batch insert payments
            var payments = obj.InvoicePayments ?? new ObservableCollection<InvoicePaymentModel>();
            if (payments.Any())
            {
                var paymentValues = new List<string>();
                foreach (var payment in payments)
                {
                    double amount = Convert.ToDouble(payment.Amount);
                    if (Math.Abs(amount) < 0.0000001) continue;
                    
                    int paymentAccountId = GetIntProp(payment, "AccountId");
                    // Validate AccountId (Payment Method) is required for payments with amount > 0
                    if (paymentAccountId == 0)
                    {
                        return (false, null!, $"Payment requires a valid AccountId (Payment Method). PaymentRef='{GetStringProp(payment, "PaymentRef")}', Amount={amount:F2}");
                    }
                    
                    string paymentRef = S(GetStringProp(payment, "PaymentRef"));
                    string notes = S(GetStringProp(payment, "Notes"));
                    string paidOnStr = payment.PaidOn.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    
                    paymentValues.Add($"({obj.Invoice.Id}, {paymentAccountId}, '{paymentRef}', {amount.ToString("F2", CultureInfo.InvariantCulture)}, " +
                                    $"'{paidOnStr}', '{notes}', {GetIntProp(payment, "IsSoftDeleted")})");
                }
                
                if (paymentValues.Any())
                {
                    string batchPaymentInsert = $@"
                        INSERT INTO InvoicePayments (InvoiceId, AccountId, PaymentRef, Amount, PaidOn, Notes, IsSoftDeleted)
                        VALUES {string.Join(",", paymentValues)};";
                    var paymentInsertResult = await dapper.ExecuteQuery(batchPaymentInsert);
                    if (!paymentInsertResult.Item1)
                    {
                        return (false, null!, $"Failed to insert invoice payments: {paymentInsertResult.Item2}");
                    }
                }
            }

            // Batch insert taxes
            var invoiceTaxes = obj.InvoiceTaxes ?? new ObservableCollection<InvoiceDetailTaxesModel>();
            if (insertedDetails.Any() && invoiceTaxes.Any())
            {
                var taxValues = new List<string>();
                
                foreach (var (detail, insertedDetailId) in insertedDetails)
                {
                    if (insertedDetailId <= 0) continue;
                    
                    var taxesForThisDetail = invoiceTaxes
                        .Where(t => GetIntProp(t, "InvoiceDetailId") == GetIntProp(detail, "ItemId")
                                    || GetIntProp(t, "InvoiceDetailId") == GetIntProp(detail, "Id")
                                    || GetIntProp(t, "InvoiceDetailId") == insertedDetailId)
                        .ToList();
                    
                    foreach (var tax in taxesForThisDetail)
                    {
                        string taxIdRaw = GetStringProp(tax, "TaxId");
                        if (string.IsNullOrWhiteSpace(taxIdRaw)) continue;
                        
                        int numericTaxId = 0;
                        if (!int.TryParse(taxIdRaw, out numericTaxId))
                        {
                            var found = (await dapper.SearchByQuery<TaxMasterModel>($"SELECT * FROM TaxMaster WHERE UPPER(TaxName) = '{S(taxIdRaw)}'")) ?? new List<TaxMasterModel>();
                            if (found.Any()) numericTaxId = found.First().Id;
                        }
                        if (numericTaxId == 0) continue;
                        
                        double rate = GetDoubleProp(tax, "Rate", "TaxRate");
                        double taxable = GetDoubleProp(tax, "TaxableAmount", "TaxableAmount");
                        double taxAmt = GetDoubleProp(tax, "TaxAmount", "TaxAmount");
                        
                        taxValues.Add($"({insertedDetailId}, {numericTaxId}, {rate.ToString(CultureInfo.InvariantCulture)}, " +
                                    $"{taxable.ToString(CultureInfo.InvariantCulture)}, {taxAmt.ToString(CultureInfo.InvariantCulture)})");
                    }
                }
                
                if (taxValues.Any())
                {
                    string batchTaxInsert = $@"
                        INSERT INTO InvoiceDetailTax(InvoiceDetailId,TaxId,TaxRate,TaxableAmount,TaxAmount)
                        VALUES {string.Join(",", taxValues)};";
                    var taxInsertResult = await dapper.ExecuteQuery(batchTaxInsert);
                    if (!taxInsertResult.Item1)
                    {
                        return (false, null!, $"Failed to insert invoice taxes: {taxInsertResult.Item2}");
                    }
                }
            }

            // Update inventory if this is a purchase or sale invoice (old impact already reversed above)
            if (obj.Invoice.InvoiceType?.ToUpper() == "PURCHASE" && obj.Invoice.LocationId > 0)
            {
                await Helper.InventoryHelper.UpdateInventoryFromPurchaseInvoice(dapper, obj.Invoice.Id, obj.Invoice.OrganizationId, obj.Invoice.LocationId);
            }
            else if (obj.Invoice.InvoiceType?.ToUpper() == "SALE" && obj.Invoice.LocationId > 0)
            {
                await Helper.InventoryHelper.UpdateInventoryFromSaleInvoice(dapper, obj.Invoice.Id, obj.Invoice.OrganizationId, obj.Invoice.LocationId);
            }

            var updated = (await dapper.SearchByQuery<InvoiceModel>($"SELECT * FROM Invoice WHERE Id={obj.Invoice.Id}")) ?? new List<InvoiceModel>();
            return (true, updated.FirstOrDefault() ?? new InvoiceModel(), "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "InvoiceService Put Error for InvoiceId: {InvoiceId}", obj.Invoice.Id);
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> PutStatus(InvoiceModel obj, string Status)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Invoice SET
									Status='{Status}',
									UpdatedBy = {obj.UpdatedBy}, 
									UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
									UpdatedFrom = '{S(obj.UpdatedFrom)}'
							   WHERE Id = {obj.Id};";
            var res = await dapper.Update(SQLUpdate);

            return (true, "OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool, string)> ClientComments(InvoicesModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Invoice SET 
					PartyName = '{S(obj.Invoice.PartyName)}', 
					PartyPhone = '{S(obj.Invoice.PartyPhone)}', 
					PartyEmail = '{obj.Invoice.PartyEmail}',
					PartyAddress = '{S(obj.Invoice.PartyAddress)}', 
					ClientComments = '{S(obj.Invoice.ClientComments)}',
					Rating = {obj.Invoice.Rating}
				WHERE Id = {obj.Invoice.Id};";
            var res = await dapper.Update(SQLUpdate);

			foreach(var i in obj.InvoiceDetails ?? new ObservableCollection<InvoiceItemReportModel>())
			{
                string sql = $@"UPDATE InvoiceDetail SET Rating = {GetIntProp(i,"Rating")} WHERE InvoiceId = {obj.Invoice.Id} and ItemId={GetIntProp(i,"ItemId")};";
                var res1 = await dapper.Update(sql);
            }

            return (true, "OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        // Check if invoice is posted to GL - prevent deletion
        var invoice = await dapper.SearchByQuery<InvoiceModel>($"SELECT * FROM Invoice WHERE Id = {id}");
        if (invoice != null && invoice.Any() && invoice.First().IsPostedToGL == 1)
        {
            return (false, "Cannot delete invoice that is posted to General Ledger.");
        }
        return await dapper.Delete("Invoice", id);
    }

    public async Task<(bool, string)> SoftDelete(InvoiceModel obj)
    {
        // Check if invoice is posted to GL - prevent deletion
        var invoice = await dapper.SearchByQuery<InvoiceModel>($"SELECT * FROM Invoice WHERE Id = {obj.Id}");
        if (invoice != null && invoice.Any() && invoice.First().IsPostedToGL == 1)
        {
            return (false, "Cannot delete invoice that is posted to General Ledger.");
        }
        string SQLUpdate = $@"UPDATE Invoice SET
                                Status = 'DELETED',
								UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
								UpdatedBy = {obj.UpdatedBy!},
								IsSoftDeleted = 1 
							WHERE Id = {obj.Id};";//

        return await dapper.Update(SQLUpdate)!;
    }

    public async Task<(bool, string)> InvoiceStatus(int InvoiceId, string Status, int SoftDelete = 0)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Invoice SET 
					                Status = '{Status!.ToUpper()}',
                                    IsSoftDeleted = {SoftDelete}
				                WHERE Id = {InvoiceId};";

            return await dapper.Update(SQLUpdate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, List<BillMasterReportModel>)>? GetInvoiceReport(int id)
    {
        string SQL = $@"Select * from vw_InvoiceMasterReport Where Id={id}";

        List<BillMasterReportModel> result = (await dapper.SearchByQuery<BillMasterReportModel>(SQL)) ?? new List<BillMasterReportModel>();
        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }
    
    public async Task<(bool, InvoiceCompleteReportModel?)>? GetInvoiceCompleteReport(int id)
    {
        try
        {
            InvoiceCompleteReportModel report = new();
            
            // Get Master/Header
            string masterSQL = $@"SELECT * FROM vw_InvoiceMasterReport WHERE Id={id}";
            var masterList = await dapper.SearchByQuery<InvoiceMasterReportModel>(masterSQL) ?? new List<InvoiceMasterReportModel>();
            if (masterList == null || masterList.Count == 0)
                return (false, null);
            
            report.Invoice = masterList.First();
            
            // Get Details
            string detailsSQL = $@"SELECT * FROM vw_InvoiceDetailsReport WHERE InvoiceId={id}";
            report.Details = await dapper.SearchByQuery<InvoiceItemReportModel>(detailsSQL) ?? new List<InvoiceItemReportModel>();
            
            // Get Charges
            string chargesSQL = $@"SELECT * FROM vw_InvoiceChargesReport WHERE InvoiceId={id}";
            report.Charges = await dapper.SearchByQuery<InvoiceChargesReportModel>(chargesSQL) ?? new List<InvoiceChargesReportModel>();
            
            // Get Payments
            string paymentsSQL = $@"SELECT * FROM vw_InvoicePaymentsReport WHERE InvoiceId={id}";
            report.Payments = await dapper.SearchByQuery<InvoicePaymentsReportModel>(paymentsSQL) ?? new List<InvoicePaymentsReportModel>();
            
            // Get Taxes
            string taxesSQL = $@"SELECT * FROM vw_InvoiceTaxesReport WHERE InvoiceId={id}";
            report.Taxes = await dapper.SearchByQuery<InvoiceTaxesReportModel>(taxesSQL) ?? new List<InvoiceTaxesReportModel>();
            
            return (true, report);
        }
        catch (Exception ex)
        {
            return (false, null);
        }
    }

}

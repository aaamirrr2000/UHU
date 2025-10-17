
using NG.MicroERP.Shared.Models;
using NG.MicroERP.Shared.Services;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace NG.MicroERP.Shared.Helper;

public class Globals
{
    public static string BaseURI = "https://localhost:7019/";
    public static string key = "YourSecretKey1234";

    public static string Computer_sr = string.Empty;
    public static string Token { get; set; } = string.Empty;
    public static string ListName = string.Empty;
    public static string Icon = string.Empty;
    public static string Version = "1.0p";
    public static bool _tabsInitialized = false;
    public static bool _isDarkMode;

    //public static List<ItemsModel> _cart { get; set; } = new();
    public static RestaurantTablesModel? _selectedTable { get; set; } = new();
    public static string _serviceType { get; set; } = string.Empty;
    public static ServiceChargesModel? ServiceCharge { get; set; } = new();
    public static double GST { get; set; } = 0;
    public static string PageTitle { get; set; } = "";
    public static OrganizationsModel Organization { get; set; } = null!;
    //public static EmployeesModel Emp { get; set; } = null!;
    public static UsersModel User { get; set; } = null!;
    public static List<MyMenuModel>? menu { get; set; } = null;
    public static bool isVisible { get; set; } = true;
    public static string seletctedMenuItem { get; set; } = string.Empty;

    public static async Task<(List<ServiceChargesModel> Charges, double TotalAmount)> GetServiceChargesAsync(DateTime today, double baseAmount)
    {
        var myDate = today.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string criteria = $"CAST('{myDate}' AS DATE) BETWEEN CAST(EffectiveFrom AS DATE) AND CAST(EffectiveTo AS DATE) AND OrganizationId = {Globals.Organization.Id}";

        List<ServiceChargesModel> res = await Functions.GetAsync<List<ServiceChargesModel>>($"ServiceCharges/Search/{criteria}", true)
                                          ?? new List<ServiceChargesModel>();

        double total = 0;

        foreach (var charge in res)
        {
            double chargeAmount = string.Equals(charge.ChargeType, "Percentage", StringComparison.OrdinalIgnoreCase)
                ? baseAmount * charge.Amount / 100.0  // percentage
                : charge.Amount;                      // flat

            charge.CalculatedAmount = chargeAmount; // Optional: store it in the model
            total += chargeAmount;
        }

        return (res, total);
    }

    public static async Task<(List<TaxModel> Taxes, double TotalTaxAmount)> GetTaxesAsync(DateTime today, double baseAmount)
    {
        string dateStr = today.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        string criteria = $"CAST('{dateStr}' AS DATE) BETWEEN CAST(EffectiveFrom AS DATE) AND CAST(EffectiveTo AS DATE) AND OrganizationId = {Globals.Organization.Id}";

        List<TaxModel>? list = await Functions.GetAsync<List<TaxModel>>($"Tax/Search/{criteria}", true);

        var taxes = list ?? new List<TaxModel>();
        double totalTax = 0;

        foreach (var tax in taxes)
        {
            double taxAmount = baseAmount * tax.RatePercent / 100.0;
            tax.TaxAmount = taxAmount;
            totalTax += taxAmount;
        }

        return (taxes, totalTax);
    }

    public static string Encrypt(string text)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using Aes aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = new byte[16]; // Zero IV (not recommended for production, use a random IV instead)

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        byte[] textBytes = Encoding.UTF8.GetBytes(text);
        cryptoStream.Write(textBytes, 0, textBytes.Length);
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = new byte[16];

        using MemoryStream memoryStream = new(cipherBytes);
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        byte[] textBytes = new byte[cipherBytes.Length];
        int bytesRead = cryptoStream.Read(textBytes, 0, textBytes.Length);
        return Encoding.UTF8.GetString(textBytes, 0, bytesRead);
    }

    public static async Task<(double GrandTotal, double TaxAmount, double ServiceAmount, double DiscountAmount, double SubTotal)> CalculateBillTotalsAsync(int billId)
    {

        // 🔄 Load Service Charge
        ServiceChargeCalculationService _serviceChargeService = new();
        await _serviceChargeService.InitializeAsync();
        string serviceType = _serviceChargeService.ServiceChargeType;
        double serviceValue = _serviceChargeService.ServiceCharge;

        // 🔄 Load GST
        TaxCalculationService _gstService = new();
        await _gstService.InitializeAsync();
        double gstPercentage = _gstService.GST;

        // 🔄 Get Bill Header (for discount)
        var billRes = await Functions.GetAsync<List<BillModel>>($"Bill/Search/bill.Id={billId}", true);
        if (billRes == null || billRes.Count == 0)
            return (0, 0, 0, 0, 0);

        var bill = billRes.First();
        double discount = Convert.ToDouble( bill.DiscountAmount);

        // 🔄 Get Bill Details (for subtotal)
        var detailRes = await Functions.GetAsync<List<BillDetailModel>>($"BillDetail/Search/BillDetail.BillId={billId}", true);
        if (detailRes == null || detailRes.Count == 0)
            return (0, 0, 0, discount, 0);

        double subTotal = detailRes.Sum(x => x.Item_Amount);

        // ✅ Calculate Service Amount
        double serviceAmount = 0;
        if (serviceType == "PERCENTAGE")
        {
            serviceAmount = (serviceValue / 100.0) * subTotal;
        }
        else if (serviceType == "AMOUNT")
        {
            serviceAmount = serviceValue;
        }

        // ✅ Calculate Tax
        double taxableAmount = subTotal + serviceAmount - discount;
        double taxAmount = (gstPercentage / 100.0) * taxableAmount;

        // ✅ Calculate Grand Total
        double grandTotal = taxableAmount + taxAmount;

        // ✅ Return all values
        return (grandTotal, taxAmount, serviceAmount, discount, subTotal);
    }

    public static BillsModel BillGridTotals(BillsModel Bill)
    {
        //Bill Detail Total
        double TotalBillAmount = 0;
        foreach (var i in Bill.BillDetails)
        {
            TotalBillAmount += (i.Qty * i.UnitPrice) + i.TaxAmount - i.DiscountAmount;
        }

        //Payments Total
        decimal TotalPaymentsAmount = 0;
        foreach (var i in Bill.BillPayments)
        {
            TotalPaymentsAmount += i.AmountPaid;
        }

        //Service Charges
        decimal TotalServiceChargesAmount = 0;

        foreach (var i in Bill.BillCharges.Where(x => x.ChargeCategory != "TAX"))
        {
            TotalServiceChargesAmount += i.CalculatedAmount;
        }

        //Tax
        decimal TotalTaxAmount = 0;

        foreach (var i in Bill.BillCharges.Where(x => x.ChargeCategory == "TAX"))
        {
            TotalTaxAmount += i.CalculatedAmount;
        }

        Bill.Bill.SubTotalAmount = Convert.ToDecimal(TotalBillAmount);
        Bill.Bill.TotalServiceChargeAmount = Convert.ToDecimal(TotalServiceChargesAmount);
        Bill.Bill.TotalTaxAmount = Convert.ToDecimal(TotalTaxAmount);
        Bill.Bill.TotalPaidAmount = Convert.ToDecimal(TotalPaymentsAmount);
        Bill.Bill.BilledAmount = Bill.Bill.SubTotalAmount + Bill.Bill.TotalServiceChargeAmount + Bill.Bill.TotalTaxAmount - Bill.Bill.DiscountAmount;
        Bill.Bill.BalanceAmount = Bill.Bill.BilledAmount - Bill.Bill.TotalPaidAmount;

        return Bill;
    }

    public static (bool canAccess, bool canEdit) PageAccess(List<PermissionsModel> userPermissions, int targetMenuId)
    {
        // If user has GroupId 1 (presumably admin), grant full access
        if (userPermissions.Any(p => p.GroupId == 1))
        {
            return (true, true);
        }

        // Find permission for the specific menu
        var menuPermission = userPermissions.FirstOrDefault(p =>
            p.MenuId == targetMenuId &&
            p.IsActive == 1 &&
            p.IsSoftDeleted == 0);

        if (menuPermission == null)
        {
            return (false, false); // No permission found for this menu
        }

        // Check privilege type
        switch (menuPermission.Privilege?.ToLower())
        {
            case "FULL ACCESS":
                return (true, true);
            case "READ ONLY":
                return (true, false);
            default:
                return (false, false); // No access or invalid privilege
        }
    }
}

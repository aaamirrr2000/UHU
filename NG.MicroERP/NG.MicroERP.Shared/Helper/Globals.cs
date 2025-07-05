
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
        string criteria = $"CAST('{myDate}' AS DATE) BETWEEN CAST(EffectiveFrom AS DATE) AND CAST(EffectiveTo AS DATE) AND organization_id = {Globals.Organization.Id}";

        // --- 2. Fetch rows ---------------------------------------------------
        List<ServiceChargesModel> res = await Functions.GetAsync<List<ServiceChargesModel>>($"ServiceCharges/Search/{criteria}", true) ?? new List<ServiceChargesModel>();

        // --- 3. Calculate total, honouring ChargeType -----------------------
        double total = res.Sum(c => string.Equals(c.ChargeType, "Percentage", StringComparison.OrdinalIgnoreCase)
                ? baseAmount * c.Amount / 100.0     // percentage
                : c.Amount                          // flat
        );

        return (res, total);
    }

    public static async Task<(List<TaxModel> Taxes, double TotalTaxAmount)> GetTaxesAsync(DateTime today, double baseAmount)
    {
        string dateStr = today.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        string criteria = $"CAST('{dateStr}' AS DATE) BETWEEN CAST(EffectiveFrom AS DATE) AND CAST(EffectiveTo AS DATE) AND organization_id = {Globals.Organization.Id}";

        List<TaxModel>? list = await Functions.GetAsync<List<TaxModel>>($"Tax/Search/{criteria}", true);

        var taxes = list ?? new List<TaxModel>();

        double totalTax = taxes.Sum(t => baseAmount * t.RatePercent / 100.0);

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
}

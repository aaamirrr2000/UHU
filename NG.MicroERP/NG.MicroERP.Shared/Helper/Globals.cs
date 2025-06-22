
using NG.MicroERP.Shared.Models;
using NG.MicroERP.Shared.Services;

using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public static ServiceChargeModel? ServiceCharge { get; set; } = new();
    public static double GST { get; set; } = 0;
    public static string PageTitle { get; set; } = "";
    public static OrganizationsModel Organization { get; set; } = null!;
    //public static EmployeesModel Emp { get; set; } = null!;
    public static UsersModel User { get; set; } = null!;
    public static List<MyMenuModel>? menu { get; set; } = null;
    public static bool isVisible { get; set; } = true;
    public static string seletctedMenuItem { get; set; } = string.Empty;

    public static async Task<(double ServiceCharges, double GST, double NetAmount)> CalculateBillAmounts(int BillId)
    {
        Bill_And_Bill_Detail_Model Bills = new();
        ObservableCollection<BillDetailModel> BillDetails = new();
        var res = await Functions.GetAsync<List<BillModel>>($"Bill/Search/bill.Id={BillId}", true);
        if (res is { Count: > 0 })
        {
            Bills.Bill = res.First();

            var detailRes = await Functions.GetAsync<List<BillDetailModel>>($"BillDetail/Search/BillDetail.BillId={BillId}", true);
            if (detailRes != null)
            {
                BillDetails.Clear();
                foreach (var item in detailRes)
                {
                    BillDetails.Add(item);
                }
            }
        }

        double subTotal = 0;
        foreach (var item in BillDetails)
        {
            subTotal += item.Item_Amount;
        }

        //Calcualte Service Charges
        ServiceChargeCalculationService _serviceChargeService = new();
        await _serviceChargeService.InitializeAsync();
        Globals.ServiceCharge!.ServiceChargeType = _serviceChargeService.ServiceChargeType;
        Globals.ServiceCharge.ServiceCharge = _serviceChargeService.ServiceCharge;

        //Get GST
        TaxCalculationService _gst = new();
        await _gst.InitializeAsync();
        Globals.GST = _gst.GST;
        
        double discount = Bills.Bill?.DiscountAmount ?? 0;

        double serviceCharge = 0;
        if (Globals.ServiceCharge!.ServiceChargeType == "PERCENTAGE")
        {
            serviceCharge = (Globals.ServiceCharge.ServiceCharge / 100) * subTotal;
        }
        else if (Globals.ServiceCharge.ServiceChargeType == "AMOUNT")
        {
            serviceCharge = Globals.ServiceCharge.ServiceCharge;
        }

        double taxAmount = (subTotal + serviceCharge - discount) * Globals.GST / 100;

        double netAmount = subTotal + serviceCharge + taxAmount - discount;

        return (serviceCharge, taxAmount, subTotal);

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
            double discount = bill.DiscountAmount;

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

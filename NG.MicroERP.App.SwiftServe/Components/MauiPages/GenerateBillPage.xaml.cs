using NG.MicroERP.App.SwiftServe.Helper;
using NG.MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace NG.MicroERP.App.SwiftServe.Components.MauiPages;

public partial class GenerateBillPage : ContentPage
{
    private int _invoiceId;
    private InvoicesModel _invoices = new();
    private ObservableCollection<InvoiceItemReportModel> _invoiceDetails = new();
    private bool _billGenerated = false;

    public GenerateBillPage(int id)
    {
        InitializeComponent();
        _invoiceId = id;
        LoadBillData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MyGlobals.PageTitle = "Generate Bill";
    }

    private async Task LoadBillData()
    {
        try
        {
            var res = await MyFunctions.GetAsync<List<InvoiceModel>>($"Invoice/Search/i.Id={_invoiceId}", true);
            if (res != null && res.Count > 0)
            {
                _invoices.Invoice = res.First();
                if (_invoices.Invoice.Status == "COMPLETE")
                    _billGenerated = true;

                var detailRes = await MyFunctions.GetAsync<List<InvoiceItemReportModel>>($"InvoiceDetail/Search/InvoiceDetail.InvoiceId={_invoiceId}", true);
                if (detailRes != null)
                {
                    _invoiceDetails = new ObservableCollection<InvoiceItemReportModel>(detailRes);
                }
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error loading bill: {ex.Message}");
            await DisplayAlert("Error", $"Failed to load bill: {ex.Message}", "OK");
        }
    }

    private async void OnGenerateBillClicked(object sender, EventArgs e)
    {
        try
        {
            var invoice = new InvoiceModel
            {
                Id = _invoiceId,
                DiscountAmount = _invoices.Invoice?.DiscountAmount ?? 0
            };

            var result = await MyFunctions.PostAsync<string>("Invoice/GenerateBill", invoice, true);

            if (!result.Success)
            {
                await DisplayAlert("⚠️ Billing Error", "Unable to generate the bill. Please check the order details and try again.", "OK");
            }
            else
            {
                await DisplayAlert("✅ Done", "Bill generated and ready to go!", "Great");
                Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error generating bill: {ex.Message}");
            await DisplayAlert("Error", $"Failed to generate bill: {ex.Message}", "OK");
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}

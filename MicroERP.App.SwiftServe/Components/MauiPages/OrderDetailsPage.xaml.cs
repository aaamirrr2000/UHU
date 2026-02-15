using MicroERP.App.SwiftServe.Helper;
using MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class OrderDetailsPage : ContentPage
{
    private string _action;
    private int _id;
    private InvoicesModel _invoices = new();
    private ObservableCollection<InvoiceItemReportModel> _invoiceDetails = new();

    public OrderDetailsPage(string action, int id)
    {
        InitializeComponent();
        _action = action;
        _id = id;
        
        Title = action == "ORDER" ? "Order Summary" : "Table Summary";
        
        LoadOrderDetails();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MyGlobals.PageTitle = "Order View";
    }

    private async Task LoadOrderDetails()
    {
        try
        {
            string billUrl = string.Empty;
            string billDetailUrl = string.Empty;

            if (_action == "ORDER")
            {
                billUrl = $"Invoice/Search/i.Id={_id} AND i.InvoiceType='BILL'";
                billDetailUrl = $"InvoiceDetail/Search/InvoiceDetail.InvoiceId={_id}";
            }
            else if (_action == "TABLE")
            {
                billUrl = $"Invoice/Search/i.TableId={_id} AND i.InvoiceType='BILL' AND i.Status!='COMPLETE'";
            }

            var res = await MyFunctions.GetAsync<List<InvoiceModel>>(billUrl, true);
            if (res != null && res.Count > 0)
            {
                _invoices.Invoice = res.First();

                if (_action == "ORDER")
                {
                    var res1 = await MyFunctions.GetAsync<List<InvoiceItemReportModel>>(billDetailUrl, true);
                    if (res1 != null)
                    {
                        _invoiceDetails = new ObservableCollection<InvoiceItemReportModel>(res1);
                    }
                }
                else if (_action == "TABLE")
                {
                    var invoiceIds = string.Join(",", res.Select(i => i.Id));
                    billDetailUrl = $"InvoiceDetail/Search/InvoiceDetail.InvoiceId IN ({invoiceIds})";
                    var res1 = await MyFunctions.GetAsync<List<InvoiceItemReportModel>>(billDetailUrl, true);
                    if (res1 != null)
                    {
                        _invoiceDetails = new ObservableCollection<InvoiceItemReportModel>(res1);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error loading order details: {ex.Message}");
            await DisplayAlert("Error", $"Failed to load order details: {ex.Message}", "OK");
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}


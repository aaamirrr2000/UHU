using MicroERP.App.SwiftServe.Helper;
using MicroERP.App.SwiftServe.Components.MauiPages.Controls;
using MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class ClientFeedbackPage : ContentPage
{
    private int _invoiceId;
    private FeedbackModel _feedback = new();
    private ObservableCollection<InvoiceItemReportModel> _invoiceDetails = new();
    private int _rating = 3;

    public ClientFeedbackPage(int id)
    {
        InitializeComponent();
        NavigationPage.SetTitleView(this, NavigationMenu.CreateTitleView(this, NavMenu));
        _invoiceId = id;
        LoadFeedbackData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private async Task LoadFeedbackData()
    {
        try
        {
            if (_invoiceId > 0)
            {
                var result = await MyFunctions.GetAsync<List<InvoiceItemReportModel>>($"InvoiceDetail/Search/InvoiceDetail.InvoiceId={_invoiceId}", true);
                if (result != null)
                {
                    _invoiceDetails = new ObservableCollection<InvoiceItemReportModel>(
                        result.Select(r => { r.Rating = 5; return r; }).ToList()
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error loading feedback data: {ex.Message}");
            await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            InvoicesModel myInvoice = new();
            myInvoice.Invoice.Id = _invoiceId;
            myInvoice.Invoice.PartyName = _feedback.Name?.ToUpper() ?? "";
            myInvoice.Invoice.PartyEmail = _feedback.Email ?? "";
            myInvoice.Invoice.PartyPhone = _feedback.Phone ?? "";
            myInvoice.Invoice.Rating = _rating;
            myInvoice.Invoice.ClientComments = _feedback.Comments ?? "";
            myInvoice.InvoiceDetails = _invoiceDetails;

            var result = await MyFunctions.PostAsync<string>("Invoice/ClientComments", myInvoice, true);

            if (!result.Success)
            {
                await DisplayAlert("⚠️ Save Failed", "Unable to save your changes. Please try again.", "OK");
            }
            else
            {
                await DisplayAlert("✅ All Set", "Your changes have been saved.", "Great!");
                Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error saving feedback: {ex.Message}");
            await DisplayAlert("Error", $"Failed to save feedback: {ex.Message}", "OK");
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    public class FeedbackModel
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public string Comments { get; set; } = string.Empty;
    }
}


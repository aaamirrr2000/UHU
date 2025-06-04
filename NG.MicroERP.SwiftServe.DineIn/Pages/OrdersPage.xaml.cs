using NG.MicroERP.Shared.Models;
using NG.MicroERP.Shared.Helper;
using System.Collections.ObjectModel;

namespace NG.MicroERP.SwiftServe.DineIn.Pages;

public partial class OrdersPage : ContentPage
{
    private ObservableCollection<BillModel> bills = new();
    private List<BillModel> allBills = new();

    public OrdersPage()
    {
        InitializeComponent();
        LoadBills();
        BillsCollectionView.ItemsSource = bills;
        BindingContext = this;
    }

    private async void LoadBills()
    {
        var res = await Config.GetAsync<List<BillModel>>("Bill/Search", true);
        if (res != null)
        {
            allBills = res;
            bills.Clear();
            foreach (var item in allBills)
                bills.Add(item);
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.ToLower() ?? "";

        var filtered = allBills.Where(b =>
            (b.SeqNo?.ToLower().Contains(query) ?? false) ||
            (b.PartyName?.ToLower().Contains(query) ?? false) ||
            (b.Party?.ToLower().Contains(query) ?? false)
        ).ToList();

        bills.Clear();
        foreach (var item in filtered)
            bills.Add(item);
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is BillModel bill)
        {
            await Navigation.PushAsync(new BillPrintPage(bill.Id));
            //await DisplayAlert("Edit", $"Edit Bill ID: {bill.Id}", "OK");
            // Navigate to edit page or show edit modal
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is BillModel bill)
        {
            bool confirm = await DisplayAlert("Delete", $"Delete Bill ID: {bill.Id}?", "Yes", "No");
            if (confirm)
            {
                bills.Remove(bill);
                // Also remove from your DB if needed
            }
        }
    }
}
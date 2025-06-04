using Controls.UserDialogs.Maui;

using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System.Collections.ObjectModel;

namespace NG.MicroERP.App.Pages;

public partial class OrdersPage : ContentPage
{
    private ObservableCollection<BillModel> bills = new();
    private List<BillModel> FilteredBills = new();

    public OrdersPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        FromDatePicker.Date = DateTime.Today;
        LoadBills();
    }

    private async void LoadBills()
    {
        string query = BillSearchBar.Text?.ToLower() ?? "";

        string date = FromDatePicker.Date.ToString("yyyy-MM-dd");
        string criteria = $"CAST(bill.TranDate AS DATE)='{date}'";

        bills.Clear();
        var res = await Functions.GetAsync<List<BillModel>>($"Bill/Search/{criteria}", true);
        if (res != null)
        {    
            foreach (var item in res)
            {

                item.IsDeleteVisible = false;
                if (item.TranDate.Date == DateTime.Today)
                {
                    if (item.Status != "COMPLETE")
                        item.IsDeleteVisible = true;
                }


                if (item.Status != "COMPLETE")
                    item.IsStatusVisible = true;
                else
                    item.IsStatusVisible = false;

                bills.Add(item);
            }
        }

        // Apply client-side search filter
        var filteredBills = bills.Where(b =>
            string.IsNullOrEmpty(query) ||
            (b.SeqNo?.ToLower().Contains(query) ?? false) ||
            (b.PartyName?.ToLower().Contains(query) ?? false) ||
            (b.Party?.ToLower().Contains(query) ?? false)
        ).ToList();

        // Refresh CollectionView
        BillsCollectionView.ItemsSource = null;
        BillsCollectionView.ItemsSource = filteredBills;
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        LoadBills();
        refreshView.IsRefreshing = false;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        LoadBills();
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        LoadBills();
    }


    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is BillModel bill)
        {
            await Navigation.PushAsync(new BillPrintPage(bill.Id));
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is BillModel bill)
        {
            bool confirm = await DisplayAlert("Delete", $"Delete Bill ID: {bill.SeqNo}?", "Yes", "No");

            if (confirm)
            {
                try
                {
                    bills.Remove(bill);
                    bill.UpdatedBy = Globals.User.Id;
                    var res = await Functions.PostAsync<List<BillModel>>("Bill/SoftDelete", bill, true);
                    LoadBills();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
            }
        }
    }

    private async void OnStatusClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is BillModel bill)
        {
            var res = await Functions.PostAsync<List<BillModel>>($"Bill/StatusUpdate/COMPLETE", bill, true);
            if (res.Success == true)
                await DisplayAlert("Status Update", $"Order is Complete now", "OK");
            else
                await DisplayAlert("Error", $"Cannot Update Status of Bill {bill.SeqNo}", "OK");

        }
    }

}

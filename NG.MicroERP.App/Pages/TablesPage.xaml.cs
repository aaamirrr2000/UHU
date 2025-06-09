using Microsoft.Maui.Controls;

using NG.MicroERP.App.Pages;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NG.MicroERP.App.Pages;

public partial class TablesPage : ContentPage
{
    public ObservableCollection<RestaurantTablesModel> Tables { get; } = new();

    public TablesPage()
    {
        InitializeComponent();
        BindingContext = this;
        ListOfTables();
    }

    public async void ListOfTables()
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var data = await Functions.GetAsync<List<RestaurantTablesModel>>("RestaurantTables/Search", true)
                       ?? new List<RestaurantTablesModel>();

            Tables.Clear();
            foreach (var table in data)
            {
                table.TableImage = table.IsAvailable == 1 ? "table.png" : "table.png";
                table.AvailableStatus = table.IsAvailable == 1 ? "Busy" : "Available";
                Tables.Add(table);
            }
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        ListOfTables();
        refreshView.IsRefreshing = false;
    }

    private async void TablesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is RestaurantTablesModel selectedTable)
        {
            string serviceType = "Dine-In"; // Default
            await Navigation.PushAsync(new OrderPage(selectedTable, serviceType));
        }
    }

    private async void ServiceTypeButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantTablesModel selectedTable)
        {
            string serviceType = button.ClassId; // "Dine-In", "Takeaway", "Parcel"
            await Navigation.PushAsync(new OrderPage(selectedTable, serviceType));
        }
    }

    private async void OnViewClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantTablesModel table)
        {
            string serviceType = button.Text; // e.g., "Dine-In", "Takeaway"
            await Navigation.PushAsync(new OrderPage(table, serviceType));
        }
    }

    private async void OnOrderClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantTablesModel table)
        {
            await DisplayAlert("Order", $"Placing order for table {table.TableNumber}", "OK");
        }
    }
}

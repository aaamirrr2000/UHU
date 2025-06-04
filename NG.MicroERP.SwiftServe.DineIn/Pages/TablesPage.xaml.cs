using Microsoft.Maui.Controls;

using NG.MicroERP.SwiftServe.DineIn.Pages;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NG.MicroERP.SwiftServe.DineIn.Pages;

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
        var data = await Config.GetAsync<List<RestaurantTablesModel>>("RestaurantTables/Search", true) ?? new List<RestaurantTablesModel>();

        Tables.Clear();
        foreach (var table in data)
        {
            table.TableImage = table.IsAvailable == 1 ? "dinningtable.png" : "dinningtable1.png";
            table.AvailableStatus = table.IsAvailable == 1 ? "Busy" : "Available";
            Tables.Add(table);
        }
    }

    private async void TablesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedTable = e.CurrentSelection.FirstOrDefault() as RestaurantTablesModel;
        if (selectedTable != null)
        {
            await Navigation.PushAsync(new OrderPage(selectedTable));
        }
    }

    private async void OnViewClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantTablesModel table)
        {
            await Navigation.PushAsync(new OrderPage(table));
        }
    }

    private void OnOrderClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantTablesModel table)
        {
            DisplayAlert("Order", $"Placing order for table {table.TableNumber}", "OK");
        }
    }
}
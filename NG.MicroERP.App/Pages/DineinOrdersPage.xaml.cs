using NG.MicroERP.Shared.Models;
using NG.MicroERP.Shared.Helper;
using System.Collections.ObjectModel;
using Controls.UserDialogs.Maui;

namespace NG.MicroERP.App.Pages;

public partial class DineinOrdersPage : ContentPage
{
    ObservableCollection<DineinOrderStatusModel> Orders = new();

    public DineinOrdersPage()
    {
        InitializeComponent();
        
        LoadOrders();
        BindingContext = this;
    }

    private async void LoadOrders()
    {
        try
        {
            UserDialogs.Instance.ShowLoading("Please wait...");

            string criteria = "Status='IN PROGRESS'";
            string encodedCriteria = Uri.EscapeDataString(criteria);

            var result = await Functions.GetAsync<List<DineinOrderStatusModel>>($"DineinOrderStatus/Search/{Globals.User.LocationId}/{encodedCriteria}", true);

            Orders.Clear(); 
            if (result != null)
            {
                foreach (var item in result)
                    Orders.Add(item);

                OrdersCollectionView.ItemsSource = Orders;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            UserDialogs.Instance.HideHud();
        }
    }

    private async void OnReadyClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is DineinOrderStatusModel order)
        {
            var rec = await Functions.PostAsync<BillDetailModel>($"BillDetail/BillItemStatus/{order.id}/READY", null, true);
            if (rec.Success == false)
            {
                await DisplayAlert("Error", "Record Not saved.", "Okay");
            }
            else
            {
                await DisplayAlert("Save", $"Record Saved Successfully.", "Okay");
                LoadOrders();
                //await Navigation.PopAsync();
            }
            
        }
    }

    private async void OnRefreshCartClicked(object sender, EventArgs e)
    {
        LoadOrders();
    }

}

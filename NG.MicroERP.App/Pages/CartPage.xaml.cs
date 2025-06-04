using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.Shared.Services;

using System.Collections.ObjectModel;


namespace NG.MicroERP.App.Pages;

public partial class CartPage : ContentPage
{
    private ObservableCollection<ItemsModel> _cartItems;
    public string BillNote { get; set; } = string.Empty;
    RestaurantTablesModel tbl = new RestaurantTablesModel();

    public CartPage(ObservableCollection<ItemsModel> cart, RestaurantTablesModel table)
    {
        InitializeComponent();

        _cartItems = cart;
        Cart.ItemsSource = _cartItems;
        tbl = table;
        BindingContext = this;
        UpdateTotal();
    }

    private void UpdateTotal()
    {
        double total = _cartItems.Sum(i => i.RetailPrice);
        TotalLabel.Text = $"{total:N2}";
    }

    private async void OnRemoveItemClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is ItemsModel item)
        {
            bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to remove \"{item.Name}\"?", "Yes", "No");

            if (confirm)
            {
                var itemToRemove = _cartItems.FirstOrDefault(x => x.Id == item.Id);
                if (itemToRemove != null)
                {
                    _cartItems.Remove(itemToRemove);
                    UpdateTotal();
                }
            }
        }
    }

    private async void OnPlaceOrderClicked(object sender, EventArgs e)
    {
        var tranDate = DateTime.Now;

        var billDetails = _cartItems.Select(item => new BillDetailModel
        {
            ItemId = item.Id,
            StockCondition = "NEW",
            Qty = item.MaxQty,
            UnitPrice = item.RetailPrice,
            TranDate = tranDate,
            Description = item.Description,
            Status = "IN PROGRESS",
            ServingSize= item.ServingSize,
            IsTakeAway = Convert.ToInt16( item.MinQty)
        }).ToList();

        // Calculate the total bill amount
        double totalAmount = billDetails.Sum(d => (d.Qty * Convert.ToDouble(d.UnitPrice)));

        var bill = new BillModel
        {
            OrganizationId = Globals.Organization.Id,
            BillType = "BILL",
            Source = "POS",
            TableId = tbl.Id,
            SalesId = Globals.User.EmpId,
            PartyId = 1,
            TranDate = tranDate,
            CreatedBy = Globals.User.Id,
            CreatedOn = tranDate,
            CreatedFrom = Functions.GetComputerDetails(),
            LocationId = Globals.User.LocationId,
            Description = BillNote,
            BillAmount = Convert.ToDouble(totalAmount)
        };


        Bill_And_Bill_Detail_Model request = new Bill_And_Bill_Detail_Model();
        request.Bill = bill;
        request.BillDetails = new ObservableCollection<BillDetailModel>(billDetails);

        await Save(request);
    }


    private async Task Save(Bill_And_Bill_Detail_Model data)
    {
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        var rec = await Functions.PostAsync<Bill_And_Bill_Detail_Model>("Bill/Insert", data, true);
        if (rec.Success == false)
        {
            await DisplayAlert("Error", "Record Not saved.", "Okay");
        }
        else
        {
            //string result = rec!.Result!.Bill!.SeqNo!;
            await DisplayAlert("Save", $"Record Saved Successfully.", "Okay");
            _cartItems = null;
            Cart.ItemsSource = _cartItems;
            //await Navigation.PopAsync();
            //await Navigation.PopToRootAsync();
            Application.Current.MainPage = new NavigationPage(new MainMenuPage());
        }

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
    }
}
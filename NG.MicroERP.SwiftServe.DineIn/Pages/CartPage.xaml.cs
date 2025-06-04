
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.Shared.Services.Services;

using System.Collections.ObjectModel;


namespace NG.MicroERP.SwiftServe.DineIn.Pages;

public partial class CartPage : ContentPage
{
    private ObservableCollection<ItemsModel> _cartItems;
    string BillNote = string.Empty;

    public CartPage(List<ItemsModel> cart)
    {
        InitializeComponent();

        _cartItems = new ObservableCollection<ItemsModel>(cart);
        Cart.ItemsSource = _cartItems;
        BindingContext = this;
        UpdateTotal();
    }

    private void UpdateTotal()
    {
        double total = _cartItems.Sum(i => i.RetailPrice);
        TotalLabel.Text = $"Total: {total:N2}";
    }

    private void OnRemoveItemClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is ItemsModel item && Cart.ItemsSource is ObservableCollection<ItemsModel> cartItems)
        {
            cartItems.Remove(item);
            UpdateTotal();
        }
    }

    private async void OnPlaceOrderClicked(object sender, EventArgs e)
    {
        var bill = new BillModel
        {
            OrganizationId=Globals.Organization.Id,
            BillType = "BILL",
            PartyId=1,
            TranDate = DateTime.Now,
            CreatedBy = Globals.User.Id,
            CreatedOn = DateTime.Now,
            CreatedFrom = "Functions.GetComputerDetails()",
            LocationId = Globals.User.LocationId,
            Remark = BillNote
        };

        var billDetails = _cartItems.Select(item => new BillDetailModel
        {
            ItemId = item.Id,
            StockCondition="NEW",
            Qty = item.MaxQty,
            UnitPrice = item.RetailPrice,
            TranDate = bill.TranDate,
            Remark = item.Description!

        }).ToList();

        var request = new Bill_And_Bill_Detail_Model
        {
            Bill = bill,
            BillDetails = new ObservableCollection<BillDetailModel>(billDetails)
        };

        await Save(request);
    }

    private async Task Save(Bill_And_Bill_Detail_Model data)
    {  
        var rec = await Config.PostAsync<Bill_And_Bill_Detail_Model>("Bill/Insert", data, true);
        if (rec.Success == false)
        {
            await DisplayAlert("Error", "Record Not saved.", "Okay");
        }
        else
        {
            string result = rec!.Result!.Bill!.SeqNo!;
            await DisplayAlert("Save", $"Record Saved Successfully. INV: {result}", "Okay");
        }
    }
}
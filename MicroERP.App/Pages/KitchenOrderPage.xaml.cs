using MicroERP.Shared.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.App.Pages;

public partial class KitchenOrderPage : ContentPage
{
    private Bill_And_Bill_Detail_Model _data;

    public KitchenOrderPage()
    {
        InitializeComponent();

        BindingContext = _data;
        
    }

    private async Task LoadBills()
    {
            //_data.Bill = res.FirstOrDefault()!;
            //var res1 = await Functions.GetAsync<List<BillDetailModel>>($"BillDetail/Search/BillDetail.BillId={Id}", true);
            //if (res1 != null)
            //{
            //    BillDetails.Clear();
            //    foreach (var item in res1)
            //    {
            //        BillDetails.Add(item);
            //    }
            //    ItemsList.ItemsSource = _data.BillDetails;
            //}
    }
}

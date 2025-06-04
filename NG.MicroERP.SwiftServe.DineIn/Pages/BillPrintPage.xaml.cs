using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System.Collections.ObjectModel;

using static MudBlazor.Icons.Material;

namespace NG.MicroERP.SwiftServe.DineIn.Pages;

public partial class BillPrintPage : ContentPage
{
    private Bill_And_Bill_Detail_Model Bills = new();
    public BillPrintPage(int BillId)
    {
        InitializeComponent();
        LoadDataAsync(BillId);
    }

    private async void LoadDataAsync(int BillId)
    {
        await LoadBills(BillId);
        await LoadBillDetails();
        BindingContext = Bills;
        LoadOrganization();
        LoadBillHeader();
        LoadTotals();
    }

    private async Task LoadBillDetails()
    {
        BillDetailsCollectionView.ItemsSource = null;
        BillDetailsCollectionView.ItemsSource = Bills.BillDetails;
    }

    private void LoadBillHeader()
    {
        txtSeqNo.Text = Bills.Bill.SeqNo;
        txtTranDate.Text = Bills.Bill.TranDate.ToString("dd-MM-yyyy");
        txtLocation.Text = Bills.Bill.Location;
    }

    private void LoadTotals()
    {
        txtGrandTotal.Text = $"Rs {Bills.BillDetails.Sum(x => x.Item_Amount):N2}";
    }

    private async void LoadOrganization()
    {
        txtOrganization.Text = Globals.Organization.Name;
        txtOrganizationAddress.Text = Globals.Organization.Address;
        txtContact.Text = $"Phone: {Globals.Organization.Phone} | Email: {Globals.Organization.Email}";

    }

    private async Task LoadBills(int Id)
    {
        var res = await Config.GetAsync<List<BillModel>>($"Bill/Search/bill.Id={Id}", true);
        if (res != null)
        {
            Bills.Bill = res.FirstOrDefault()!;
            var res1 = await Config.GetAsync<List<BillDetailModel>>($"BillDetail/Search/BillDetail.BillId={Id}", true);
            if (res1 != null)
            {
                Bills.BillDetails.Clear();
                foreach (var item in res1)
                    Bills.BillDetails.Add(item);
            }
        }
    }

}
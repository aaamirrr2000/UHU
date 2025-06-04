using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System.Collections.ObjectModel;

namespace NG.MicroERP.App.Pages;

public partial class BillPrintPage : ContentPage
{
    private Bill_And_Bill_Detail_Model Bills = new();
    public ObservableCollection<BillDetailModel> BillDetails { get; set; } = new ObservableCollection<BillDetailModel>();

    public BillPrintPage(int BillId)
    {
        InitializeComponent();
        LoadBills(BillId);
        BindingContext = this;
    }

    private async Task LoadBills(int Id)
    {
        var res = await Functions.GetAsync<List<BillModel>>($"Bill/Search/bill.Id={Id}", true);
        if (res != null)
        {
            Bills.Bill = res.FirstOrDefault()!;
            var res1 = await Functions.GetAsync<List<BillDetailModel>>($"BillDetail/Search/BillDetail.BillId={Id}", true);
            if (res1 != null)
            {
                BillDetails.Clear();
                foreach (var item in res1)
                {
                    if (item.Status == "READY")
                        item.Status = "✔";
                    else
                        item.Status = "";

                    BillDetails.Add(item);
                }
            }

            // Now update UI properties
            txtOrganization.Text = Globals.Organization.Name;
            txtOrganizationAddress.Text = Globals.Organization.Address;
            txtContact.Text = $"Phone: {Globals.Organization.Phone}  Email: {Globals.Organization.Email}";

            txtSeqNo.Text = Bills.Bill.SeqNo;
            txtTranDate.Text = Bills.Bill.TranDate.ToString("dd-MM-yyyy");
            txtLocation.Text = Bills.Bill.Location;

            BillDetailsCollectionView.ItemsSource = null;
            BillDetailsCollectionView.ItemsSource = BillDetails;

            txtGrandTotal.Text = $" {BillDetails.Sum(x => x.Item_Amount):N2}";
        }
    }
}
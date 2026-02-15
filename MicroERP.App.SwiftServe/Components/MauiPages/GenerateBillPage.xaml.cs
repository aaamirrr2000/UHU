using MicroERP.App.SwiftServe.Helper;
using MicroERP.App.SwiftServe.Components.MauiPages.Controls;
using MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class GenerateBillPage : ContentPage
{
    private int _invoiceId;
    private InvoicesModel _invoiceData = new();
    private ObservableCollection<ChartOfAccountsModel> _paymentMethods = new();
    private ObservableCollection<PaymentRow> _paymentRows = new();
    private decimal _totalDue;
    private decimal _subTotal;
    private decimal _paidTotal;

    public ObservableCollection<ChartOfAccountsModel> PaymentMethods => _paymentMethods;
    public ObservableCollection<InvoiceDetailModel> BillDetails { get; } = new();
    public ObservableCollection<InvoicePaymentModel> ExistingPayments { get; } = new();
    public ObservableCollection<PaymentRow> PaymentRows => _paymentRows;
    public string BillCode => _invoiceData?.Invoice?.Code ?? $"#{_invoiceId}";
    public string TableInfo => _invoiceData?.Invoice?.TableName != null ? $"Table: {_invoiceData.Invoice.TableName}" : "";
    public decimal SubTotal => _subTotal;
    public decimal PaidTotal => _paidTotal;
    public decimal TotalDue => _totalDue;
    public decimal NewPaymentsTotal => _paymentRows.Sum(p => p.AmountValue);

    public GenerateBillPage(int id)
    {
        InitializeComponent();
        BindingContext = this;
        NavigationPage.SetTitleView(this, NavigationMenu.CreateTitleView(this, NavMenu));
        _invoiceId = id;
        LoadBillAndPaymentMethods();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MyGlobals.PageTitle = "Final Bill";
    }

    private async void LoadBillAndPaymentMethods()
    {
        try
        {
            if (LoadingOverlay != null) { LoadingOverlay.IsVisible = true; LoadingOverlay.Message = "Loading..."; }

            var criteria = Uri.EscapeDataString("InterfaceType='PAYMENT METHOD'");
            var pmTask = MyFunctions.GetAsync<List<ChartOfAccountsModel>>($"ChartOfAccounts/Search/{criteria}", true);
            var invTask = MyFunctions.GetAsync<InvoicesModel>($"Invoice/Get/{_invoiceId}", true);
            var pmList = await pmTask;
            var inv = await invTask;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _paymentMethods = new ObservableCollection<ChartOfAccountsModel>(pmList ?? new List<ChartOfAccountsModel>());
                if (inv != null)
                {
                    _invoiceData = inv;
                    ExistingPayments.Clear();
                    foreach (var p in inv.InvoicePayments ?? new ObservableCollection<InvoicePaymentModel>())
                        ExistingPayments.Add(p);
                    BillDetails.Clear();
                    if (inv.InvoiceDetails != null)
                    {
                        foreach (var d in inv.InvoiceDetails)
                        {
                            BillDetails.Add(new InvoiceDetailModel
                            {
                                Id = d.InvoiceDetailId,
                                Item = d.ItemName,
                                Qty = d.Qty,
                                UnitPrice = d.UnitPrice,
                                DiscountAmount = d.DiscountAmount
                            });
                        }
                    }
                    RecalcTotals();
                }
                OnPropertyChanged(nameof(BillCode));
                OnPropertyChanged(nameof(TableInfo));
                OnPropertyChanged(nameof(SubTotal));
                OnPropertyChanged(nameof(PaidTotal));
                OnPropertyChanged(nameof(TotalDue));
                OnPropertyChanged(nameof(NewPaymentsTotal));
            });
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "LoadBill: {Message}", ex.Message);
            MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlert("Error", "Failed to load bill.", "OK"));
        }
        finally
        {
            MainThread.BeginInvokeOnMainThread(() => { if (LoadingOverlay != null) LoadingOverlay.IsVisible = false; });
        }
    }

    private void RecalcTotals()
    {
        _subTotal = (decimal)(BillDetails.Sum(d => d.Qty * d.UnitPrice - d.DiscountAmount));
        var inv = _invoiceData?.Invoice;
        if (inv != null)
        {
            _paidTotal = ExistingPayments.Sum(p => p.Amount);
            _totalDue = (inv.BillAmount > 0 ? inv.BillAmount : _subTotal) - _paidTotal;
            if (_totalDue < 0) _totalDue = 0;
        }
        else
        {
            _paidTotal = 0;
            _totalDue = _subTotal;
        }
    }

    public void OnAddPayment(object sender, EventArgs e)
    {
        _paymentRows.Add(new PaymentRow());
        OnPropertyChanged(nameof(PaymentRows));
        OnPropertyChanged(nameof(NewPaymentsTotal));
    }

    public void OnRemovePayment(object sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is PaymentRow row)
        {
            _paymentRows.Remove(row);
            OnPropertyChanged(nameof(PaymentRows));
            OnPropertyChanged(nameof(NewPaymentsTotal));
        }
    }

    private async void OnSavePayments(object sender, EventArgs e)
    {
        var newOnes = _paymentRows.Where(p => (p.SelectedMethod?.Id ?? 0) > 0 && p.AmountValue > 0).ToList();
        if (newOnes.Count == 0)
        {
            await DisplayAlert("Info", "Add at least one payment (method and amount).", "OK");
            return;
        }

        try
        {
            if (LoadingOverlay != null) LoadingOverlay.IsVisible = true;
            var allPayments = new ObservableCollection<InvoicePaymentModel>();
            foreach (var p in ExistingPayments)
                allPayments.Add(p);
            foreach (var p in newOnes)
            {
                allPayments.Add(new InvoicePaymentModel
                {
                    InvoiceId = _invoiceId,
                    AccountId = p.SelectedMethod?.Id ?? 0,
                    PaymentRef = p.PaymentRef ?? "",
                    Amount = p.AmountValue,
                    PaidOn = DateTime.UtcNow
                });
            }

            var payload = new InvoicesModel
            {
                Invoice = _invoiceData.Invoice,
                InvoiceDetails = _invoiceData.InvoiceDetails,
                InvoicePayments = allPayments
            };
            var res = await MyFunctions.PostAsync<InvoiceModel>("Invoice/Update", payload, true);
            if (res.Success)
            {
                await DisplayAlert("Saved", "Payments saved.", "OK");
                _paymentRows.Clear();
                foreach (var np in newOnes)
                    ExistingPayments.Add(new InvoicePaymentModel { AccountId = np.SelectedMethod?.Id ?? 0, PaymentRef = np.PaymentRef, Amount = np.AmountValue, PaidOn = DateTime.UtcNow });
                RecalcTotals();
                OnPropertyChanged(nameof(PaidTotal));
                OnPropertyChanged(nameof(TotalDue));
                OnPropertyChanged(nameof(PaymentRows));
                OnPropertyChanged(nameof(NewPaymentsTotal));
                LoadBillAndPaymentMethods();
            }
            else
                await DisplayAlert("Error", res.Message ?? "Failed to save payments.", "OK");
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "SavePayments: {Message}", ex.Message);
            await DisplayAlert("Error", "Failed to save payments.", "OK");
        }
        finally
        {
            if (LoadingOverlay != null) LoadingOverlay.IsVisible = false;
        }
    }

    private async void OnCloseBill(object sender, EventArgs e)
    {
        var totalPaid = _paidTotal + _paymentRows.Sum(p => p.AmountValue);
        var due = _totalDue - _paymentRows.Sum(p => p.AmountValue);
        if (due > 0.01m)
        {
            var saveFirst = await DisplayAlert("Balance due", $"Balance due is {due:N2}. Save payments first or close anyway?", "Save payments first", "Close anyway");
            if (saveFirst) return;
        }

        try
        {
            if (LoadingOverlay != null) LoadingOverlay.IsVisible = true;
            if (_paymentRows.Any(p => p.AmountValue > 0))
            {
                var allPayments = new ObservableCollection<InvoicePaymentModel>();
                foreach (var p in ExistingPayments) allPayments.Add(p);
                foreach (var p in _paymentRows.Where(p => (p.SelectedMethod?.Id ?? 0) > 0 && p.AmountValue > 0))
                    allPayments.Add(new InvoicePaymentModel { InvoiceId = _invoiceId, AccountId = p.SelectedMethod!.Id, PaymentRef = p.PaymentRef ?? "", Amount = p.AmountValue, PaidOn = DateTime.UtcNow });
                var payload = new InvoicesModel { Invoice = _invoiceData.Invoice, InvoiceDetails = _invoiceData.InvoiceDetails, InvoicePayments = allPayments };
                await MyFunctions.PostAsync<InvoiceModel>("Invoice/Update", payload, true);
            }
            var statusRes = await MyFunctions.PostAsync<InvoiceModel>($"Invoice/InvoiceStatus/{_invoiceId}/COMPLETE/0", null, true);
            if (statusRes.Success)
            {
                await DisplayAlert("Done", "Bill closed.", "OK");
                Navigation.PopAsync();
            }
            else
                await DisplayAlert("Error", statusRes.Message ?? "Failed to close bill.", "OK");
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "CloseBill: {Message}", ex.Message);
            await DisplayAlert("Error", "Failed to close bill.", "OK");
        }
        finally
        {
            if (LoadingOverlay != null) LoadingOverlay.IsVisible = false;
        }
    }

    private void OnBackClicked(object sender, EventArgs e) => Navigation.PopAsync();
}

public class PaymentRow : System.ComponentModel.INotifyPropertyChanged
{
    private ChartOfAccountsModel? _selectedMethod;
    private string _amountText = "";
    private string _paymentRef = "";

    public ChartOfAccountsModel? SelectedMethod { get => _selectedMethod; set { _selectedMethod = value; OnPropertyChanged(); } }
    public string AmountText { get => _amountText; set { _amountText = value ?? ""; OnPropertyChanged(); OnPropertyChanged(nameof(AmountValue)); } }
    public decimal AmountValue => decimal.TryParse(_amountText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
    public string PaymentRef { get => _paymentRef; set { _paymentRef = value ?? ""; OnPropertyChanged(); } }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name ?? ""));
}

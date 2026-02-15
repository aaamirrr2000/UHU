using MicroERP.App.SwiftServe.Helper;
using MicroERP.App.SwiftServe.Components.MauiPages.Controls;
using MicroERP.App.SwiftServe.Services;
using MicroERP.App.SwiftServe.ViewModels;
using MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class CartPage : ContentPage
{
    private CartStateService _cartState;
    private ObservableCollection<CartItemViewModel> _cartItems;
    private InvoiceModel _party = new();
    private bool _isPlacingOrder;

    public ObservableCollection<CartItemViewModel> CartItems
    {
        get => _cartItems;
        set
        {
            _cartItems = value;
            OnPropertyChanged();
        }
    }

    // BillNote is handled directly via BillNoteEditor.TextChanged event

    public CartPage()
    {
        InitializeComponent();
        BindingContext = this;
        NavigationPage.SetTitleView(this, NavigationMenu.CreateTitleView(this, NavMenu));
        _cartState = Application.Current?.Handler?.MauiContext?.Services?.GetService<CartStateService>() 
                     ?? new CartStateService();
        LoadCartData();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateHeader();
        await LoadChargesAndTaxAsync();
        UpdateSummary();
        UpdateParcelDetailsVisibility();
    }

    /// <summary>
    /// Load service charge, discount (and optionally tax) from API like sale invoice.
    /// </summary>
    private async Task LoadChargesAndTaxAsync()
    {
        try
        {
            var criteria = Uri.EscapeDataString("a.IsActive=1");
            var rules = await MyFunctions.GetAsync<List<ChargesRulesModel>>($"api/ChargesRules/Search/{criteria}", true) ?? new List<ChargesRulesModel>();
            var today = DateTime.Today;
            var effective = rules.Where(x => x.ChargeCategory != null
                && (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value.Date <= today)
                && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= today)).ToList();

            var firstService = effective.FirstOrDefault(x => x.ChargeCategory!.Trim().ToUpperInvariant() == "SERVICE");
            if (firstService != null)
            {
                MyGlobals.ServiceCharge.ChargeType = (firstService.AmountType ?? "PERCENTAGE").ToUpperInvariant();
                MyGlobals.ServiceCharge.Amount = firstService.Amount;
            }
            var firstDiscount = effective.FirstOrDefault(x => x.ChargeCategory!.Trim().ToUpperInvariant() == "DISCOUNT");
            if (firstDiscount != null)
            {
                MyGlobals.Discount.ChargeType = (firstDiscount.AmountType ?? "PERCENTAGE").ToUpperInvariant();
                MyGlobals.Discount.Amount = firstDiscount.Amount;
            }
        }
        catch
        {
            // Keep existing MyGlobals.ServiceCharge / Discount if API fails
        }
    }

    private void LoadCartData()
    {
        var items = _cartState?.CartItems ?? new ObservableCollection<ItemsModel>();
        CartItems = new ObservableCollection<CartItemViewModel>(items.Select(i => new CartItemViewModel(i)));
        if (BillNoteEditor != null)
        {
            BillNoteEditor.Text = _cartState?.BillNote ?? string.Empty;
        }
        UpdateEmptyState();
    }

    private void UpdateHeader()
    {
        if (_cartState?.SelectedTable != null)
        {
            HeaderSubtitle.Text = $"Table: {_cartState.SelectedTable.TableNumber} | Service: {_cartState.ServiceType}";
        }
        else
        {
            HeaderSubtitle.Text = $"Service: {_cartState?.ServiceType ?? "N/A"}";
        }
    }

    private void UpdateEmptyState()
    {
        bool isEmpty = !CartItems.Any();
        EmptyStateLayout.IsVisible = isEmpty;
        CartItemsCollectionView.IsVisible = !isEmpty;
    }

    private void UpdateSummary()
    {
        // RetailPrice in cart is line total (unit price × qty) per item
        double subTotal = CartItems?.Sum(vm => vm.Item.RetailPrice) ?? 0;
        SubTotalLabel.Text = subTotal.ToString("N2");

        // Service charge shown as "Delivery" to match cart design
        double serviceAmount = CalculateChargeAmount(MyGlobals.ServiceCharge, subTotal);
        ServiceChargeLabel.Text = "Delivery:";
        ServiceChargeAmountLabel.Text = serviceAmount.ToString("N2");

        // Discount (same as sale invoice: PERCENTAGE or FLAT; flat capped at subTotal)
        double discountAmount = CalculateChargeAmount(MyGlobals.Discount, subTotal, isDiscount: true);
        DiscountLabel.Text = MyGlobals.Discount.ChargeType == "PERCENTAGE"
            ? $"Discount {MyGlobals.Discount.Amount:N2}%:"
            : "Discount:";
        DiscountAmountLabel.Text = discountAmount.ToString("N2");

        // Tax on (subTotal + service - discount), like sale invoice
        double amountAfterChargesAndDiscount = subTotal + serviceAmount - discountAmount;
        if (amountAfterChargesAndDiscount < 0) amountAfterChargesAndDiscount = 0;
        double taxAmount = (MyGlobals.GST / 100) * amountAfterChargesAndDiscount;
        TaxLabel.Text = $"Tax {MyGlobals.GST:N2}%:";
        TaxAmountLabel.Text = taxAmount.ToString("N2");

        // Grand total = SubTotal + Service Charges - Discount + Tax
        double grandTotal = amountAfterChargesAndDiscount + taxAmount;
        GrandTotalLabel.Text = grandTotal.ToString("N2");
    }

    private static double CalculateChargeAmount(ServiceChargeInfo info, double baseAmount, bool isDiscount = false)
    {
        if (info == null) return 0;
        if (string.Equals(info.ChargeType, "PERCENTAGE", StringComparison.OrdinalIgnoreCase))
            return (info.Amount / 100) * baseAmount;
        if (string.Equals(info.ChargeType, "FLAT", StringComparison.OrdinalIgnoreCase))
        {
            if (isDiscount && info.Amount > baseAmount) return baseAmount;
            return info.Amount;
        }
        return 0;
    }

    private void UpdateParcelDetailsVisibility()
    {
        bool isParcel = MyGlobals._serviceType?.ToUpper() == "PARCEL";
        ParcelDetailsFrame.IsVisible = isParcel;
    }

    private void OnIncreaseQty(object sender, EventArgs e)
    {
        var vm = GetCartItemFromSender(sender);
        if (vm == null) return;
        var item = vm.Item;
        double unitPrice = item.MaxQty > 0 ? item.RetailPrice / item.MaxQty : item.RetailPrice;
        vm.MaxQty++;
        item.RetailPrice = unitPrice * item.MaxQty;
        vm.NotifyAllProperties();
        UpdateSummary();
    }

    private void OnDecreaseQty(object sender, EventArgs e)
    {
        var vm = GetCartItemFromSender(sender);
        if (vm == null || vm.MaxQty <= 1) return;
        var item = vm.Item;
        double unitPrice = item.RetailPrice / item.MaxQty;
        vm.MaxQty--;
        item.RetailPrice = unitPrice * item.MaxQty;
        vm.NotifyAllProperties();
        UpdateSummary();
    }

    private static CartItemViewModel? GetCartItemFromSender(object sender)
    {
        if (sender is not Button button) return null;
        return button.CommandParameter as CartItemViewModel ?? button.BindingContext as CartItemViewModel;
    }

    private void OnRemoveItem(object sender, EventArgs e)
    {
        var vm = GetCartItemFromSender(sender);
        if (vm != null)
        {
            CartItems.Remove(vm);
            _cartState?.CartItems?.Remove(vm.Item);
            UpdateSummary();
            UpdateEmptyState();
        }
    }

    private void OnInstructionsChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is CartItemViewModel vm)
        {
            vm.Item.Description = e.NewTextValue;
        }
    }

    private void OnBillNoteChanged(object sender, TextChangedEventArgs e)
    {
        if (_cartState != null)
        {
            _cartState.BillNote = e.NewTextValue;
        }
    }

    private async void OnPlaceOrderClicked(object sender, EventArgs e)
    {
        if (_isPlacingOrder) return;
        if (!CartItems.Any())
        {
            await DisplayAlert("Error", "Cart is empty. Please add items first.", "OK");
            return;
        }

        _isPlacingOrder = true;
        if (PlaceOrderButton != null)
            PlaceOrderButton.IsEnabled = false;
        if (PlaceOrderOverlay != null)
        {
            PlaceOrderOverlay.IsVisible = true;
            if (PlaceOrderBusyLabel != null) PlaceOrderBusyLabel.Text = "Placing order...";
        }

        try
        {
            var tranDate = DateTime.Now;
            // Use item's RevenueAccountId for InvoiceDetail (sale/bill line); fallback 58 = REVENUE OF SALES if not set
            const int defaultRevenueAccountId = 58;
            var billDetails = CartItems.Select(vm => vm.Item).Select(item => new InvoiceItemReportModel
            {
                ItemId = item.Id,
                AccountId = item.RevenueAccountId ?? defaultRevenueAccountId,
                StockCondition = "NEW",
                Qty = item.MaxQty,
                UnitPrice = item.MaxQty > 0 ? item.RetailPrice / item.MaxQty : item.RetailPrice,
                TranDate = tranDate,
                Instructions = item.Description ?? "",
                Status = "PENDING",
                ServingSize = item.ServingSize,
                Person = item.Person
            }).ToList();

            var bill = new InvoiceModel
            {
                OrganizationId = MyGlobals.Organization.Id,
                InvoiceType = "SALE INVOICE",
                Source = "POS",
                TableId = _cartState?.SelectedTable?.Id ?? 0,
                SalesId = MyGlobals.User.EmpId,
                PartyId = 1,
                PartyName = _party.PartyName ?? "",
                PartyPhone = _party.PartyPhone ?? "",
                PartyEmail = _party.PartyEmail ?? "",
                PartyAddress = _party.PartyAddress ?? "",
                TranDate = tranDate,
                ServiceType = _cartState?.ServiceType ?? "Dine-In",
                CreatedBy = MyGlobals.User.Id,
                CreatedOn = tranDate,
                CreatedFrom = Environment.MachineName,
                LocationId = MyGlobals.User.LocationId,
                Description = _cartState?.BillNote ?? "",
                BillAmount = Convert.ToDecimal(billDetails.Sum(d => d.Qty * Convert.ToDouble(d.UnitPrice)))
            };

            var request = new InvoicesModel
            {
                Invoice = bill,
                InvoiceDetails = new ObservableCollection<InvoiceItemReportModel>(billDetails)
            };

            var result = await MyFunctions.PostAsync<InvoicesModel>("Invoice/Insert", request, true);

            if (!result.Success)
            {
                if (PlaceOrderOverlay != null) PlaceOrderOverlay.IsVisible = false;
                _isPlacingOrder = false;
                if (PlaceOrderButton != null) PlaceOrderButton.IsEnabled = true;
                await DisplayAlert("❌ Save Failed", "Record not saved.", "OK");
            }
            else
            {
                if (PlaceOrderOverlay != null) PlaceOrderOverlay.IsVisible = false;
                _cartState?.Clear();
                CartItems.Clear();
                await DisplayAlert("✅ Saved", "Order placed successfully.", "OK");
                Application.Current!.MainPage = new NavigationPage(new TablesPage());
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error placing order: {ex.Message}");
            if (PlaceOrderOverlay != null) PlaceOrderOverlay.IsVisible = false;
            _isPlacingOrder = false;
            if (PlaceOrderButton != null) PlaceOrderButton.IsEnabled = true;
            await DisplayAlert("Error", $"Failed to place order: {ex.Message}", "OK");
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
            Navigation.PopAsync();
        else
            Application.Current!.MainPage = new NavigationPage(new TablesPage());
    }

    private void OnPromoApplyClicked(object sender, EventArgs e)
    {
        // Placeholder: promo code logic can be wired to API later
        string code = PromoCodeEntry?.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(code))
            return;
        // Optional: DisplayAlert("Promo", "Code applied (not implemented).", "OK");
    }
}


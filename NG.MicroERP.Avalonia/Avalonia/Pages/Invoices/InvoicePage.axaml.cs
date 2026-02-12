using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NG.MicroERP.Shared.Models;

namespace Avalonia.Pages.Invoices
{
    public partial class InvoicePage : Window
    {
        private InvoicesModel _record = new();
        private List<PartiesModel> _parties = new();
        private List<CurrenciesModel> _currencies = new();
        private List<string> _invoiceTypeList = new();
        private List<ItemsModel> _allItems = new();
        private int? _currentRecordId;
        private string _action = "INSERT";
        private string _invoiceType = "SALE";
        private bool _loading;
        private bool _saving;
        private PartiesModel? _selectedParty;

        public InvoicePage() : this("SALE", null) { }

        public InvoicePage(string invoiceType, int? existingId)
        {
            _invoiceType = invoiceType ?? "SALE";
            _currentRecordId = existingId;
            _action = existingId.HasValue && existingId.Value != 0 ? "UPDATE" : "INSERT";
            InitializeComponent();
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            EnsureInvoiceInitialized();
            await LoadAllAsync();
        }

        private void EnsureInvoiceInitialized()
        {
            if (_record == null) _record = new InvoicesModel();
            if (_record.Invoice == null) _record.Invoice = new InvoiceModel();
            if (_record.InvoiceDetails == null) _record.InvoiceDetails = new ObservableCollection<InvoiceItemReportModel>();
            if (_record.InvoicePayments == null) _record.InvoicePayments = new ObservableCollection<InvoicePaymentModel>();
            if (_record.InvoiceCharges == null) _record.InvoiceCharges = new ObservableCollection<InvoiceChargesModel>();
            if (_record.InvoiceTaxes == null) _record.InvoiceTaxes = new ObservableCollection<InvoiceDetailTaxesModel>();
        }

        private async Task LoadAllAsync()
        {
            SetLoading(true);
            try
            {
                _currencies = await App.Functions.GetAsync<List<CurrenciesModel>>("Currencies/Search", true) ?? new List<CurrenciesModel>();
                string partyType = _invoiceType.StartsWith("SALE", StringComparison.OrdinalIgnoreCase) ? "CUSTOMER" : "SUPPLIER";
                _parties = await App.Functions.GetAsync<List<PartiesModel>>($"Parties/Search/a.PartyType='{partyType}'", true) ?? new List<PartiesModel>();
                _allItems = await App.Functions.GetAsync<List<ItemsModel>>("Items/Search", true) ?? new List<ItemsModel>();

                InitializeInvoiceTypeList();
                SetDefaultValues();

                if (_currentRecordId.HasValue && _currentRecordId.Value != 0)
                    await LoadInvoiceAsync(_currentRecordId.Value);
                else
                    BindToUi();
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void InitializeInvoiceTypeList()
        {
            _invoiceTypeList.Clear();
            if (_invoiceType.StartsWith("SALE", StringComparison.OrdinalIgnoreCase))
            {
                _invoiceTypeList.Add("SALE");
                _invoiceTypeList.Add("SALE RETURN");
            }
            else
            {
                _invoiceTypeList.Add("PURCHASE");
                _invoiceTypeList.Add("PURCHASE RETURN");
            }
        }

        private void SetDefaultValues()
        {
            if (_currencies?.Any() == true)
            {
                var baseCur = _currencies.FirstOrDefault(c => c.IsBaseCurrency == 1);
                if (baseCur != null)
                {
                    _record.Invoice.BaseCurrencyId = baseCur.Id;
                    _record.Invoice.EnteredCurrencyId = baseCur.Id;
                }
            }
            _record.Invoice.ExchangeRate = 1.0;
            if (_parties?.Any() == true)
            {
                _selectedParty = _parties[0];
                _record.Invoice.PartyId = _selectedParty.Id;
                _record.Invoice.PartyName = _selectedParty.Name;
            }
            if (_action == "INSERT")
            {
                _record.Invoice.InvoiceType = _invoiceType;
                _record.Invoice.TranDate = DateTime.Now;
                _record.Invoice.OrganizationId = App.Globals.User?.OrganizationId ?? 0;
            }
        }

        private async Task LoadInvoiceAsync(int id)
        {
            try
            {
                var loaded = await App.Functions.GetAsync<InvoicesModel>($"Invoice/Get/{id}", true);
                if (loaded != null)
                {
                    _record = loaded;
                    EnsureInvoiceInitialized();
                    _selectedParty = _parties?.FirstOrDefault(p => p.Id == _record.Invoice.PartyId);
                }
            }
            catch { /* ignore */ }
            BindToUi();
        }

        private void BindToUi()
        {
            HeaderTitle.Text = $"{_invoiceType} Invoice";
            HeaderSubtitle.Text = _currentRecordId.GetValueOrDefault() > 0 ? $"Invoice #{_record.Invoice?.Code}" : "New invoice";

            ComboInvoiceType.ItemsSource = _invoiceTypeList;
            ComboInvoiceType.SelectedItem = _record.Invoice?.InvoiceType;

            ComboParty.ItemsSource = _parties;
            ComboParty.SelectedItem = _selectedParty;

            if (DateTranDate != null) DateTranDate.SelectedDate = _record.Invoice?.TranDate ?? DateTime.Today;
            TxtInvoiceCode.Text = _record.Invoice?.Code ?? "";
            var cur = _currencies?.FirstOrDefault(c => c.Id == (_record.Invoice?.EnteredCurrencyId ?? 0));
            TxtCurrency.Text = cur?.Name ?? cur?.Code ?? "N/A";
            TxtDescription.Text = _record.Invoice?.Description ?? "";
            TxtInvoiceDiscount.Text = ((double)(_record.Invoice?.DiscountAmount ?? 0)).ToString("N2");
            TxtServiceCharges.Text = ((double)(_record.Invoice?.ChargesAmount ?? 0)).ToString("N2");
            TxtInvoiceTax.Text = ((double)(_record.Invoice?.TaxAmount ?? 0)).ToString("N2");

            GridLines.ItemsSource = null;
            GridLines.ItemsSource = _record.InvoiceDetails ?? new ObservableCollection<InvoiceItemReportModel>();

            ApplyItemFilter();
            UpdateTotals();
        }

        private void ApplyItemFilter()
        {
            var q = TxtItemSearch?.Text?.Trim() ?? "";
            var list = string.IsNullOrWhiteSpace(q)
                ? _allItems
                : _allItems.Where(x =>
                    (x.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Code?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            ItemsList.ItemsSource = list;
        }

        private void UpdateTotals()
        {
            if (_record?.Invoice == null) return;
            double subtotal = _record.InvoiceDetails?.Sum(d => d.ItemTotalAmount) ?? 0;
            _record.Invoice.InvoiceAmount = (decimal)subtotal;
            double discount = (double)_record.Invoice.DiscountAmount;
            double charges = (double)_record.Invoice.ChargesAmount;
            double tax = (double)_record.Invoice.TaxAmount;
            double total = subtotal - discount + charges + tax;
            _record.Invoice.TotalInvoiceAmount = (decimal)total;
            decimal totalPaid = _record.InvoicePayments?.Sum(p => p.Amount) ?? 0;
            double balance = total - (double)totalPaid;
            _record.Invoice.TotalPayments = totalPaid;
            _record.Invoice.BalanceAmount = (decimal)balance;

            TxtSubtotal.Text = subtotal.ToString("N2");
            TxtInvoiceTotal.Text = total.ToString("N2");
            TxtTotalPaid.Text = totalPaid.ToString("N2");
            TxtBalance.Text = balance.ToString("N2");

            UpdateColumnTotals();
        }

        private void UpdateColumnTotals()
        {
            var details = _record?.InvoiceDetails;
            if (details == null || details.Count == 0)
            {
                TxtTotalQty.Text = "0";
                TxtTotalDiscount.Text = "0.00";
                TxtTotalTax.Text = "0.00";
                TxtTotalLineAmount.Text = "0.00";
                return;
            }
            double sumQty = details.Sum(d => d.Qty);
            double sumDiscount = details.Sum(d => d.DiscountAmount);
            double sumTax = details.Sum(d => d.TaxAmount);
            double sumTotal = details.Sum(d => d.ItemTotalAmount);
            TxtTotalQty.Text = sumQty.ToString("N2");
            TxtTotalDiscount.Text = sumDiscount.ToString("N2");
            TxtTotalTax.Text = sumTax.ToString("N2");
            TxtTotalLineAmount.Text = sumTotal.ToString("N2");
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Handled) return;
            if (e.Key == Key.L && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
            {
                FocusItemList();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.N && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control) { OnNew(sender, e); e.Handled = true; return; }
            if (e.Key == Key.S && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control) { OnSave(sender, e); e.Handled = true; return; }
            if (e.Key == Key.P && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control) { OnPrint(sender, e); e.Handled = true; return; }
            if (e.Key == Key.Delete && GridLines.SelectedItem != null) { OnDeleteLine(sender, e); e.Handled = true; }
        }

        private void FocusItemList()
        {
            TxtItemSearch?.Focus();
            Dispatcher.UIThread.Post(() => ItemsList?.Focus(), DispatcherPriority.Input);
            SetStatus("Item list focused. Use arrow keys to move, Enter to open edit and add to grid.");
        }

        private void SetLoading(bool on)
        {
            _loading = on;
            if (BusyOverlay != null) BusyOverlay.IsVisible = on;
            BtnSave.IsEnabled = !on && !_saving;
            BtnNew.IsEnabled = !on;
            BtnPrint.IsEnabled = !on;
        }

        private void SetStatus(string msg)
        {
            if (StatusText != null) StatusText.Text = msg;
        }

        private void OnNew(object? sender, RoutedEventArgs e)
        {
            _record = new InvoicesModel();
            EnsureInvoiceInitialized();
            _currentRecordId = null;
            _action = "INSERT";
            SetDefaultValues();
            BindToUi();
            SetStatus("New invoice. Tab to move, Ctrl+S to save.");
        }

        private async void OnSave(object? sender, RoutedEventArgs e)
        {
            if (_saving || _loading) return;
            EnsureInvoiceInitialized();
            SyncFromUi();
            _record.Invoice.OrganizationId = App.Globals.User?.OrganizationId ?? 0;
            _record.Invoice.Source = "MANUAL";
            _record.Invoice.UpdatedBy = App.Globals.User?.Id ?? 0;
            _record.Invoice.UpdatedOn = DateTime.Now;
            if (_action == "INSERT")
            {
                _record.Invoice.CreatedBy = App.Globals.User?.Id ?? 0;
                _record.Invoice.CreatedOn = DateTime.Now;
            }

            _saving = true;
            SetStatus("Saving…");
            try
            {
                var actionType = _currentRecordId.GetValueOrDefault() != 0 ? "UPDATE" : "INSERT";
                var (success, result, message) = await App.Functions.PostAsync<InvoiceModel>($"Invoice/{actionType}", _record, true);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _saving = false;
                    if (success && result != null)
                    {
                        _currentRecordId = result.Id;
                        _action = "UPDATE";
                        _record.Invoice.Id = result.Id;
                        _record.Invoice.Code = result.Code;
                        SetStatus($"Saved. Invoice #{result.Code}");
                        BindToUi();
                    }
                    else
                        SetStatus($"Error: {message}");
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() => { _saving = false; SetStatus($"Error: {ex.Message}"); });
            }
        }

        private void SyncFromUi()
        {
            _record.Invoice.InvoiceType = ComboInvoiceType.SelectedItem as string ?? _record.Invoice.InvoiceType;
            if (ComboParty.SelectedItem is PartiesModel p)
            {
                _record.Invoice.PartyId = p.Id;
                _record.Invoice.PartyName = p.Name;
            }
            var sel = DateTranDate.SelectedDate;
            _record.Invoice.TranDate = sel.HasValue ? sel.Value.DateTime : (_record.Invoice.TranDate ?? DateTime.Today);
            _record.Invoice.Description = TxtDescription.Text ?? "";
            if (decimal.TryParse(TxtInvoiceDiscount?.Text?.Replace(",", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var disc))
                _record.Invoice.DiscountAmount = disc;
            if (decimal.TryParse(TxtServiceCharges?.Text?.Replace(",", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var chg))
                _record.Invoice.ChargesAmount = chg;
            if (decimal.TryParse(TxtInvoiceTax?.Text?.Replace(",", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var tx))
                _record.Invoice.TaxAmount = tx;
        }

        private void OnPrint(object? sender, RoutedEventArgs e)
        {
            if (_currentRecordId.GetValueOrDefault() == 0)
            {
                SetStatus("Save the invoice before printing.");
                return;
            }
            var report = new InvoiceReport(_currentRecordId!.Value);
            report.Show();
        }

        private async void OnAddItem(object? sender, RoutedEventArgs e)
        {
            var picker = new ItemPickerWindow();
            await picker.ShowDialog(this);
            if (picker.SelectedItem != null)
            {
                EnsureInvoiceInitialized();
                double price = picker.SelectedItem.BasePrice > 0 ? picker.SelectedItem.BasePrice : picker.SelectedItem.RetailPrice;
                double qty = picker.SelectedItem.Qty > 0 ? picker.SelectedItem.Qty : 1;
                double total = qty * price;
                _record.InvoiceDetails.Add(new InvoiceItemReportModel
                {
                    ItemId = picker.SelectedItem.Id,
                    ItemName = picker.SelectedItem.Name ?? "",
                    Qty = qty,
                    UnitPrice = price,
                    DiscountAmount = 0,
                    TaxAmount = 0,
                    ItemTotalAmount = total
                });
                RefreshGridAndTotals();
                SetStatus($"Added {picker.SelectedItem.Name}. Delete key to remove line.");
            }
        }

        private void OnManualLine(object? sender, RoutedEventArgs e)
        {
            var dialog = new ManualLineDialog();
            dialog.ShowDialog(this).ContinueWith(_ =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (dialog.ResultLine != null)
                    {
                        EnsureInvoiceInitialized();
                        _record.InvoiceDetails.Add(dialog.ResultLine);
                        RefreshGridAndTotals();
                        SetStatus("Manual line added.");
                    }
                });
            });
        }

        private void OnDeleteLine(object? sender, RoutedEventArgs e)
        {
            if (GridLines.SelectedItem is InvoiceItemReportModel line)
            {
                _record.InvoiceDetails.Remove(line);
                RefreshGridAndTotals();
                SetStatus("Line removed.");
            }
            else
                SetStatus("Select a line (click or arrow keys) then Delete.");
        }

        private async void OnEditLine(object? sender, RoutedEventArgs e)
        {
            if (GridLines.SelectedItem is not InvoiceItemReportModel line)
            {
                SetStatus("Select a line first, then click Edit line.");
                return;
            }
            var dialog = new LineEditDialog { Title = "Edit line — Enter to save, Esc to cancel" };
            dialog.SetLine(line);
            var result = await dialog.ShowDialog<bool?>(this);
            RefreshGridAndTotals();
            SetStatus(result == true ? "Line updated." : "Edit cancelled.");
        }

        private void OnUpdateGrid(object? sender, RoutedEventArgs e)
        {
            RefreshGridAndTotals();
            SetStatus("Grid and totals updated.");
        }

        private void RefreshGridAndTotals()
        {
            GridLines.ItemsSource = null;
            GridLines.ItemsSource = _record?.InvoiceDetails ?? new ObservableCollection<InvoiceItemReportModel>();
            UpdateTotals();
        }

        private async void OnLineDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (GridLines.SelectedItem is InvoiceItemReportModel line)
            {
                var dialog = new LineEditDialog();
                dialog.SetLine(line);
                await dialog.ShowDialog<bool?>(this);
                RefreshGridAndTotals();
            }
        }

        private async void OnPayment(object? sender, RoutedEventArgs e)
        {
            if (_currentRecordId.GetValueOrDefault() == 0)
            {
                SetStatus("Save the invoice first, then add payments.");
                return;
            }
            var paymentMethods = new[] { "CASH", "CARD", "BANK", "CHEQUE", "OTHER" };
            var dialog = new InvoicePaymentDialog();
            dialog.SetInvoice(_currentRecordId!.Value, _record.InvoicePayments, paymentMethods);
            await dialog.ShowDialog(this);
            RefreshGridAndTotals();
            SetStatus("Payments updated. Total paid and balance refreshed. Save invoice to persist.");
        }

        private void OnInvoiceDiscountChanged(object? sender, TextChangedEventArgs e)
        {
            if (_record?.Invoice != null && decimal.TryParse(TxtInvoiceDiscount?.Text?.Replace(",", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                _record.Invoice.DiscountAmount = v;
            UpdateTotals();
        }
        private void OnServiceChargesChanged(object? sender, TextChangedEventArgs e)
        {
            if (_record?.Invoice != null && decimal.TryParse(TxtServiceCharges?.Text?.Replace(",", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                _record.Invoice.ChargesAmount = v;
            UpdateTotals();
        }
        private void OnInvoiceTaxChanged(object? sender, TextChangedEventArgs e)
        {
            if (_record?.Invoice != null && decimal.TryParse(TxtInvoiceTax?.Text?.Replace(",", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                _record.Invoice.TaxAmount = v;
            UpdateTotals();
        }

        private void OnPartySelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ComboParty?.SelectedItem is PartiesModel p)
            {
                _selectedParty = p;
                _record.Invoice.PartyId = p.Id;
                _record.Invoice.PartyName = p.Name;
            }
        }

        private void OnItemSearchChanged(object? sender, TextChangedEventArgs e)
        {
            ApplyItemFilter();
        }

        private async void OnItemFromListSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ItemsList?.SelectedItem is not ItemsModel item) return;
            EnsureInvoiceInitialized();
            double price = item.BasePrice > 0 ? item.BasePrice : item.RetailPrice;
            var newLine = new InvoiceItemReportModel
            {
                ItemId = item.Id,
                ItemName = item.Name ?? "",
                Qty = 1,
                UnitPrice = price,
                DiscountAmount = 0,
                TaxAmount = 0,
                ItemTotalAmount = price
            };
            var dialog = new LineEditDialog { Title = "Add item — change details then Enter to add, Esc to cancel" };
            dialog.SetLine(newLine);
            var result = await dialog.ShowDialog<bool?>(this);
            if (result == true)
            {
                _record.InvoiceDetails.Add(newLine);
                RefreshGridAndTotals();
                SetStatus("Item added to grid. Arrow keys + Enter to add more.");
            }
            ItemsList.SelectedItem = null;
        }

        private void OnItemsClick(object? sender, RoutedEventArgs e)
        {
            FocusItemList();
        }
    }
}

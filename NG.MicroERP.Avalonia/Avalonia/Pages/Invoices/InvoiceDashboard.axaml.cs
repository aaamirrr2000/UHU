using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using NG.MicroERP.Shared.Models;

namespace Avalonia.Pages.Invoices
{
    public partial class InvoiceDashboard : Window
    {
        private List<InvoicesAllModel> _allData = new();
        private string _invoiceType = "SALE";

        public InvoiceDashboard() : this("SALE") { }

        public InvoiceDashboard(string invoiceType)
        {
            _invoiceType = invoiceType ?? "SALE";
            InitializeComponent();
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            HeaderTitle.Text = $"{_invoiceType} Invoice Management";
            DateStart.SelectedDate = DateTime.Today.AddMonths(-6);
            DateEnd.SelectedDate = DateTime.Today;
            GridInvoices.SelectionChanged += (s, _) =>
            {
                var hasSelection = GridInvoices.SelectedItem is InvoicesAllModel;
                BtnView.IsEnabled = hasSelection;
                BtnEdit.IsEnabled = hasSelection;
                BtnPrint.IsEnabled = hasSelection;
            };
            await LoadInvoicesAsync();
        }

        private async Task LoadInvoicesAsync()
        {
            BusyOverlay.IsVisible = true;
            StatusLabel.Text = "Loading…";
            try
            {
                var start = DateStart.SelectedDate?.DateTime ?? DateTime.Today.AddMonths(-6);
                var end = DateEnd.SelectedDate?.DateTime ?? DateTime.Today;
                if (start > end) { StatusLabel.Text = "Start date must be before end date."; return; }
                var startStr = start.ToString("yyyy-MM-dd");
                var endNext = end.Date.AddDays(1).ToString("yyyy-MM-dd");
                var criteria = $"i.InvoiceType = '{_invoiceType}' AND CAST(ISNULL(i.TranDate, i.CreatedOn) AS DATE) >= CAST('{startStr}' AS DATE) AND CAST(ISNULL(i.TranDate, i.CreatedOn) AS DATE) < CAST('{endNext}' AS DATE)";
                if (App.Globals.User?.OrganizationId > 0)
                    criteria += $" AND i.OrganizationId = {App.Globals.User.OrganizationId}";
                _allData = await App.Functions.GetAsync<List<InvoicesAllModel>>($"Invoice/Search/{Uri.EscapeDataString(criteria)}", true) ?? new List<InvoicesAllModel>();
                ApplySearch();
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error: {ex.Message}";
                _allData = new List<InvoicesAllModel>();
            }
            finally
            {
                BusyOverlay.IsVisible = false;
            }
        }

        private void ApplySearch()
        {
            var search = TxtSearch?.Text?.Trim() ?? "";
            var list = string.IsNullOrWhiteSpace(search)
                ? _allData
                : _allData.Where(x =>
                    (x.Code?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.PartyName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Party?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Status?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            GridInvoices.ItemsSource = list;
            StatusLabel.Text = $"{list.Count} invoice(s). Tab to move, Enter to open.";
        }

        private async void OnSearch(object? sender, RoutedEventArgs e)
        {
            await LoadInvoicesAsync();
        }

        private async void OnRefresh(object? sender, RoutedEventArgs e)
        {
            await LoadInvoicesAsync();
        }

        private void OnNew(object? sender, RoutedEventArgs e)
        {
            var page = new InvoicePage(_invoiceType, null);
            page.Show();
            page.Closed += (_, _) => _ = LoadInvoicesAsync();
        }

        private void OnView(object? sender, RoutedEventArgs e)
        {
            if (GridInvoices.SelectedItem is InvoicesAllModel row)
            {
                var report = new InvoiceReport(row.ID);
                report.Show();
            }
        }

        private void OnEdit(object? sender, RoutedEventArgs e)
        {
            if (GridInvoices.SelectedItem is InvoicesAllModel row)
            {
                var page = new InvoicePage(_invoiceType, row.ID);
                page.Show();
                page.Closed += (_, _) => _ = LoadInvoicesAsync();
            }
        }

        private void OnPrint(object? sender, RoutedEventArgs e)
        {
            if (GridInvoices.SelectedItem is InvoicesAllModel row)
            {
                var report = new InvoiceReport(row.ID);
                report.Show();
            }
        }

        private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (GridInvoices.SelectedItem is InvoicesAllModel row)
            {
                var page = new InvoicePage(_invoiceType, row.ID);
                page.Show();
                page.Closed += (_, _) => _ = LoadInvoicesAsync();
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Handled) return;
            if (e.Key == Key.N && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
            {
                OnNew(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && GridInvoices.SelectedItem is InvoicesAllModel row)
            {
                var page = new InvoicePage(_invoiceType, row.ID);
                page.Show();
                page.Closed += (_, _) => _ = LoadInvoicesAsync();
                e.Handled = true;
            }
        }
    }
}

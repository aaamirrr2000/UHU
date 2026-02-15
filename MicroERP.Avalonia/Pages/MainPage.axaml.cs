using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MicroERP.Shared.Models;

namespace Avalonia.Pages
{
    public record ActivityItem(string Title, string Subtitle);

    public partial class MainPage : Window
    {
        private readonly List<Button> _menuButtons = new();
        private readonly List<Button> _allFocusableMenuButtons = new();
        private List<InvoicesAllModel> _invoiceListAll = new();
        private const string InvoiceType = "SALE";

        public MainPage()
        {
            InitializeComponent();
            _menuButtons.AddRange(new[]
            {
                MenuDashboard, MenuInvoices, MenuProjects, MenuTeam, MenuAnalytics, MenuSettings,
                MenuProfile
            });
            _allFocusableMenuButtons.AddRange(_menuButtons);
            _allFocusableMenuButtons.Add(MenuSignOut);
            SetActiveMenuItem(MenuDashboard);
            LoadActivityItems();
            UpdateWelcomeText();
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (SidebarPanel != null)
                KeyboardNavigation.SetTabNavigation(SidebarPanel, KeyboardNavigationMode.Cycle);
            MenuDashboard?.Focus();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Handled) return;

            if (e.Key == Key.Q && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
            {
                OnSignOutClick(sender, e);
                e.Handled = true;
                return;
            }

            // Arrow keys and Home/End: navigate menu when focus is on a menu button
            if (e.Source is Button btn && _allFocusableMenuButtons.Contains(btn))
            {
                var idx = _allFocusableMenuButtons.IndexOf(btn);
                if (e.Key == Key.Enter || e.Key == Key.Space)
                {
                    btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.Up || e.Key == Key.Left)
                {
                    var prev = idx <= 0 ? _allFocusableMenuButtons.Count - 1 : idx - 1;
                    _allFocusableMenuButtons[prev].Focus();
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.Down || e.Key == Key.Right)
                {
                    var next = idx >= _allFocusableMenuButtons.Count - 1 ? 0 : idx + 1;
                    _allFocusableMenuButtons[next].Focus();
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.Home)
                {
                    _allFocusableMenuButtons[0].Focus();
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.End)
                {
                    _allFocusableMenuButtons[_allFocusableMenuButtons.Count - 1].Focus();
                    e.Handled = true;
                    return;
                }
            }
        }

        private void UpdateWelcomeText()
        {
            var user = App.Globals?.User;
            if (WelcomeText != null)
                WelcomeText.Text = user != null ? $"Welcome back, {user.FullName ?? user.Username ?? "User"}." : "Welcome back.";
        }

        private void LoadActivityItems()
        {
            var items = new[]
            {
                new ActivityItem("Project Alpha updated", "2 hours ago"),
                new ActivityItem("New task assigned to you", "5 hours ago"),
                new ActivityItem("Team sync completed", "Yesterday"),
                new ActivityItem("Design review approved", "2 days ago"),
            };
            ActivityList.ItemsSource = items;
        }

        private void SetActiveMenuItem(Button? active)
        {
            foreach (var btn in _menuButtons)
            {
                if (btn == active)
                    btn.Classes.Add("active");
                else
                    btn.Classes.Remove("active");
            }
        }

        private void OnMenuClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                SetActiveMenuItem(btn);
                if (btn == MenuDashboard)
                {
                    DashboardContent.IsVisible = true;
                    InvoiceListContent.IsVisible = false;
                }
            }
        }

        private async void OnInvoicesClick(object? sender, RoutedEventArgs e)
        {
            SetActiveMenuItem(MenuInvoices);
            DashboardContent.IsVisible = false;
            InvoiceListContent.IsVisible = true;
            InvoiceDateStart.SelectedDate = DateTime.Today.AddMonths(-6);
            InvoiceDateEnd.SelectedDate = DateTime.Today;
            InvoiceGridInvoices.SelectionChanged += (s, _) =>
            {
                var has = InvoiceGridInvoices.SelectedItem is InvoicesAllModel;
                InvoiceBtnView.IsEnabled = has;
                InvoiceBtnEdit.IsEnabled = has;
                InvoiceBtnPrint.IsEnabled = has;
            };
            await LoadInvoicesAsync();
        }

        private async Task LoadInvoicesAsync()
        {
            InvoiceStatusLabel.Text = "Loading…";
            try
            {
                var start = InvoiceDateStart.SelectedDate?.DateTime ?? DateTime.Today.AddMonths(-6);
                var end = InvoiceDateEnd.SelectedDate?.DateTime ?? DateTime.Today;
                if (start > end) { InvoiceStatusLabel.Text = "Start date must be before end date."; return; }
                var startStr = start.ToString("yyyy-MM-dd");
                var endNext = end.Date.AddDays(1).ToString("yyyy-MM-dd");
                var criteria = $"i.InvoiceType = '{InvoiceType}' AND CAST(ISNULL(i.TranDate, i.CreatedOn) AS DATE) >= CAST('{startStr}' AS DATE) AND CAST(ISNULL(i.TranDate, i.CreatedOn) AS DATE) < CAST('{endNext}' AS DATE)";
                if (App.Globals.User?.OrganizationId > 0)
                    criteria += $" AND i.OrganizationId = {App.Globals.User.OrganizationId}";
                _invoiceListAll = await App.Functions.GetAsync<List<InvoicesAllModel>>($"Invoice/Search/{Uri.EscapeDataString(criteria)}", true) ?? new List<InvoicesAllModel>();
                ApplyInvoiceSearch();
            }
            catch (Exception ex)
            {
                InvoiceStatusLabel.Text = $"Error: {ex.Message}";
                _invoiceListAll = new List<InvoicesAllModel>();
            }
        }

        private void ApplyInvoiceSearch()
        {
            var q = InvoiceTxtSearch?.Text?.Trim() ?? "";
            var list = string.IsNullOrWhiteSpace(q) ? _invoiceListAll : _invoiceListAll.Where(x =>
                (x.Code?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.PartyName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Party?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Status?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            InvoiceGridInvoices.ItemsSource = list;
            InvoiceStatusLabel.Text = $"{list.Count} invoice(s). New/Edit open in new window. Enter or double-click to edit.";
        }

        private async void OnInvoiceSearch(object? sender, RoutedEventArgs e) => await LoadInvoicesAsync();
        private async void OnInvoiceRefresh(object? sender, RoutedEventArgs e) => await LoadInvoicesAsync();

        private void OnInvoiceNew(object? sender, RoutedEventArgs e)
        {
            var page = new Invoices.InvoicePage(InvoiceType, null);
            page.Show();
            page.Closed += (_, _) => _ = LoadInvoicesAsync();
        }

        private void OnInvoiceView(object? sender, RoutedEventArgs e)
        {
            if (InvoiceGridInvoices.SelectedItem is InvoicesAllModel row)
            {
                var report = new Invoices.InvoiceReport(row.ID);
                report.Show();
            }
        }

        private void OnInvoiceEdit(object? sender, RoutedEventArgs e)
        {
            if (InvoiceGridInvoices.SelectedItem is InvoicesAllModel row)
            {
                var page = new Invoices.InvoicePage(InvoiceType, row.ID);
                page.Show();
                page.Closed += (_, _) => _ = LoadInvoicesAsync();
            }
        }

        private void OnInvoicePrint(object? sender, RoutedEventArgs e)
        {
            if (InvoiceGridInvoices.SelectedItem is InvoicesAllModel row)
            {
                var report = new Invoices.InvoiceReport(row.ID);
                report.Show();
            }
        }

        private void OnInvoiceRowDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (InvoiceGridInvoices.SelectedItem is InvoicesAllModel row)
            {
                var page = new Invoices.InvoicePage(InvoiceType, row.ID);
                page.Show();
                page.Closed += (_, _) => _ = LoadInvoicesAsync();
            }
        }

        private void OnSignOutClick(object? sender, RoutedEventArgs e)
        {
            App.Globals.User = new MicroERP.Shared.Models.UsersModel();
            App.Globals.Token = "";
            var login = new LoginPage();
            login.Show();
            Close();
        }
    }
}


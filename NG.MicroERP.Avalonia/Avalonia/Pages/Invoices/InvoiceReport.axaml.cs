using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using NG.MicroERP.Shared.Models;

namespace Avalonia.Pages.Invoices
{
    public partial class InvoiceReport : Window
    {
        private readonly int _invoiceId;
        private InvoicesModel? _data;

        public InvoiceReport() : this(0) { }

        public InvoiceReport(int invoiceId)
        {
            _invoiceId = invoiceId;
            InitializeComponent();
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            await LoadReportAsync();
        }

        private async Task LoadReportAsync()
        {
            try
            {
                _data = await App.Functions.GetAsync<InvoicesModel>($"Invoice/Get/{_invoiceId}", true);
                if (_data?.Invoice == null)
                {
                    TxtLoading.Text = "Invoice not found.";
                    FooterText.Text = "";
                    return;
                }
                BuildReportUi();
            }
            catch (Exception ex)
            {
                TxtLoading.Text = $"Error: {ex.Message}";
                FooterText.Text = "";
            }
        }

        private void BuildReportUi()
        {
            if (_data?.Invoice == null) return;

            var inv = _data.Invoice;
            ReportPanel.Children.Clear();
            TxtLoading.IsVisible = false;

            // Title
            ReportPanel.Children.Add(new TextBlock
            {
                Text = $"{inv.InvoiceType} INVOICE",
                FontSize = 18,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Color.Parse("#e8eaed"))
            });
            ReportPanel.Children.Add(new TextBlock { Text = App.Globals.Organization?.Name ?? "Company", FontSize = 14, Foreground = new SolidColorBrush(Color.Parse("#8b929a")) });

            // Invoice #, Date, Party
            ReportPanel.Children.Add(new TextBlock { Text = $"Invoice No: {inv.Code}", Foreground = new SolidColorBrush(Color.Parse("#e8eaed")), Margin = new Avalonia.Thickness(0, 12, 0, 0) });
            ReportPanel.Children.Add(new TextBlock { Text = $"Date: {inv.TranDate:dd-MMM-yyyy}", Foreground = new SolidColorBrush(Color.Parse("#8b929a")) });
            ReportPanel.Children.Add(new TextBlock { Text = $"Party: {inv.PartyName}", Foreground = new SolidColorBrush(Color.Parse("#e8eaed")), Margin = new Avalonia.Thickness(0, 8, 0, 0) });
            if (!string.IsNullOrWhiteSpace(inv.Description))
                ReportPanel.Children.Add(new TextBlock { Text = $"Description: {inv.Description}", Foreground = new SolidColorBrush(Color.Parse("#8b929a")), TextWrapping = TextWrapping.Wrap });

            // Line items header
            var headerGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("40,2*,*,*,*,*") };
            headerGrid.Children.Add(new TextBlock { Text = "#", Foreground = new SolidColorBrush(Color.Parse("#e8eaed")), FontWeight = FontWeight.SemiBold });
            var h1 = new TextBlock { Text = "Item", Foreground = new SolidColorBrush(Color.Parse("#e8eaed")), FontWeight = FontWeight.SemiBold }; Grid.SetColumn(h1, 1); headerGrid.Children.Add(h1);
            var h2 = new TextBlock { Text = "Qty", Foreground = new SolidColorBrush(Color.Parse("#e8eaed")), FontWeight = FontWeight.SemiBold }; Grid.SetColumn(h2, 2); headerGrid.Children.Add(h2);
            var h3 = new TextBlock { Text = "Unit Price", Foreground = new SolidColorBrush(Color.Parse("#e8eaed")), FontWeight = FontWeight.SemiBold }; Grid.SetColumn(h3, 3); headerGrid.Children.Add(h3);
            var h4 = new TextBlock { Text = "Discount", Foreground = new SolidColorBrush(Color.Parse("#e8eaed")), FontWeight = FontWeight.SemiBold }; Grid.SetColumn(h4, 4); headerGrid.Children.Add(h4);
            var h5 = new TextBlock { Text = "Total", Foreground = new SolidColorBrush(Color.Parse("#5dd0c4")), FontWeight = FontWeight.SemiBold }; Grid.SetColumn(h5, 5); headerGrid.Children.Add(h5);
            ReportPanel.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.Parse("#2a3140")),
                Padding = new Avalonia.Thickness(8, 6),
                CornerRadius = new Avalonia.CornerRadius(4),
                Margin = new Avalonia.Thickness(0, 16, 0, 4),
                Child = headerGrid
            });

            int sr = 1;
            foreach (var item in _data.InvoiceDetails ?? Enumerable.Empty<InvoiceItemReportModel>())
            {
                var name = item.ItemId > 0 ? (item.ItemName ?? "") : (item.ManualItem ?? item.ItemName ?? "Manual");
                var rowGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("40,2*,*,*,*,*") };
                rowGrid.Children.Add(new TextBlock { Text = sr.ToString(), Foreground = new SolidColorBrush(Color.Parse("#8b929a")) });
                var r1 = new TextBlock { Text = name, Foreground = new SolidColorBrush(Color.Parse("#e8eaed")), TextTrimming = TextTrimming.CharacterEllipsis }; Grid.SetColumn(r1, 1); rowGrid.Children.Add(r1);
                var r2 = new TextBlock { Text = item.Qty.ToString("N2"), Foreground = new SolidColorBrush(Color.Parse("#e8eaed")) }; Grid.SetColumn(r2, 2); rowGrid.Children.Add(r2);
                var r3 = new TextBlock { Text = item.UnitPrice.ToString("N2"), Foreground = new SolidColorBrush(Color.Parse("#e8eaed")) }; Grid.SetColumn(r3, 3); rowGrid.Children.Add(r3);
                var r4 = new TextBlock { Text = item.DiscountAmount.ToString("N2"), Foreground = new SolidColorBrush(Color.Parse("#8b929a")) }; Grid.SetColumn(r4, 4); rowGrid.Children.Add(r4);
                var r5 = new TextBlock { Text = item.ItemTotalAmount.ToString("N2"), Foreground = new SolidColorBrush(Color.Parse("#5dd0c4")), FontWeight = FontWeight.SemiBold }; Grid.SetColumn(r5, 5); rowGrid.Children.Add(r5);
                ReportPanel.Children.Add(new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#1e232b")),
                    Padding = new Avalonia.Thickness(8, 6),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Margin = new Avalonia.Thickness(0, 0, 0, 4),
                    Child = rowGrid
                });
                sr++;
            }

            var total = _data.InvoiceDetails?.Sum(d => d.ItemTotalAmount) ?? 0;
            ReportPanel.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.Parse("#161a22")),
                Padding = new Avalonia.Thickness(12, 10),
                CornerRadius = new Avalonia.CornerRadius(8),
                Margin = new Avalonia.Thickness(0, 12, 0, 0),
                Child = new TextBlock
                {
                    Text = $"Total: {total:N2}",
                    FontSize = 16,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = new SolidColorBrush(Color.Parse("#5dd0c4"))
                }
            });

            FooterText.Text = $"Invoice #{inv.Code} · {inv.InvoiceType}";
        }

        private void OnBack(object? sender, RoutedEventArgs e) => Close();

        private void OnPrint(object? sender, RoutedEventArgs e)
        {
            // Avalonia print: could use platform print or save as PDF; for now just close or show message
            FooterText.Text = "Print: use system print (e.g. Ctrl+P) or export from web app.";
        }
    }
}

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NG.MicroERP.Shared.Models;

namespace Avalonia.Pages.Invoices
{
    public partial class ManualLineDialog : Window
    {
        public InvoiceItemReportModel? ResultLine { get; private set; }

        public ManualLineDialog()
        {
            InitializeComponent();
            TxtQty.Text = "1";
            TxtUnitPrice.Text = "0";
        }

        private void OnAdd(object? sender, RoutedEventArgs e)
        {
            var name = TxtItemName?.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            double qty = 1;
            double.TryParse(TxtQty?.Text?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out qty);
            if (qty <= 0) qty = 1;
            double unitPrice = 0;
            double.TryParse(TxtUnitPrice?.Text?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out unitPrice);
            double total = qty * unitPrice;
            ResultLine = new InvoiceItemReportModel
            {
                ItemId = 0,
                ItemName = name,
                ManualItem = name,
                Qty = qty,
                UnitPrice = unitPrice,
                DiscountAmount = 0,
                TaxAmount = 0,
                ItemTotalAmount = total
            };
            Close(true);
        }

        private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
    }
}

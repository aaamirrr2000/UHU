using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MicroERP.Shared.Models;

namespace Avalonia.Pages.Invoices
{
    public partial class LineEditDialog : Window
    {
        private InvoiceItemReportModel? _line;

        public LineEditDialog()
        {
            InitializeComponent();
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Handled) return;
            if (e.Key == Key.Enter)
            {
                OnOk(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                OnCancel(sender, e);
                e.Handled = true;
            }
        }

        public void SetLine(InvoiceItemReportModel line)
        {
            _line = line;
            TxtItemName.Text = line.ItemName ?? "";
            TxtQty.Text = line.Qty.ToString("G", CultureInfo.InvariantCulture);
            TxtUnitPrice.Text = line.UnitPrice.ToString("G", CultureInfo.InvariantCulture);
            TxtDiscount.Text = line.DiscountAmount.ToString("G", CultureInfo.InvariantCulture);
            TxtTax.Text = line.TaxAmount.ToString("G", CultureInfo.InvariantCulture);
        }

        private void OnOk(object? sender, RoutedEventArgs e)
        {
            if (_line == null) { Close(false); return; }
            double qty = 0, unitPrice = 0, discount = 0, tax = 0;
            double.TryParse(TxtQty?.Text?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out qty);
            double.TryParse(TxtUnitPrice?.Text?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out unitPrice);
            double.TryParse(TxtDiscount?.Text?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out discount);
            double.TryParse(TxtTax?.Text?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out tax);
            if (qty <= 0) qty = 1;
            _line.ItemName = TxtItemName?.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(_line.ItemName)) _line.ItemName = "Item";
            _line.ManualItem = _line.ItemName;
            _line.Qty = qty;
            _line.UnitPrice = unitPrice;
            _line.DiscountAmount = discount;
            _line.TaxAmount = tax;
            _line.ItemTotalAmount = (qty * unitPrice) - discount + tax;
            Close(true);
        }

        private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
    }
}


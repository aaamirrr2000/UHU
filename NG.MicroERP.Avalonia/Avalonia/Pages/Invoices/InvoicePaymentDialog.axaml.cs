using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NG.MicroERP.Shared.Models;

namespace Avalonia.Pages.Invoices
{
    public partial class InvoicePaymentDialog : Window
    {
        private ObservableCollection<InvoicePaymentModel> _payments = new();
        private int _invoiceId;

        public InvoicePaymentDialog()
        {
            InitializeComponent();
        }

        public void SetInvoice(int invoiceId, ObservableCollection<InvoicePaymentModel>? payments, string[]? paymentMethods)
        {
            _invoiceId = invoiceId;
            TitleText.Text = $"Payments for Invoice #{invoiceId}";
            if (payments != null)
                _payments = payments;
            else
                _payments = new ObservableCollection<InvoicePaymentModel>();
            ListPayments.ItemsSource = _payments;
            if (paymentMethods is { Length: > 0 })
            {
                ComboMethod.ItemsSource = paymentMethods;
                ComboMethod.SelectedItem = paymentMethods[0];
            }
        }

        private void OnAddPayment(object? sender, RoutedEventArgs e)
        {
            var method = ComboMethod?.SelectedItem as string ?? TxtRef?.Text ?? "CASH";
            var ref_ = TxtRef?.Text ?? "";
            if (!double.TryParse(TxtAmount?.Text ?? "0", System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
                return;
            _payments.Add(new InvoicePaymentModel
            {
                InvoiceId = _invoiceId,
                PaymentMethod = method,
                PaymentRef = ref_,
                Amount = (decimal)amount,
                PaidOn = DateTime.UtcNow
            });
            TxtAmount.Text = "";
            TxtRef.Text = "";
        }

        private void OnDeletePayment(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is InvoicePaymentModel pm)
            {
                _payments.Remove(pm);
            }
        }

        private void OnClose(object? sender, RoutedEventArgs e) => Close();
    }
}

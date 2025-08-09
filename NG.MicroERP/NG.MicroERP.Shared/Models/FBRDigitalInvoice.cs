using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class FbrInvoice
{
    public string InvoiceNumber { get; set; }
    public string POSID { get; set; }
    public string USIN { get; set; }
    public DateTime DateTime { get; set; }
    public string BuyerNTN { get; set; }
    public string BuyerCNIC { get; set; }
    public string BuyerName { get; set; }
    public string BuyerPhoneNumber { get; set; }
    public decimal TotalSaleValue { get; set; }
    public decimal TotalTaxCharged { get; set; }
    public decimal Discount { get; set; }
    public int InvoiceType { get; set; }
    public int PaymentMode { get; set; }
    public List<FbrInvoiceItem> Items { get; set; }
}

public class FbrInvoiceItem
{
    public string ItemCode { get; set; }
    public string ItemName { get; set; }
    public string PCTCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SaleValue { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxCharged { get; set; }
    public decimal TotalAmount { get; set; }
}

public class Fbr
{
    public FbrInvoice FbrInvoice { get; set; }
    public List<FbrInvoiceItem> FbrInvoiceItems { get; set; }
}



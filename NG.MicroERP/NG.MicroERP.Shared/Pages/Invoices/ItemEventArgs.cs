using NG.MicroERP.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Pages.Invoices;

public class QtyChangedEventArgs
{
    public InvoiceItemReportModel Item { get; set; } = new();
    public double NewQty { get; set; }
}

public class ServingSizeChangedEventArgs
{
    public string Size { get; set; } = string.Empty;
    public ItemsModel Item { get; set; } = new();
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class PurchaseOrdersModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; }
    public int CustomerId { get; set; } = 0;
    public string? PONumber { get; set; } = string.Empty;
    public DateTime PODate { get; set; } = DateTime.Today;
    public DateTime ExpectedDelivery { get; set; } = DateTime.Today;
    public string? ReferenceNo { get; set; } = string.Empty;
    public int Status { get; set; } = 0;
    public int Priority { get; set; } = 0;
    public int CurrencyId { get; set; } = 0;
    public double ExchangeRate { get; set; } = 0;
    public string? PaymentTerms { get; set; } = string.Empty;
    public string? DeliveryAddress { get; set; } = string.Empty;
    public string? Remarks { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

}

public class PurchaseOrderDetailsModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int PurchaseOrderId { get; set; } = 0;
    public int ItemId { get; set; } = 0;
    public string? ItemDescription { get; set; } = string.Empty;
    public double Quantity { get; set; } = 0;
    public double UnitPrice { get; set; } = 0;
    public double DiscountPercent { get; set; } = 0;
    public double TaxPercent { get; set; } = 0;
    public DateTime DeliveryDate { get; set; } = DateTime.Today;
    public string? Remarks { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();
}

public class PurchaseOrderChargesModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int PurchaseOrderId { get; set; } = 0;
    public int AccountId { get; set; } = 0;
    public string? ChargeDescription { get; set; } = string.Empty;
    public double Amount { get; set; } = 0;
    public int IsPercentage { get; set; } = 0;
    public double PercentageValue { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

}

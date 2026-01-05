using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class SerializedItemModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? SerialNumber { get; set; } = string.Empty;
    public int ItemId { get; set; } = 0;
    public string? ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public string? Status { get; set; } = "AVAILABLE"; // AVAILABLE, RESERVED, SOLD, DAMAGED, RETURNED
    public DateTime? PurchaseDate { get; set; }
    public double PurchasePrice { get; set; } = 0;
    public DateTime? SaleDate { get; set; }
    public double SalePrice { get; set; } = 0;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public int InvoiceId { get; set; } = 0;
    public string? InvoiceCode { get; set; } = string.Empty;
    public string? BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public string? DisplayName => $"{SerialNumber} - {ItemName}";
}


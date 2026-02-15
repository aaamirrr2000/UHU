using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class InventoryModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public int ItemId { get; set; } = 0;
    public string? ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; } = string.Empty;
    public string? StockCondition { get; set; } = "NEW";
    public double Quantity { get; set; } = 0;
    public double ReservedQuantity { get; set; } = 0;
    public double AvailableQuantity { get; set; } = 0;
    public double AverageCost { get; set; } = 0;
    public double LastCost { get; set; } = 0;
    public DateTime? LastMovementDate { get; set; }
    public double ReorderLevel { get; set; } = 0;
    public double MaxLevel { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public string? DisplayName => $"{LocationName} - {ItemName} ({StockCondition})";
}



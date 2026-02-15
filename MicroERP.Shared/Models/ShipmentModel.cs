using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MicroERP.Shared.Models;

public class ShipmentModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? ShipmentNo { get; set; } = string.Empty;
    public string? ShipmentType { get; set; } = "INCOMING"; // INCOMING, OUTGOING, TRANSFER
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; } = string.Empty;
    public string? ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; } = 0;
    public DateTime? ShipmentDate { get; set; } = DateTime.Today;
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Status { get; set; } = "PENDING"; // PENDING, IN_TRANSIT, RECEIVED, PARTIAL, COMPLETED, CANCELLED
    public int TotalItems { get; set; } = 0;
    public double TotalQuantity { get; set; } = 0;
    public double TotalValue { get; set; } = 0;
    public int CourierId { get; set; } = 0;
    public string? CourierName { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public ObservableCollection<ShipmentDetailModel> Details { get; set; } = new ObservableCollection<ShipmentDetailModel>();

    public string? DisplayName => $"{ShipmentNo} - {ShipmentType}";
}

public class ShipmentDetailModel
{
    public int Id { get; set; } = 0;
    public int ShipmentId { get; set; } = 0;
    public int ItemId { get; set; } = 0;
    public string? ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; } = string.Empty;
    public string? StockCondition { get; set; } = "NEW";
    public double Quantity { get; set; } = 0;
    public double ReceivedQuantity { get; set; } = 0;
    public double UnitPrice { get; set; } = 0;
    public double TotalPrice { get; set; } = 0;
    public string? BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; } = string.Empty;
    public int SeqNo { get; set; } = 1;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
}



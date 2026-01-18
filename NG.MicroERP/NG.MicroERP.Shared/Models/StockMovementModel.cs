using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace NG.MicroERP.Shared.Models;

public class StockMovementModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? MovementNo { get; set; } = string.Empty;
    public string? DocumentType { get; set; } = string.Empty; // GRN (Goods Receipt Note), SIR (Stock Issue Requisition), STN (Stock Transfer Note), ADJ (Adjustment), RTN (Return), DMG (Damage), LSS (Loss)
    public string? MovementType { get; set; } = "TRANSFER"; // TRANSFER, ADJUSTMENT, RETURN, DAMAGE, LOSS
    public int FromLocationId { get; set; } = 0;
    public string? FromLocationName { get; set; } = string.Empty;
    public int ToLocationId { get; set; } = 0;
    public string? ToLocationName { get; set; } = string.Empty;
    public DateTime? MovementDate { get; set; } = DateTime.Today;
    public string? ReferenceNo { get; set; } = string.Empty;
    public string? ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; } = 0;
    public string? Status { get; set; } = "PENDING"; // PENDING, IN_TRANSIT, COMPLETED, CANCELLED
    public string? Reason { get; set; } = string.Empty;
    public int ApprovedBy { get; set; } = 0;
    public string? ApprovedByName { get; set; } = string.Empty;
    public DateTime? ApprovedDate { get; set; }
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public ObservableCollection<StockMovementDetailModel> Details { get; set; } = new ObservableCollection<StockMovementDetailModel>();

    public string? DisplayName => $"{MovementNo} - {MovementType}";
}

public class StockMovementDetailModel
{
    public int Id { get; set; } = 0;
    public int MovementId { get; set; } = 0;
    public int ItemId { get; set; } = 0;
    public string? ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; } = string.Empty;
    public string? StockCondition { get; set; } = "NEW";
    public double Quantity { get; set; } = 0;
    public double UnitCost { get; set; } = 0;
    public double TotalCost { get; set; } = 0;
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class PhysicalCashCountModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public Guid? SessionId { get; set; }
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public string? Locker { get; set; } = string.Empty;
    public DateTime? CountDate { get; set; } = DateTime.Today;
    public decimal Denomination { get; set; } = 0;
    public int Quantity { get; set; } = 0;
    public decimal Amount { get; set; } = 0;
    public string? Notes { get; set; } = string.Empty;
    public string? Comments { get; set; } = string.Empty;
    public int CountedBy { get; set; } = 0;
    public string? CountedByName { get; set; } = string.Empty;
    public int VerifiedBy { get; set; } = 0;
    public string? VerifiedByName { get; set; } = string.Empty;
    public DateTime? VerifiedOn { get; set; }
    public string? Status { get; set; } = "NOT RECONCILED"; // RECONCILED, NOT RECONCILED
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    // UI Display Fields
    public string? StatusDisplay => Status switch
    {
        "RECONCILED" => "Reconciled",
        "NOT RECONCILED" => "Not Reconciled",
        _ => "Unknown"
    };

    public string? DisplayName => $"{LocationName} - {Locker} - {CountDate:yyyy-MM-dd} - {Denomination:C0} x {Quantity} = {Amount:C}";

    public override string ToString()
    {
        return DisplayName ?? string.Empty;
    }
}

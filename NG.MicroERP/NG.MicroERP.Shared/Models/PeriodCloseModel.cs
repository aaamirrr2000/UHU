using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class PeriodCloseModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? PeriodName { get; set; } = string.Empty;
    public string? ModuleType { get; set; } = "ALL"; // ALL, STOCK, INVOICE, CASHBOOK, GENERALLEDGER
    public DateTime? StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; } = DateTime.Today;
    public string? Status { get; set; } = "OPEN"; // OPEN, CLOSE, OPEN_PENDING
    public DateTime? ClosedDate { get; set; }
    public int ClosedBy { get; set; } = 0;
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    // UI Display Fields
    public string? ClosedByName { get; set; } = string.Empty;
    public string? StatusDisplay => Status switch
    {
        "OPEN" => "Open",
        "CLOSE" => "Closed",
        "OPEN_PENDING" => "Open Pending",
        _ => "Unknown"
    };
    public string? DisplayName => $"{PeriodName} ({ModuleType}) - {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";

    public override string ToString()
    {
        return PeriodName ?? string.Empty;
    }
}


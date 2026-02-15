using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class LeaveRequestsModel
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int EmpId { get; set; }
    public string? Fullname { get; set; }
    public int LeaveTypeId { get; set; }
    public string? LeaveName { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
    public string? ContactAddress { get; set; }
    public string? ContactNumber { get; set; }
    public string? Status { get; set; }

    public DateTime? AppliedDate { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public string? Remarks { get; set; }
    public int IsActive { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedFrom { get; set; }

    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; }

    public int IsSoftDeleted { get; set; }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class HiringModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? JobTitle { get; set; } = string.Empty;
    public int DepartmentId { get; set; } = 0;
    public string? DepartmentName { get; set; }
    public int DesignationId { get; set; } = 0;
    public string? DesignationName { get; set; }
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; }
    public string? CandidateName { get; set; } = string.Empty;
    public string? CandidateCNIC { get; set; } = string.Empty;
    public string? CandidateEmail { get; set; } = string.Empty;
    public string? CandidatePhone { get; set; } = string.Empty;
    public string? CandidateAddress { get; set; } = string.Empty;
    public decimal OfferedSalary { get; set; } = 0;
    public DateTime? ApplicationDate { get; set; }
    public DateTime? InterviewDate { get; set; }
    public DateTime? OfferDate { get; set; }
    public DateTime? JoiningDate { get; set; }
    public string? Status { get; set; } = string.Empty; // APPLIED, SHORTLISTED, INTERVIEWED, OFFERED, ACCEPTED, REJECTED, HIRED
    public string? HiringType { get; set; } = string.Empty; // PERMANENT, CONTRACT, TEMPORARY, INTERN
    public int HiredEmployeeId { get; set; } = 0; // Links to Employees table when hired
    public string? Remarks { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Now;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    // UI Display Fields
    public string? EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; } = string.Empty;
}


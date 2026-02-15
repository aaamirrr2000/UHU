using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class EmployeesModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? EmpId { get; set; } = string.Empty;
    public string? Fullname { get; set; } = string.Empty;
    public string? Pic { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Cnic { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? EmpType { get; set; } = string.Empty;
    public int DepartmentId { get; set; } = 0;
    public string? DepartmentName { get; set; }
    public int DesignationId { get; set; } = 0;
    public string? DesignationName { get; set; }
    public int ShiftId { get; set; } = 0;
    public int LocationId { get; set; } = 0;
    public int ParentId { get; set; } = 0;
    public int ExcludeFromAttendance { get; set; } = 0;
    public int IsActive { get; set; }
    
    // HRMS Extended Fields
    public string? Gender { get; set; } = string.Empty;
    public string? MaritalStatus { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Mobile { get; set; } = string.Empty;
    public string? City { get; set; } = string.Empty;
    public string? Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; } = string.Empty;
    public DateTime? HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal BasicSalary { get; set; } = 0;
    public int BankAccountId { get; set; } = 0;
    public string? BankAccountNumber { get; set; } = string.Empty;
    public string? EmergencyContactName { get; set; } = string.Empty;
    public string? EmergencyContactPhone { get; set; } = string.Empty;
    public string? EmergencyContactRelation { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public bool IsSoftDeleted { get; set; }

    public string? DisplayName =>
            string.Format("{0} ({1}), {2}, {3}",
                Fullname, EmpId, DesignationName, DepartmentName);
}

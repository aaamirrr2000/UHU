using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;
public class UsersModel
{
    public int Id { get; set; } = 0;
    public string? Username { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    public string? UserType { get; set; } = string.Empty;
    public int DarKLightTheme { get; set; } = 1;
    public int EmpId { get; set; } = 0;
    public int GroupId { get; set; } = 0;
    public int LocationId { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public string? Token { get; set; } = string.Empty;
    public string? FullName { get; set; } = string.Empty;
    public string? GroupName { get; set; } = string.Empty;
    public string? Pic { get; set; } = string.Empty;
    public string? LocationName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public int OrganizationId { get; set; } = 0;
    public string? Dashboard { get; set; }=string.Empty;

    public EmployeesModel? SelectedEmployee { get; set; }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class BankModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? BankName { get; set; } = string.Empty;
    public string? BranchCode { get; set; } = string.Empty;
    public string? BranchName { get; set; } = string.Empty;
    public string? AccountTitle { get; set; } = string.Empty;
    public string? AccountNumber { get; set; } = string.Empty;
    public string? IBAN { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public int CityId { get; set; } = 0;
    public string? AreaName { get; set; }
    public int AccountId { get; set; } = 0;
    public string? Account { get; set; }
    public string? Phone { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

}
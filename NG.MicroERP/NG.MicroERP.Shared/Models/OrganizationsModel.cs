using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor.Utilities;

namespace NG.MicroERP.Shared.Models;

public class OrganizationsModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public string? Code { get; set; } = string.Empty;
    public string? EntraId { get; set; } = string.Empty;
    public string? Logo { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = null!;
    public string MenuColor { get; set; } = null!;
    public string? Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public int MaxUsers { get; set; } = 0;
    public double DbSize { get; set; } = 0;
    public string? LoginPic { get; set; } = string.Empty;
    public string? Industry { get; set; } = string.Empty;
    public string? Website { get; set; } = string.Empty;
    public string? TimeZone { get; set; } = string.Empty;
    public int IsVerified { get; set; } = 0;
    public DateTime Expiry { get; set; } = DateTime.Today;
    public int ParentId { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public string? CreatedSource { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public string? UpdatedSource { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    //
    public string? ParentOrganizationName { get; set; } = string.Empty;

}
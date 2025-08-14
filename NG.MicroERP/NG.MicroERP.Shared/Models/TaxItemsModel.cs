using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class TaxItemsModel
{
    public int Id { get; set; } = 0;
    public Guid GUID { get; set; }
    public int OrganizationId { get; set; } = 0;
    public int TaxId { get; set; } = 0;
    public int ItemId { get; set; } = 0;
    public string? InvoiceType { get; set; } = string.Empty;
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

public class TaxItemConfigModel
{
    public int Id { get; set; } = 0;
    public Guid GUID { get; set; }
    public int OrganizationId { get; set; } = 0;
    public int TaxId { get; set; } = 0;
    public string? TaxName { get; set; } = string.Empty;
    public double TaxRate { get; set; } = 0;
    public string? TaxType { get; set; } = string.Empty;
    public int ItemId { get; set; } = 0;
    public string? Name { get; set; } = string.Empty;
    public string? Unit { get; set; } = string.Empty;
    public double CostPrice { get; set; } = 0;
    public double RetailPrice { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public int CategoriesId { get; set; } = 0;
    public string? Description { get; set; } = string.Empty;
    public int IsInventoryItem { get; set; } = 0;
    public string? PCTCode { get; set; } = string.Empty;
    public string? ServingSize { get; set; } = string.Empty;
    public int IsActive { get; set; } = 0;
    public int IsSoftDeleted { get; set; } = 0;

}

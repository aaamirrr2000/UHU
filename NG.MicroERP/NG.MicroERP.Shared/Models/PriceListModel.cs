using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class PriceListModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public int ItemId { get; set; } = 0;
    public string? ItemName { get; set; }
    public string? PriceListName { get; set; } = string.Empty;  // Stores TypeCode.ListValue where ListName='PRICE LIST' (e.g., 'INDIVIDUAL CUSTOMERS', 'DEALERS')
    public int MinQuantity { get; set; } = 0;
    public double OneQuantityPrice { get; set; } = 0;
    public double MinQuantityPrice { get; set; } = 0;
    public DateTime? EffectiveDate { get; set; } = DateTime.Today;
    public DateTime? ExpiryDate { get; set; } = DateTime.Today;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public int UpdatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

}
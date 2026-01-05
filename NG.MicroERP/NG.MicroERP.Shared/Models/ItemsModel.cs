using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class ItemsModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? Pic { get; set; } = string.Empty;
    public string? Code { get; set; } = string.Empty;
    public string? HsCode { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public double MinQty { get; set; } = 0;
    public double MaxQty { get; set; } = 0;
    public double ReorderQty { get; set; } = 0;
    public double DefaultDiscount { get; set; } = 0;
    public double CostPrice { get; set; } = 0;
    public double BasePrice { get; set; } = 0;
    public int CategoryId { get; set; } = 0;
    public string? StockType { get; set; } = string.Empty;
    public string? SaleType { get; set; } = string.Empty;
    public string? Unit { get; set; } = string.Empty;
    public int TaxRuleId { get; set; }
    public string? ServingSize { get; set; }
    public int IsFavorite { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int Rating { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public int Recent_Sales_Volume { get; set; } = 0;

    public List<ServingSizeModel> ServingSizes { get; set; } = new();
    public string? CategoryCode { get; set; } = string.Empty;
    public string? CategoryName { get; set; } = string.Empty;
    public double Qty { get; set; } = 0;
    public int Person { get; set; } = 0;
    public double RetailPrice { get; set; } = 0; // Calculated retail price (base + tax - discount per unit)
    public string? DisplayName =>
        string.Format("{0} ({1})",
            Name, CategoryName);

}

public class ServingSizeModel
{
    public string? Size { get; set; } = String.Empty;
    public double Price { get; set; }= 0;
    public string? Pic { get; set; } = String.Empty;
}
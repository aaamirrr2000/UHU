using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class ItemsModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Pic { get; set; } = string.Empty;
    public string? Code { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public double MinQty { get; set; } = 0;
    public double MaxQty { get; set; } = 0;
    public double Discount { get; set; } = 0;
    public double CostPrice { get; set; } = 0;
    public double RetailPrice { get; set; } = 0;
    public int CategoriesId { get; set; } = 0;
    public string? StockType { get; set; } = string.Empty;
    public string? Unit { get; set; } = string.Empty;
    public int IsInventoryItem { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();
    public int Recent_Sales_Volume { get; set; } = 0;

    //
    public double BasePrice { get; set; } // Used for recalculating price
    public string? ServingSize { get; set; } // Serialized string from DB
    public List<ServingSizeModel> ServingSizes { get; set; } = new();
}

public class ServingSizeModel
{
    public string Size { get; set; }
    public double Price { get; set; }
}
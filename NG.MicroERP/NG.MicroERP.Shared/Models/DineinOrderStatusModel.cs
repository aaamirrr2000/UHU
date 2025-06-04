using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class DineinOrderStatusModel
{
    public int id { get; set; } = 0;
    public string? Invoice { get; set; } = string.Empty;
    public int TableId { get; set; } = 0;
    public string? TableNumber { get; set; } = string.Empty;
    public string? TableLocation { get; set; } = string.Empty;
    public int ItemId { get; set; } = 0;
    public string? ItemName { get; set; } = string.Empty;
    public string? StockCondition { get; set; } = string.Empty;
    public string? ServingSize { get; set; } = string.Empty;
    public double Qty { get; set; } = 0;
    public double UnitPrice { get; set; } = 0;
    public double DiscountAmount { get; set; } = 0;
    public double TaxAmount { get; set; } = 0;
    public int BillId { get; set; } = 0;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public string? ItemsInstruction { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
    public int IsTakeAway { get; set; } = 0;
    public string TakeAway { get; set; } = string.Empty;
    public DateTime TranDate { get; set; } = DateTime.Today;
    public int CreatedBy { get; set; } = 0;
    public string? Username { get; set; } = string.Empty;
    public string? Fullname { get; set; } = string.Empty;
    public string? BillInstruction { get; set; } = string.Empty;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public string? LastUpdateBy { get; set; } = string.Empty;
    public string? LastUpdatedName { get; set; } = string.Empty;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int OrganizationId { get; set; } = 0;
    public string? OrganizationName { get; set; } = string.Empty;

}
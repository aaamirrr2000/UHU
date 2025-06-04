using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class StockReceivingModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string FileAttachment { get; set; } = string.Empty;
    public DateTime TranDate { get; set; } = DateTime.Today;
    public int ItemId { get; set; } = 0;
    public string? StockCondition { get; set; } = string.Empty;
    public double Qty { get; set; } = 0;
    public double Price { get; set; } = 0;
    public string? Description { get; set; } = string.Empty;
    public DateTime ExpDate { get; set; } = DateTime.Today;
    public int PartyId { get; set; } = 0;
    public int LocationId { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    public string Item { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string Items { get; set; } = string.Empty;


}
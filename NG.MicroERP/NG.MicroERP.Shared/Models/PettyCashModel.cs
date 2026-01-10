using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class PettyCashModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? SeqNo { get; set; } = string.Empty;
    public string? FileAttachment { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; }
    public string? Source { get; set; }
    public int PartyId { get; set; } = 0;
    public DateTime? TranDate { get; set; } = DateTime.Today;
    public string? Description { get; set; } = string.Empty;
    public double Amount { get; set; } = 0;
    public int AccountId { get; set; } = 0;
    public string? TranType { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; } = string.Empty;
    public string? RefNo { get; set; } = string.Empty;
    public string? TranRef { get; set; } = string.Empty;
    public int BaseCurrencyId { get; set; } = 0;
    public int EnteredCurrencyId { get; set; } = 0;
    public double ExchangeRate { get; set; } = 1.0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    // General Ledger Posting Fields
    public int IsPostedToGL { get; set; } = 0;
    public DateTime? PostedToGLDate { get; set; }
    public int PostedToGLBy { get; set; } = 0;
    public string? GLEntryNo { get; set; } = string.Empty;

}

public class PettyCashReportModel
{
    public int Id { get; set; } = 0;
    public string? SeqNo { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public DateTime TranDate { get; set; } = DateTime.Today;
    public string? Description { get; set; } = string.Empty;
    public double Amount { get; set; } = 0;
    public int AccountId { get; set; } = 0;
    public string? AccountName { get; set; } = string.Empty;
    public string? TranType { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; } = string.Empty;
    public string? RefNo { get; set; } = string.Empty;
    public string? TranRef { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public int CreatedBy { get; set; } = 0;
    public string? CreatedFrom { get; set; } = string.Empty;
    public string? CreatedByUser { get; set; } = string.Empty;
    public string? CreatedByName { get; set; } = string.Empty;
    public DateTime? UpdatedOn { get; set; } = DateTime.Today;
    public int UpdatedBy { get; set; } = 0;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public string? UpdatedByUser { get; set; } = string.Empty;
    public string? UpdatedByName { get; set; } = string.Empty;

}


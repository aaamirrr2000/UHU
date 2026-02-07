using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace NG.MicroERP.Shared.Models;

public class GeneralLedgerHeaderModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? EntryNo { get; set; } = string.Empty;
    public DateTime? EntryDate { get; set; } = DateTime.Today;
    public string? Source { get; set; } = "MANUAL";
    public string? Description { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; } = string.Empty;
    public string? ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; } = 0;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public int BaseCurrencyId { get; set; } = 0;
    public int EnteredCurrencyId { get; set; } = 0;
    public double ExchangeRate { get; set; } = 1.0;
    public double TotalDebit { get; set; } = 0;
    public double TotalCredit { get; set; } = 0;
    public int IsReversed { get; set; } = 0;
    public string? ReversedEntryNo { get; set; } = string.Empty;
    public int IsPosted { get; set; } = 0;
    public DateTime? PostedDate { get; set; }
    public int PostedBy { get; set; } = 0;
    public int IsAdjusted { get; set; } = 0;
    public string? AdjustmentEntryNo { get; set; } = string.Empty;
    public string? FileAttachment { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    // Detail collection
    public ObservableCollection<GeneralLedgerDetailModel> Details { get; set; } = new ObservableCollection<GeneralLedgerDetailModel>();

    // Warnings for missing accounts or configuration issues
    public List<string> Warnings { get; set; } = new List<string>();
    public bool HasWarnings => Warnings != null && Warnings.Any();

    public string? DisplayName =>
        string.Format("{0} - {1} ({2})", EntryNo, Description, EntryDate?.ToString("yyyy-MM-dd") ?? "");

    public override string ToString()
    {
        return EntryNo ?? string.Empty;
    }
}

public class GeneralLedgerDetailModel
{
    private static int lastUsedId = 0;
    public int Id { get; set; } = 0;
    public int HeaderId { get; set; } = 0;
    public int AccountId { get; set; } = 0;
    public string? AccountCode { get; set; } = string.Empty;
    public string? AccountName { get; set; } = string.Empty;
    public string? AccountType { get; set; } = string.Empty;
    /// <summary>Chart of Accounts InterfaceType (e.g. ACCOUNTS RECEIVABLE, TAX, REVENUE) for journal sorting.</summary>
    public string? InterfaceType { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public double DebitAmount { get; set; } = 0;
    public double CreditAmount { get; set; } = 0;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public int CostCenterId { get; set; } = 0;
    public int ProjectId { get; set; } = 0;
    public int CurrencyId { get; set; } = 0;
    public double ExchangeRate { get; set; } = 1.0;
    public int SeqNo { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    // UI helper properties
    public string? AccountDisplayName => $"{AccountCode} - {AccountName}";

    public GeneralLedgerDetailModel()
    {
        Id = --lastUsedId; // Use negative IDs for new items
    }
}

// Legacy model for backward compatibility with report views
public class GeneralLedgerEntriesModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? EntryNo { get; set; } = string.Empty;
    public DateTime? EntryDate { get; set; } = DateTime.Today;
    public int AccountId { get; set; } = 0;
    public string? AccountCode { get; set; } = string.Empty;
    public string? AccountName { get; set; } = string.Empty;
    public string? AccountType { get; set; } = string.Empty;
    public double DebitAmount { get; set; } = 0;
    public double CreditAmount { get; set; } = 0;
    public string? Description { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; } = string.Empty;
    public string? ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; } = 0;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public int CostCenterId { get; set; } = 0;
    public int ProjectId { get; set; } = 0;
    public int CurrencyId { get; set; } = 0;
    public double ExchangeRate { get; set; } = 1.0;
    public int IsReversed { get; set; } = 0;
    public string? ReversedEntryNo { get; set; } = string.Empty;
    public int IsPosted { get; set; } = 0;
    public DateTime? PostedDate { get; set; }
    public int PostedBy { get; set; } = 0;
    public int IsAdjusted { get; set; } = 0;
    public string? AdjustmentEntryNo { get; set; } = string.Empty;
    public string? FileAttachment { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    public string? DisplayName =>
        string.Format("{0} - {1} ({2})", EntryNo, Description, EntryDate?.ToString("yyyy-MM-dd") ?? "");

    public override string ToString()
    {
        return EntryNo ?? string.Empty;
    }
}

public class GeneralLedgerReportModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? EntryNo { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public int AccountId { get; set; } = 0;
    public string? AccountCode { get; set; } = string.Empty;
    public string? AccountName { get; set; } = string.Empty;
    public string? AccountType { get; set; } = string.Empty;
    public double DebitAmount { get; set; } = 0;
    public double CreditAmount { get; set; } = 0;
    public string? Description { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; } = string.Empty;
    public string? ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; } = 0;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public int IsPosted { get; set; } = 0;
    public DateTime? PostedDate { get; set; }
    public int PostedBy { get; set; } = 0;
    public string? PostedByUser { get; set; } = string.Empty;
    public int IsReversed { get; set; } = 0;
    public string? ReversedEntryNo { get; set; } = string.Empty;
    public int IsAdjusted { get; set; } = 0;
    public string? AdjustmentEntryNo { get; set; } = string.Empty;
    public string? FileAttachment { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public string? CreatedByUser { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public string? UpdatedByUser { get; set; } = string.Empty;
}

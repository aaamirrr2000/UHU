using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class InvoiceMasterReportModel
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string SeqNo { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int PartyId { get; set; }
    public string PartyNameDb { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string PartyPhone { get; set; } = string.Empty;
    public string PartyEmail { get; set; } = string.Empty;
    public string PartyAddress { get; set; } = string.Empty;
    public string? ScenarioId { get; set; } = string.Empty;
    public string? Description_SaleType { get; set; } = string.Empty;
    public string? BuyerType { get; set; } = string.Empty;
    public string? Purpose_TaxContext { get; set; } = string.Empty;
    public int TableId { get; set; }
    public DateTime TranDate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SubTotalAmount { get; set; }
    public decimal TotalChargeAmount { get; set; }
    public decimal InvoiceAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public TimeSpan PreprationTime { get; set; }
    public string ClientComments { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedOn { get; set; }
    public int CreatedBy { get; set; }
    public string Username { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
}

// Keep BillMasterReportModel for backward compatibility
public class BillMasterReportModel : InvoiceMasterReportModel { }

public class InvoiceItemReportModel
{
    public int InvoiceDetailId { get; set; }
    public int InvoiceId { get; set; }
    public string SeqNo { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public bool IsInventoryItem { get; set; }
    public string ServingSize { get; set; } = string.Empty;
    public string StockCondition { get; set; } = string.Empty;
    public string ManualItem { get; set; } = string.Empty;
    public int? AccountId { get; set; }
    public double Qty { get; set; }
    public double UnitPrice { get; set; }
    public double DiscountAmount { get; set; }
    public double TaxAmount { get; set; }
    public double ItemTotalAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Person { get; set; } = 0;
    public DateTime TranDate { get; set; }
    public int Rating { get; set; }
    public short IsSoftDeleted { get; set; } = 0;
    public List<TaxItemsModel> AppliedTaxes { get; set; } = new();

}

public class InvoicePaymentsReportModel
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public string SeqNo { get; set; } = string.Empty;
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentRef { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public DateTime PaidOn { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class InvoiceChargesReportModel
{
    public int ChargeId { get; set; }
    public int InvoiceId { get; set; }
    public string SeqNo { get; set; } = string.Empty;
    public int ChargeRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string ChargeCategory { get; set; } = string.Empty;
    public string AmountType { get; set; } = string.Empty;
    public double Rate { get; set; }
    public double AppliedAmount { get; set; }
    public int SequenceOrder { get; set; }
    public string CalculationBase { get; set; } = string.Empty;
}

public class InvoiceTaxesReportModel
{
    public int TaxId { get; set; }
    public int InvoiceId { get; set; }
    public int InvoiceDetailId { get; set; }
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public int CalculationOrder { get; set; }
}

// Comprehensive Invoice Report Model
public class InvoiceCompleteReportModel
{
    public InvoiceMasterReportModel Invoice { get; set; } = new();
    public List<InvoiceItemReportModel> Details { get; set; } = new();
    public List<InvoiceChargesReportModel> Charges { get; set; } = new();
    public List<InvoicePaymentsReportModel> Payments { get; set; } = new();
    public List<InvoiceTaxesReportModel> Taxes { get; set; } = new();
}

// Keep old models for backward compatibility
public class BillPaymentsReportModel : InvoicePaymentsReportModel 
{
    public int BillId { get => InvoiceId; set => InvoiceId = value; }
}

public class BillChargesReportModel : InvoiceChargesReportModel
{
    public int BillChargeId { get => ChargeId; set => ChargeId = value; }
    public int BillId { get => InvoiceId; set => InvoiceId = value; }
    public string RuleType { get; set; } = string.Empty;
    public double CalculatedAmount { get => AppliedAmount; set => AppliedAmount = value; }
}

// Manual Line Entry Data Model
public class ManualLineData
{
    public string ItemName { get; set; } = string.Empty;
    public int? AccountId { get; set; }
    public double Qty { get; set; } = 1.0;
    public double UnitPrice { get; set; } = 0;
    public double DiscountAmount { get; set; } = 0;
    public double TaxAmount { get; set; } = 0;
    public int? TaxRuleId { get; set; } = null; // Tax rule ID for manual items
}


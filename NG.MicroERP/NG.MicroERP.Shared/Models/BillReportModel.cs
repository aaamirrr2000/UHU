using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class BillMasterReportModel
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string SeqNo { get; set; } = string.Empty;
    public string BillType { get; set; } = string.Empty;
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
    public int TableId { get; set; }
    public DateTime TranDate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SubTotalAmount { get; set; }
    public decimal TotalChargeAmount { get; set; }
    public decimal BillAmount { get; set; }
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

public class BillItemReportModel
{
    public int BillDetailId { get; set; }
    public int BillId { get; set; }
    public string SeqNo { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public bool IsInventoryItem { get; set; }
    public string ServingSize { get; set; } = string.Empty;
    public string StockCondition { get; set; } = string.Empty;
    public double Qty { get; set; }
    public double UnitPrice { get; set; }
    public double DiscountAmount { get; set; }
    public double TaxAmount { get; set; }
    public double ItemTotalAmount { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Person { get; set; } = 0;
    public DateTime TranDate { get; set; }
    public int Rating { get; set; }
    public short IsSoftDeleted { get; set; } = 0;
}

public class BillPaymentsReportModel
{
    public int PaymentId { get; set; }
    public int BillId { get; set; }
    public string SeqNo { get; set; } = string.Empty;
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentRef { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public DateTime PaidOn { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class BillChargesReportModel
{
    public int BillChargeId { get; set; }
    public int BillId { get; set; }
    public string SeqNo { get; set; } = string.Empty;
    public int ChargeRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string AmountType { get; set; } = string.Empty;
    public double Rate { get; set; }
    public double CalculatedAmount { get; set; }
    public int SequenceOrder { get; set; }
    public string CalculationBase { get; set; } = string.Empty;
}

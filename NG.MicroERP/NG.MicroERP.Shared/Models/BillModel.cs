using Org.BouncyCastle.Asn1.Cms;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;
public class BillModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int? OrganizationId { get; set; } = 0;
    public string? SeqNo { get; set; } = string.Empty;
    public string? BillType { get; set; } = string.Empty;
    public string? Source { get; set; } = string.Empty;
    public int SalesId { get; set; } = 0;
    public int TableId { get; set; } = 0;
    public int LocationId { get; set; } = 0;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public string? PartyPhone { get; set; } = string.Empty;
    public string? PartyEmail { get; set; } = string.Empty;
    public string? PartyAddress { get; set; } = string.Empty;
    public DateTime TranDate { get; set; } = DateTime.Today;
    public string ServiceType { get; set; } = string.Empty;
    public int PreprationTime { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal SubTotalAmount { get; set; } = 0;
    public decimal TotalChargeAmount { get; set; } = 0;
    public decimal BillAmount { get; set; } = 0;
    public decimal TotalPaidAmount { get; set; } = 0;
    public string? Description { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
    public string ClientComments { get; set; } = string.Empty;
    public int Rating { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    // Computed
    public decimal BalanceAmount => BillAmount - TotalPaidAmount;

    // UI Display Fields
    public string Location { get; set; } = string.Empty;
    public string Party { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Fullname { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ElapsedTime { get; set; } = "";
}

public class BillDetailModel
{
    private static int lastUsedId = 0;
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int ItemId { get; set; } = 0;
    public string? StockCondition { get; set; } = string.Empty;
    public string? ServingSize { get; set; } = string.Empty;
    public double Qty { get; set; } = 0;
    public double UnitPrice { get; set; } = 0;
    public double DiscountAmount { get; set; } = 0;
    public double TaxAmount { get; set; } = 0;
    public int BillId { get; set; } = 0;
    public DateTime TranDate { get; set; } = DateTime.Today;
    public int IsSoftDeleted { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Person { get; set; } = 0;
    public int Rating { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    // UI & Navigation Fields
    public string Item { get; set; } = string.Empty;
    public string StockType { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;
    public string? SeqNo { get; set; } = string.Empty;
    public string? BillType { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public string? Pic { get; set; } = string.Empty;
    public string? TableName { get; set; } = string.Empty;
    public string? TableLocation { get; set; } = string.Empty;
    public string? BillStatus { get; set; } = string.Empty;
    public string? BillDetailStatus { get; set; } = string.Empty;

    public BillDetailModel()
    {
        Id = lastUsedId++;
    }

    public double Item_Amount => (Qty * UnitPrice) + TaxAmount - DiscountAmount;
}


public class BillsModel
{
    public BillModel Bill { get; set; } = new BillModel();
    public ObservableCollection<BillDetailModel> BillDetails { get; set; } = new();
    public ObservableCollection<BillChargeModel> BillCharges { get; set; } = new();
    public ObservableCollection<BillPaymentModel> BillPayments { get; set; } = new();
}

public class BillChargeModel
{
    public int Id { get; set; }
    public int BillId { get; set; }
    public int ChargeRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = "CHARGE"; // CHARGE or DISCOUNT
    public string AmountType { get; set; } = "FLAT"; // FLAT or PERCENTAGE
    public decimal Rate { get; set; }
    public decimal CalculatedAmount { get; set; }
    public int SequenceOrder { get; set; } = 0;
    public string CalculationBase { get; set; } = "BILLED_AMOUNT";
    public string ChargeCategory { get; set; } = "OTHER"; // NEW FIELD: SERVICE, TAX, DISCOUNT, OTHER
    public short IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();
}

public class BillPaymentModel
{
    public int Id { get; set; }
    public int BillId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentRef { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime PaidOn { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public short IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; }
}


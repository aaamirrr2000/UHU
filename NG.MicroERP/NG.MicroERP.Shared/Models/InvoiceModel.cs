using Org.BouncyCastle.Asn1.Cms;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class InvoicesModel
{
    public InvoiceModel Invoice { get; set; } = new InvoiceModel();
    public ObservableCollection<InvoiceItemReportModel> InvoiceDetails { get; set; } = new();
    public ObservableCollection<InvoiceChargesModel> InvoiceCharges { get; set; } = new();
    public ObservableCollection<InvoicePaymentModel> InvoicePayments { get; set; } = new();
    public ObservableCollection<InvoiceDetailTaxesModel> InvoiceTaxes { get; set; } = new();
}

public class InvoiceModel
{
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? InvoiceType { get; set; } = string.Empty;
    public string? Source { get; set; } = string.Empty;
    public int SalesId { get; set; } = 0;
    public int LocationId { get; set; } = 0;
    public int PartyId { get; set; } = 0;
    public int AccountId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public string? PartyPhone { get; set; } = string.Empty;
    public string? PartyEmail { get; set; } = string.Empty;
    public string? PartyAddress { get; set; } = string.Empty;
    public DateTime? TranDate { get; set; } = DateTime.Today;
    public string? Description { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
    public string? ClientComments { get; set; } = string.Empty;
    public int Rating { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    // UI Display Fields
    public string Location { get; set; } = string.Empty;
    public string Party { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Fullname { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ElapsedTime { get; set; } = "";
    public decimal TotalQty { get; set; } = 0;
    public decimal ChargesAmount { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal InvoiceAmount { get; set; } = 0;
    public decimal BalanceAmount { get; set; } = 0;
    public decimal TotalPayments { get; set; } = 0;
    public decimal TotalInvoiceAmount { get; set; } = 0;

    // Currency Fields
    public int BaseCurrencyId { get; set; } = 0;
    public int EnteredCurrencyId { get; set; } = 0;
    public double ExchangeRate { get; set; } = 1.0;

    // General Ledger Posting Fields
    public int IsPostedToGL { get; set; } = 0;
    public DateTime? PostedToGLDate { get; set; }
    public int PostedToGLBy { get; set; } = 0;
    public string? GLEntryNo { get; set; } = string.Empty;

    // UI/Service Communication Fields (not persisted to DB)
    public bool SkipStockValidation { get; set; } = false;

}

public class InvoiceDetailModel
{
    private static int lastUsedId = 0;
    public int Id { get; set; } = 0;
    public int ItemId { get; set; } = 0;
    public string? StockCondition { get; set; } = string.Empty;
    public string? ServingSize { get; set; } = string.Empty;
    public double Qty { get; set; } = 0;
    public double UnitPrice { get; set; } = 0;
    public double DiscountAmount { get; set; } = 0;
    public int InvoiceId { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Rating { get; set; } = 0;
    public DateTime TranDate { get; set; } = DateTime.Today;
    public int IsSoftDeleted { get; set; } = 0;
  
    // UI & Navigation Fields
    public string Item { get; set; } = string.Empty;
    public string StockType { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;
    public string? Code { get; set; } = string.Empty;
    public string? InvoiceType { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? LocationName { get; set; } = string.Empty;
    public int PartyId { get; set; } = 0;
    public string? PartyName { get; set; } = string.Empty;
    public string? Pic { get; set; } = string.Empty;
    public string? TableName { get; set; } = string.Empty;
    public string? TableLocation { get; set; } = string.Empty;
    public string? InvoiceStatus { get; set; } = string.Empty;
    public string? InvoiceDetailStatus { get; set; } = string.Empty;

    public InvoiceDetailModel()
    {
        Id = lastUsedId++;
    }

    public double ItemTotalAmount { get; set; } = 0;

    public List<InvoiceDetailTaxesModel> AppliedTaxes { get; set; } = new();
}

public class InvoiceChargesModel
{
    public int Id { get; set; } = 0;
    public int InvoiceId { get; set; } = 0;
    public int RulesId { get; set; } = 0;
    public int AccountId { get; set; } = 0;
    public string? ChargeCategory { get; set; } = string.Empty;
    public string? AmountType { get; set; } = string.Empty;
    public double Amount { get; set; } = 0;
    public double AppliedAmount { get; set; } = 0;
    public int IsSoftDeleted { get; set; } = 0;

}

public class InvoicePaymentModel
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int AccountId { get; set; } = 0;
    public string? PaymentMethod { get; set; }
    public string? PaymentRef { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidOn { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public short IsSoftDeleted { get; set; } = 0;
}

public class InvoiceDetailTaxesModel
{
    public int Id { get; set; } = 0;
    public int InvoiceDetailId { get; set; } = 0;
    public string? TaxId { get; set; } = string.Empty;
    public double TaxRate { get; set; } = 0;
    public decimal TaxableAmount { get; set; } = 0m;
    public decimal TaxAmount { get; set; } = 0m;
    public int CalculationOrder { get; set; } = 0;


}
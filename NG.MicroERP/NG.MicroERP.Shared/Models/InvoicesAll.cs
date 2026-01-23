using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class InvoicesAllModel
{
    public int ID { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? InvoiceType { get; set; } = string.Empty;
    public string? Source { get; set; } = string.Empty;
    public int SalesId { get; set; } = 0;
    public string? Fullname { get; set; } = string.Empty;
    public int TableId { get; set; } = 0;
    public string? TableName { get; set; } = string.Empty;
    public int LocationId { get; set; } = 0;
    public string? Location { get; set; } = string.Empty;
    public int PartyId { get; set; } = 0;
    public string? Party { get; set; } = string.Empty;
    public string? PartyName { get; set; } = string.Empty;
    public string? ScenarioId { get; set; } = string.Empty;
    public int AccountId { get; set; } = 0;
    public string? AccountName { get; set; } = string.Empty;
    public DateTime TranDate { get; set; } = DateTime.Today;
    public int PreprationTime { get; set; } = 0;
    public double SubTotalAmount { get; set; } = 0;
    public double TotalChargeAmount { get; set; } = 0;
    public double DiscountAmount { get; set; } = 0;
    public double BillAmount { get; set; } = 0;
    public double TotalPaidAmount { get; set; } = 0;
    public double BalanceAmount { get; set; } = 0;
    public string? Description { get; set; } = string.Empty;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? Status { get; set; } = string.Empty;
    public string? Username { get; set; } = string.Empty;
    public string? ClientComments { get; set; } = string.Empty;
    
    // Currency Fields
    public int BaseCurrencyId { get; set; } = 0;
    public int EnteredCurrencyId { get; set; } = 0;
    public double ExchangeRate { get; set; } = 1.0;
    public string? CurrencyCode { get; set; } = string.Empty;
    public string? CurrencyName { get; set; } = string.Empty;

}
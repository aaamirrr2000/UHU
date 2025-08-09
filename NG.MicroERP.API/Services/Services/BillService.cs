using Newtonsoft.Json;

using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services;

public interface IBillService
{
    Task<(bool, List<BillsAllModel>)>? Search(string Criteria = "");
    Task<(bool, BillsModel?)>? Get(int id);
    Task<(bool, string)> Put(BillsModel obj);
    Task<(bool, BillModel, string)> Post(BillsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(BillModel obj);
	Task<(bool, List<BillMasterReportModel>)>? GetBillReport(int id);
	Task<(bool, string)> ClientComments(BillsModel obj);
    Task<(bool, string)> BillStatus(int BillId, string Status, int SoftDelete = 0);
    Task<List<BillChargeModel>> CalculateBillCharges(DateTime StartDate, DateTime EndDate, decimal BilledAmount = 0);
}


public class BillService : IBillService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<BillsAllModel>)>? Search(string Criteria = "")
    {
        string SQL = @"SELECT * FROM Bills";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " WHERE " + Criteria;

        SQL += " ORDER BY Id DESC";

        List<BillsAllModel> result = (await dapper.SearchByQuery<BillsAllModel>(SQL)) ?? new List<BillsAllModel>();
        return result == null || result.Count == 0 ? (false, null!) : (true, result);
    }

    public async Task<(bool, BillsModel?)> Get(int id)
    {
        BillsModel result = new();

        List<BillModel> bill = await dapper.SearchByQuery<BillModel>($"Select * from Bill Where Id={id}") ?? new List<BillModel>();
        List<BillItemReportModel> bill_detail = await dapper.SearchByQuery<BillItemReportModel>($"Select * from BillItemReport Where BillId={id}") ?? new List<BillItemReportModel>();
        List<BillChargeModel> bill_charge = await dapper.SearchByQuery<BillChargeModel>($"Select * from BillCharges Where BillId={id}") ?? new List<BillChargeModel>();
        List<BillPaymentModel> bill_payments = await dapper.SearchByQuery<BillPaymentModel>($"Select * from BillPayments Where BillId={id}") ?? new List<BillPaymentModel>();

        result.Bill = bill.FirstOrDefault();
        result.BillDetails = new ObservableCollection<BillItemReportModel>(bill_detail);
        result.BillCharges = new ObservableCollection<BillChargeModel>(bill_charge);
        result.BillPayments = new ObservableCollection<BillPaymentModel>(bill_payments);

        return (true, result);
    }

    public async Task<(bool, List<BillMasterReportModel>)>? GetBillReport(int id)
    {
		string SQL = $@"Select * from BillReport Where Id={id}";

        List<BillMasterReportModel> result = (await dapper.SearchByQuery<BillMasterReportModel>(SQL)) ?? new List<BillMasterReportModel>();
        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, BillModel, string)> Post(BillsModel obj)
    {
        try
        {
            string Code = dapper.GetCode("INV", "Bill", "SeqNo")!;
            string SQLInsert = $@"
                INSERT INTO Bill (OrganizationId, SeqNo, BillType, Source, SalesId, LocationId, PartyId, PartyName, PartyPhone, PartyEmail,
                PartyAddress, TableId, TranDate, ServiceType, PreprationTime, DiscountAmount, Description, Status, CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted)
                VALUES ({obj.Bill.OrganizationId}, '{Code}', '{obj.Bill.BillType!.ToUpper()}', '{obj.Bill.Source!.ToUpper()}', {obj.Bill.SalesId},
                {obj.Bill.LocationId}, {obj.Bill.PartyId}, '{obj.Bill.PartyName!.ToUpper()}', '{obj.Bill.PartyPhone!.ToUpper()}',
                '{obj.Bill.PartyEmail!.ToUpper()}', '{obj.Bill.PartyAddress!.ToUpper()}', {obj.Bill.TableId},
                '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', '{obj.Bill.ServiceType!.ToUpper()}', '{obj.Bill.PreprationTime}',
                {obj.Bill.DiscountAmount}, '{obj.Bill.Description!.ToUpper()}',
                '{obj.Bill.Status!.ToUpper()}', {obj.Bill.CreatedBy}, '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}',
                '{obj.Bill.CreatedFrom!.ToUpper()}', {obj.Bill.IsSoftDeleted})";
            
            var res = await dapper.Insert(SQLInsert);

            if (res.Item1)
            {
                int insertedId = res.Item2;
                foreach (var detail in obj.BillDetails!)
                {
                    string detailInsert = $@"
                        INSERT INTO BillDetail (ItemId, StockCondition, ServingSize, Qty, UnitPrice, DiscountAmount, TaxAmount,
                        BillId, Description, Status, Person, TranDate, IsSoftDeleted)
                        VALUES ({detail.ItemId}, '{detail.StockCondition!.ToUpper()}', '{detail.ServingSize}', {detail.Qty},
                        {detail.UnitPrice}, {detail.DiscountAmount}, {detail.TaxAmount}, {insertedId}, '{detail.Instructions!.ToUpper()}',
                        '{detail.Status!.ToUpper()}', {detail.Person}, '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {detail.IsSoftDeleted})";
                    await dapper.Insert(detailInsert);
                }

                foreach (var charge in obj.BillCharges)
                {
                    string chargeInsert = $@"
                        INSERT INTO BillCharges (BillId, ChargeRuleId, RuleName, RuleType, AmountType, Rate,
                        CalculatedAmount, SequenceOrder, CalculationBase, ChargeCategory, IsSoftDeleted)
                        VALUES ({insertedId}, {charge.ChargeRuleId}, '{charge.RuleName}', '{charge.RuleType}',
                        '{charge.AmountType}', {charge.Rate}, {charge.CalculatedAmount}, {charge.SequenceOrder},
                        '{charge.CalculationBase}', '{charge.ChargeCategory}', {charge.IsSoftDeleted})";
                    await dapper.Insert(chargeInsert);
                }


                foreach (var payment in obj.BillPayments)
                {
                    string paymentInsert = $@"
                        INSERT INTO BillPayments (BillId, PaymentMethod, PaymentRef, AmountPaid, PaidOn, Notes, IsSoftDeleted)
                        VALUES ({insertedId}, '{payment.PaymentMethod}', '{payment.PaymentRef}', {payment.AmountPaid},
                        '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', '{payment.Notes?.Replace("'", "''")}', 0)";
                    await dapper.Insert(paymentInsert);
                }

                //await DigitalInvoice(obj, insertedId);

                var output = await Search($"Id = {res.Item2}")!;
                List<BillsAllModel> result = output.Item2;

                BillModel result1 = new();
                result1.Id = result.FirstOrDefault()!.ID;
                result1.SeqNo = result.FirstOrDefault()!.SeqNo;

                return (true, result1, "OK");
            }

            return (false, null, "Bill Save Error");

        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task DigitalInvoice(BillsModel obj, int BillId)
    {
        //Generate Json to upload on FBR portal
        var fbrInvoice = new FbrInvoice
        {
            InvoiceNumber = obj.Bill.SeqNo ?? $"BILL-{obj.Bill.Id}",
            POSID = obj.Bill.LocationId.ToString(),
            USIN = $"{obj.Bill.TranDate:yyyyMMdd}-{obj.Bill.Id:D6}",
            DateTime = obj.Bill.TranDate,
            BuyerNTN = "",
            BuyerCNIC = "", 
            BuyerName = obj.Bill.PartyName,
            BuyerPhoneNumber = obj.Bill.PartyPhone,
            TotalSaleValue = obj.Bill.SubTotalAmount,
            TotalTaxCharged = obj.Bill.TotalTaxAmount,
            Discount = obj.Bill.DiscountAmount,
            InvoiceType = 1, // 1 = Standard
            PaymentMode = 1, // 1 = Cash, 2 = Credit Card, etc.
            Items = obj.BillDetails.Select(d => new FbrInvoiceItem
            {
                ItemCode = d.ItemId.ToString(),
                ItemName = d.ItemName,
                PCTCode = "1000.00", // map to real PCT Code if you have it
                Quantity = Convert.ToDecimal(d.Qty),
                UnitPrice = Convert.ToDecimal(d.UnitPrice),
                SaleValue = Convert.ToDecimal(d.Qty * d.UnitPrice),
                TaxRate = Convert.ToDecimal(d.TaxAmount > 0 ? (d.TaxAmount / (d.Qty * d.UnitPrice) * 100) : 0),
                TaxCharged = Convert.ToDecimal(d.TaxAmount),
                TotalAmount = Convert.ToDecimal(d.ItemTotalAmount)
            }).ToList()
        };

        string InvoiceJSon = JsonConvert.SerializeObject(fbrInvoice, Formatting.Indented);

        //Get QRcode from FBR and store with Bill
        
    }

    public async Task<(bool, string)> Put(BillsModel obj)
    {
        try
        {
            string SQLUpdate = $@"
                UPDATE Bill SET OrganizationId = {obj.Bill.OrganizationId}, SeqNo = '{obj.Bill.SeqNo!.ToUpper()}',
                BillType = '{obj.Bill.BillType!.ToUpper()}', Source = '{obj.Bill.Source!.ToUpper()}', SalesId = {obj.Bill.SalesId},
                LocationId = {obj.Bill.LocationId}, PartyId = {obj.Bill.PartyId}, PartyName = '{obj.Bill.PartyName!.ToUpper()}',
                PartyPhone = '{obj.Bill.PartyPhone!.ToUpper()}', PartyEmail = '{obj.Bill.PartyEmail!.ToUpper()}',
                PartyAddress = '{obj.Bill.PartyAddress!.ToUpper()}', TableId = {obj.Bill.TableId},
                TranDate = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', DiscountAmount = {obj.Bill.DiscountAmount},
                Description = '{obj.Bill.Description!.ToUpper()}', Status = '{obj.Bill.Status!.ToUpper()}',
                ServiceType = '{obj.Bill.ServiceType!.ToUpper()}', PreprationTime = '{obj.Bill.PreprationTime}',
                ClientComments = '{obj.Bill.ClientComments}', Rating = {obj.Bill.Rating}, UpdatedBy = {obj.Bill.UpdatedBy},
                UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', UpdatedFrom = '{obj.Bill.UpdatedFrom!.ToUpper()}',
                IsSoftDeleted = {obj.Bill.IsSoftDeleted} WHERE Id = {obj.Bill.Id};";

            await dapper.Update(SQLUpdate);

            //Bill Detail
            await dapper.ExecuteQuery($"DELETE FROM BillDetail WHERE BillId = {obj.Bill.Id}");

            foreach (var detail in obj.BillDetails!)
            {
                string detailInsert = $@"
                    INSERT INTO BillDetail (ItemId, StockCondition, ServingSize, Qty, UnitPrice, DiscountAmount, TaxAmount,
                    BillId, Description, Status, Person, TranDate, IsSoftDeleted)
                    VALUES ({detail.ItemId}, '{detail.StockCondition!.ToUpper()}', '{detail.ServingSize}', {detail.Qty},
                    {detail.UnitPrice}, {detail.DiscountAmount}, {detail.TaxAmount}, {obj.Bill.Id},
                    '{detail.Instructions!.ToUpper()}', '{detail.Status!.ToUpper()}', {detail.Person},
                    '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {detail.IsSoftDeleted})";
                await dapper.Insert(detailInsert);
            }


            //Bill Charge
            await dapper.ExecuteQuery($"DELETE FROM BillCharges WHERE BillId = {obj.Bill.Id}");

            foreach (var charge in obj.BillCharges)
            {
                string chargeInsert = $@"
                        INSERT INTO BillCharges (BillId, ChargeRuleId, RuleName, RuleType, AmountType, Rate,
                        CalculatedAmount, SequenceOrder, CalculationBase, ChargeCategory, IsSoftDeleted)
                        VALUES ({charge.BillId}, {charge.ChargeRuleId}, '{charge.RuleName}', '{charge.RuleType}',
                        '{charge.AmountType}', {charge.Rate}, {charge.CalculatedAmount}, {charge.SequenceOrder},
                        '{charge.CalculationBase}', '{charge.ChargeCategory}', {charge.IsSoftDeleted})";

                await dapper.Insert(chargeInsert);
            }


            //Bill Payments
            await dapper.ExecuteQuery($"DELETE FROM BillPayments WHERE BillId = {obj.Bill.Id}");

            foreach (var payment in obj.BillPayments)
            {
                string paymentInsert = $@"
                    INSERT INTO BillPayments (BillId, PaymentMethod, PaymentRef, AmountPaid, PaidOn, Notes, IsSoftDeleted)
                    VALUES ({obj.Bill.Id}, '{payment.PaymentMethod}', '{payment.PaymentRef}', {payment.AmountPaid},
                    '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', '{payment.Notes?.Replace("'", "''")}', 0)";
                await dapper.Insert(paymentInsert);
            }

            return (true, "OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool, string)> PutStatus(BillModel obj, string Status)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Bill SET
									Status='{Status}'
									UpdatedBy = {obj.UpdatedBy}, 
									UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
									UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}'
							   WHERE Id = {obj.Id};";
            var res = await dapper.Update(SQLUpdate);

            return (true, "OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool, string)> ClientComments(BillsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Bill SET 
					PartyName = '{obj.Bill.PartyName!.ToUpper()}', 
					PartyPhone = '{obj.Bill.PartyPhone!.ToUpper()}', 
					PartyEmail = '{obj.Bill.PartyEmail!}', 
					PartyAddress = '{obj.Bill.PartyAddress!.ToUpper()}', 
					ClientComments = '{obj.Bill.ClientComments}',
					Rating = {obj.Bill.Rating}
				WHERE Id = {obj.Bill.Id};";
            var res = await dapper.Update(SQLUpdate);


			foreach(var i in obj.BillDetails)
			{
                SQLUpdate = $@"UPDATE BillDetail SET Rating = {i.Rating} WHERE BillId = {obj.Bill.Id} and ItemId={i.ItemId};";
                var res1 = await dapper.Update(SQLUpdate);

            }

            return (true, "OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Bill", id);
    }

    public async Task<(bool, string)> SoftDelete(BillModel obj)
    {
        string SQLUpdate = $@"UPDATE Bill SET
                                Status = 'DELETED',
								UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
								UpdatedBy = {obj.UpdatedBy!},
								IsSoftDeleted = 1 
							WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate)!;
    }

    public async Task<(bool, string)> BillStatus(int BillId, string Status, int SoftDelete = 0)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Bill SET 
					                Status = '{Status!.ToUpper()}',
                                    IsSoftDeleted = {SoftDelete}
				                WHERE Id = {BillId};";

            return await dapper.Update(SQLUpdate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<List<BillChargeModel>> CalculateBillCharges(DateTime StartDate, DateTime EndDate, decimal BilledAmount = 0)
    {
        List<BillChargeModel> calculatedCharges = new();
        decimal runningTotal = BilledAmount;

        string query = $@"
                        SELECT 
                            Id AS ChargeRuleId, 
                            RuleName, 
                            RuleType, 
                            AmountType, 
                            Amount AS Rate,
                            SequenceOrder, 
                            CalculationBase, 
                            ISNULL(ChargeCategory, 'OTHER') AS ChargeCategory
                        FROM ChargeRules
                        WHERE 
                            IsActive = 1 
                            AND IsSoftDeleted = 0 
                            AND EffectiveFrom <= '{StartDate.ToString("yyyy-MM-dd")}'
                            AND EffectiveTo >= '{EndDate.ToString("yyyy-MM-dd")}'
                        ORDER BY SequenceOrder;";

        var chargeRules = await dapper.SearchByQuery<BillChargeModel>(query);
        if (chargeRules == null || !chargeRules.Any())
            return calculatedCharges;

        foreach (var rule in chargeRules)
        {
            decimal baseAmount = rule.CalculationBase == "AFTER_PREVIOUS_CHARGES"
                                ? runningTotal
                                : BilledAmount;

            decimal amount = rule.AmountType switch
            {
                "FLAT" => rule.Rate,
                "PERCENTAGE" => Math.Round((baseAmount * rule.Rate / 100), 2),
                _ => 0
            };

            if (rule.RuleType == "DISCOUNT")
                amount = -amount;

            rule.CalculatedAmount = amount;
            calculatedCharges.Add(rule);

            // Update running total only for charges
            if (rule.RuleType == "CHARGE")
                runningTotal += amount;
            else
                runningTotal -= Math.Abs(amount);
        }

        return calculatedCharges;
    }
}

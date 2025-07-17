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
    Task<(bool, string)> Post(BillsModel obj);
    Task<(bool, string)> Put(BillsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(BillModel obj);
	Task<(bool, List<BillMasterReportModel>)>? GetBillReport(int id);
	Task<(bool, string)> ClientComments(BillsModel obj);
    Task<(bool, string)> BillStatus(int BillId, string Status, int SoftDelete = 0);
    Task<List<BillChargeModel>> CalculateBillCharges(decimal billedAmount);
}


public class BillService : IBillService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<BillsAllModel>)>? Search(string Criteria = "")
    {
        string SQL = @"SELECT * FROM Bills";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " WHERE " + Criteria;

        SQL += " ORDER BY Bill.Id DESC";

        List<BillsAllModel> result = (await dapper.SearchByQuery<BillsAllModel>(SQL)) ?? new List<BillsAllModel>();
        return result == null || result.Count == 0 ? (false, null!) : (true, result);
    }

    public async Task<(bool, BillsModel?)> Get(int id)
    {
        BillsModel result = new();

        List<BillModel> bill = await dapper.SearchByQuery<BillModel>($"Select * from Bills Where Id={id}") ?? new List<BillModel>();
        List<BillDetailModel> bill_detail = await dapper.SearchByQuery<BillDetailModel>($"Select * from BillDetail Where BillId={id}") ?? new List<BillDetailModel>();
        List<BillChargeModel> bill_charge = await dapper.SearchByQuery<BillChargeModel>($"Select * from BillCharges Where BillId={id}") ?? new List<BillChargeModel>();
        List<BillPaymentModel> bill_payments = await dapper.SearchByQuery<BillPaymentModel>($"Select * from BillPayments Where BillId={id}") ?? new List<BillPaymentModel>();

        result.Bill = bill.FirstOrDefault();
        result.BillDetails = new ObservableCollection<BillDetailModel>(bill_detail);
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

    public async Task<(bool, string)> Post(BillsModel obj)
    {
        try
        {
            string Code = dapper.GetCode("INV", "Bill", "SeqNo")!;
            string SQLInsert = $@"
                INSERT INTO Bill (OrganizationId, SeqNo, BillType, Source, SalesId, LocationId, PartyId, PartyName, PartyPhone, PartyEmail,
                PartyAddress, TableId, TranDate, ServiceType, PreprationTime, DiscountAmount, SubTotalAmount, TotalChargeAmount,
                BillAmount, TotalPaidAmount, Description, Status, CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted)
                VALUES ({obj.Bill.OrganizationId}, '{Code}', '{obj.Bill.BillType!.ToUpper()}', '{obj.Bill.Source!.ToUpper()}', {obj.Bill.SalesId},
                {obj.Bill.LocationId}, {obj.Bill.PartyId}, '{obj.Bill.PartyName!.ToUpper()}', '{obj.Bill.PartyPhone!.ToUpper()}',
                '{obj.Bill.PartyEmail!.ToUpper()}', '{obj.Bill.PartyAddress!.ToUpper()}', {obj.Bill.TableId},
                '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', '{obj.Bill.ServiceType!.ToUpper()}', '{obj.Bill.PreprationTime}',
                {obj.Bill.DiscountAmount}, {obj.Bill.SubTotalAmount}, {obj.Bill.TotalChargeAmount},
                {obj.Bill.BillAmount}, {obj.Bill.TotalPaidAmount}, '{obj.Bill.Description!.ToUpper()}',
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
                        {detail.UnitPrice}, {detail.DiscountAmount}, {detail.TaxAmount}, {insertedId}, '{detail.Description!.ToUpper()}',
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
            }

            //var output = await Search($"Bill.Id = {res.Item2}")!;
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
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
                SubTotalAmount = {obj.Bill.SubTotalAmount}, TotalChargeAmount = {obj.Bill.TotalChargeAmount},
                BillAmount = {obj.Bill.BillAmount}, TotalPaidAmount = {obj.Bill.TotalPaidAmount},
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
                    '{detail.Description!.ToUpper()}', '{detail.Status!.ToUpper()}', {detail.Person},
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

    public async Task<List<BillChargeModel>> CalculateBillCharges(decimal billedAmount)
    {
        List<BillChargeModel> calculatedCharges = new();
        decimal runningTotal = billedAmount;

        string query = @"
                        SELECT Id AS ChargeRuleId, RuleName, RuleType, AmountType, Amount AS Rate,
                               SequenceOrder, CalculationBase, ISNULL(ChargeCategory, 'OTHER') AS ChargeCategory
                        FROM ChargeRules
                        WHERE IsActive = 1 AND IsSoftDeleted = 0
                        ORDER BY SequenceOrder";

        var chargeRules = await dapper.SearchByQuery<BillChargeModel>(query);
        if (chargeRules == null || !chargeRules.Any())
            return calculatedCharges;

        foreach (var rule in chargeRules)
        {
            decimal baseAmount = rule.CalculationBase == "AFTER_PREVIOUS_CHARGES"
                                ? runningTotal
                                : billedAmount;

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

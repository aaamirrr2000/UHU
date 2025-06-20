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
    Task<(bool, List<BillModel>)>? Search(string Criteria = "");
    Task<(bool, Bill_And_Bill_Detail_Model?)>? Get(int id);
    Task<(bool, BillModel?, string)> Post(Bill_And_Bill_Detail_Model obj);
    Task<(bool, string)> Put(Bill_And_Bill_Detail_Model obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(BillModel obj);
	Task<(bool, List<BillReportModel>)>? GetBillReport(int id);
	Task<(bool, string)> ClientComments(Bill_And_Bill_Detail_Model obj);
    Task<(bool, string)> GenerateBill(BillModel obj);
    Task<(bool, string)> BillStatus(int BillId, string Status, int SoftDelete = 0);
}


public class BillService : IBillService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<BillModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"

					SELECT
                        bill.ID,
                        bill.seqno,
                        bill.billtype,
                        bill.source,
                        bill.salesid,
                        employees.Fullname,
                        bill.tableid,
                        RestaurantTables.TableNumber AS TableName,
                        bill.locationid,
                        locations.name AS location,
                        bill.partyid,
                        parties.name AS party,
                        bill.partyname,
                        bill.partyphone,
                        bill.partyemail,
                        bill.partyaddress,
                        bill.trandate,
                        bill.ServiceType,
                        bill.ServiceCharge,
                        bill.DiscountAmount,
                        bill.taxamount,

                        ROUND(
                            (
                                ISNULL(ItemTotals.TotalItemAmount, 0)
                                + (ISNULL(ItemTotals.TotalItemAmount, 0) * ISNULL(bill.ServiceCharge, 0) / 100)
                                + (ISNULL(ItemTotals.TotalItemAmount, 0) * ISNULL(bill.taxamount, 0) / 100)
                                - ISNULL(bill.DiscountAmount, 0)
                            ), 2
                        ) AS billamount,

                        bill.paymentmethod,
                        bill.paymentref,
                        bill.paymentamount,
                        bill.Description,
                        bill.CreatedBy,
                        bill.CreatedOn,
                        bill.Status,
                        users.username,
                        bill.ClientComments

                    FROM bill
                    LEFT JOIN locations ON locations.id = bill.locationid
                    LEFT JOIN parties ON parties.id = bill.partyid
                    LEFT JOIN users ON users.id = bill.CreatedBy
                    LEFT JOIN employees ON employees.id = users.empid
                    LEFT JOIN RestaurantTables ON RestaurantTables.id = bill.tableid

                    LEFT JOIN (
                        SELECT
                            billid,
                            SUM(
                                (ISNULL(qty, 0) * ISNULL(unitprice, 0)) 
                                - ISNULL(DiscountAmount, 0) 
                                + ISNULL(TaxAmount, 0)
                            ) AS TotalItemAmount
                        FROM BillDetail
                        WHERE IsSoftDeleted = 0
                        GROUP BY billid
                    ) AS ItemTotals ON ItemTotals.billid = bill.ID

                    WHERE bill.IsSoftDeleted = 0

					";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by bill.Id Desc";

        List<BillModel> result = (await dapper.SearchByQuery<BillModel>(SQL)) ?? new List<BillModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }


    public async Task<(bool, Bill_And_Bill_Detail_Model?)> Get(int id)
    {
        var res = new Bill_And_Bill_Detail_Model();
        BillDetailService billDetailService = new BillDetailService();


        var billResult = await Search($"Bill.Id = {id}")!;
        if (billResult.Item1 == false)
            return (false, null);

        res.Bill = billResult.Item2.FirstOrDefault()!;

        var billDetailResult = await billDetailService.Search($"BillDetail.BillId = {id}")!;
        if (billDetailResult.Item1)
        {
            if (billDetailResult.Item1 && billDetailResult.Item2 != null)
            {
                res.BillDetails.Clear();

                foreach (var item in billDetailResult.Item2)
                {
                    res.BillDetails.Add(item);
                }
            }
        }

        return (true, res);
    }



    public async Task<(bool, List<BillReportModel>)>? GetBillReport(int id)
    {
		string SQL = $@"Select * from BillReport Where Id={id}";

        List<BillReportModel> result = (await dapper.SearchByQuery<BillReportModel>(SQL)) ?? new List<BillReportModel>();
        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, BillModel?, string)> Post(Bill_And_Bill_Detail_Model obj)
    {
        try
        {
            string Code = dapper.GetCode("INV", "Bill", "seqno")!;

            string SQLInsert = $@"INSERT INTO Bill 
								(
									OrganizationId, 
									SeqNo, 
									BillType, 
									Source, 
									SalesId, 
									LocationId, 
									PartyId, 
									PartyName, 
									PartyPhone, 
									PartyEmail, 
									PartyAddress, 
									TableId, 
									TranDate, 
									ServiceCharge,
									DiscountAmount, 
									TaxAmount, 
									PaymentMethod, 
									PaymentRef, 
									PaymentAmount, 
									Description,
									Status,
									ServiceType,
									CreatedBy, 
									CreatedOn, 
									CreatedFrom, 
									IsSoftDeleted
								) 
								VALUES 
								(
									{obj.Bill.OrganizationId},
									'{Code}', 
									'{obj.Bill.BillType!.ToUpper()}', 
									'{obj.Bill.Source!.ToUpper()}', 
									{obj.Bill.SalesId},
									{obj.Bill.LocationId},
									{obj.Bill.PartyId},
									'{obj.Bill.PartyName!.ToUpper()}', 
									'{obj.Bill.PartyPhone!.ToUpper()}', 
									'{obj.Bill.PartyEmail!.ToUpper()}', 
									'{obj.Bill.PartyAddress!.ToUpper()}', 
									{obj.Bill.TableId},
									'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
									{obj.Bill.ServiceCharge},
									{obj.Bill.DiscountAmount},
									{obj.Bill.TaxAmount},
									'{obj.Bill.PaymentMethod!.ToUpper()}', 
									'{obj.Bill.PaymentRef!.ToUpper()}', 
									{obj.Bill.PaymentAmount},
									'{obj.Bill.Description!.ToUpper()}',
									'{obj.Bill.Status!.ToUpper()}',
									'{obj.Bill.ServiceType!.ToUpper()}',
									{obj.Bill.CreatedBy},
									'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
									'{obj.Bill.CreatedFrom!.ToUpper()}', 
									{obj.Bill.IsSoftDeleted}
								)";

            var res = await dapper.Insert(SQLInsert);

			if (res.Item1==true)
			{
				int InsertedId = res.Item2;
                foreach (var sql in obj.BillDetails!)
				{
				   SQLInsert = $@"INSERT INTO BillDetail 
								(
									ItemId, 
									StockCondition, 
									ServingSize, 
									Qty, 
									UnitPrice, 
									DiscountAmount, 
									TaxAmount, 
									BillId, 
									Description, 
									Status,
									Person,
									TranDate, 
									IsSoftDeleted
								) 
								VALUES 
								(
									{sql.ItemId},
									'{sql.StockCondition!.ToUpper()}', 
									'{sql.ServingSize}', 
									{sql.Qty},
									{sql.UnitPrice},
									{sql.DiscountAmount},
									{sql.TaxAmount},
									{res.Item2},
									'{sql.Description!.ToUpper()}', 
									'{sql.Status!.ToUpper()}', 
									{sql.Person},
									'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
									{sql.IsSoftDeleted}
								)";

					var result = await dapper.Insert(SQLInsert);
				}
            }
            var Output = await Search($"bill.id={res.Item2}")!;

            return (true, Output.Item2.FirstOrDefault(), "");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool, string)> Put(Bill_And_Bill_Detail_Model obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Bill SET 
					OrganizationId = {obj.Bill.OrganizationId}, 
					SeqNo = '{obj.Bill.SeqNo!.ToUpper()}', 
					BillType = '{obj.Bill.BillType!.ToUpper()}', 
					Source = '{obj.Bill.Source!.ToUpper()}', 
					SalesId = {obj.Bill.SalesId}, 
					LocationId = {obj.Bill.LocationId}, 
					PartyId = {obj.Bill.PartyId}, 
					PartyName = '{obj.Bill.PartyName!.ToUpper()}', 
					PartyPhone = '{obj.Bill.PartyPhone!.ToUpper()}', 
					PartyEmail = '{obj.Bill.PartyEmail!.ToUpper()}', 
					PartyAddress = '{obj.Bill.PartyAddress!.ToUpper()}', 
					TableId = {obj.Bill.TableId},
					TranDate = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					ServiceCharge = {obj.Bill.ServiceCharge},
					DiscountAmount = {obj.Bill.DiscountAmount}, 
					TaxAmount = {obj.Bill.TaxAmount}, 
					PaymentMethod = '{obj.Bill.PaymentMethod!.ToUpper()}', 
					PaymentRef = '{obj.Bill.PaymentRef!.ToUpper()}', 
					PaymentAmount = {obj.Bill.PaymentAmount}, 
					Description = '{obj.Bill.Description!.ToUpper()}',
					Status = '{obj.Bill.Status!.ToUpper()}',
					ServiceType = '{obj.Bill.ServiceType!.ToUpper()}',
					UpdatedBy = {obj.Bill.UpdatedBy}, 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.Bill.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.Bill.IsSoftDeleted} 
				WHERE Id = {obj.Bill.Id};";


            var res = await dapper.Update(SQLUpdate);

            await dapper.ExecuteQuery($"Delete from BillDetail where billid = {obj.Bill.Id}");

            foreach (var sql in obj.BillDetails!)
            {
                SQLUpdate = $@"UPDATE BillDetail SET 
					ItemId = {sql.ItemId}, 
					StockCondition = '{sql.StockCondition!.ToUpper()}', 
					ServingSize = '{sql.ServingSize!.ToUpper()}', 
					Qty = {sql.Qty}, 
					UnitPrice = {sql.UnitPrice}, 
					DiscountAmount = {sql.DiscountAmount}, 
					TaxAmount = {sql.TaxAmount}, 
					BillId = {sql.BillId}, 
					Description = '{sql.Description!.ToUpper()}', 
					Status = '{sql.Status!.ToUpper()}', 
					Person = {sql.Person},
					TranDate = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					IsSoftDeleted = {sql.IsSoftDeleted}  
				WHERE Id = {sql.Id};";
                var result = await dapper.Insert(SQLUpdate);
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

    public async Task<(bool, string)> ClientComments(Bill_And_Bill_Detail_Model obj)
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
								UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
								UpdatedBy = {obj.UpdatedBy!},
								IsSoftDeleted = 1 
							WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate)!;
    }

    public async Task<(bool, string)> GenerateBill(BillModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Bill SET 
					ServiceCharge = {obj.ServiceCharge},
					DiscountAmount = {obj.DiscountAmount}, 
					TaxAmount = {obj.TaxAmount},
                    Status = 'COMPLETE'
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);

            return (true, "OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
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
}

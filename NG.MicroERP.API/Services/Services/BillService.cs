using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;

using System;
using System.Collections.Generic;
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
						RestaurantTables.TableNumber as TableName,
						bill.locationid,
						locations.name as location,
						bill.partyid,
						parties.name as party,
						bill.partyname,
						bill.partyphone,
						bill.partyemail,
						bill.partyaddress,
						bill.trandate,
						bill.DiscountAmount,
						bill.taxamount,
						bill.billamount,
						bill.paymentmethod,
						bill.paymentref,
						bill.paymentamount,
						bill.Description,
						bill.CreatedBy,
						bill.CreatedOn,
						bill.Status,
						users.username
					FROM bill
					LEFT JOIN locations on locations.id=bill.locationid
					LEFT JOIN parties on parties.id=bill.partyid
					LEFT JOIN users on users.id=bill.CreatedBy
					LEFT JOIN employees on employees.id=users.empid
					LEFT JOIN RestaurantTables on RestaurantTables.id=bill.tableid
					Where bill.IsSoftDeleted=0

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

    public async Task<(bool, Bill_And_Bill_Detail_Model?)>? Get(int id)
    {
        Bill_And_Bill_Detail_Model res = new Bill_And_Bill_Detail_Model();

        var BillResult = Search($"Bill.Id = {id}");
        var BillDetailResult = Search($"Bill.Id = {id}");

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
									DiscountAmount, 
									TaxAmount, 
									BillAmount, 
									PaymentMethod, 
									PaymentRef, 
									PaymentAmount, 
									Description, 
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
									{obj.Bill.DiscountAmount},
									{obj.Bill.TaxAmount},
									{obj.Bill.BillAmount},
									'{obj.Bill.PaymentMethod!.ToUpper()}', 
									'{obj.Bill.PaymentRef!.ToUpper()}', 
									{obj.Bill.PaymentAmount},
									'{obj.Bill.Description!.ToUpper()}', 
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
									IsTakeAway,
									TranDate, 
									IsSoftDeleted
								) 
								VALUES 
								(
									{sql.ItemId},
									'{sql.StockCondition!.ToUpper()}', 
									'{sql.ServingSize!.ToUpper()}', 
									{sql.Qty},
									{sql.UnitPrice},
									{sql.DiscountAmount},
									{sql.TaxAmount},
									{InsertedId},
									'{sql.Description!.ToUpper()}', 
									'{sql.Status!.ToUpper()}', 
									{sql.IsTakeAway},
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
					DiscountAmount = {obj.Bill.DiscountAmount}, 
					TaxAmount = {obj.Bill.TaxAmount}, 
					BillAmount = {obj.Bill.BillAmount}, 
					PaymentMethod = '{obj.Bill.PaymentMethod!.ToUpper()}', 
					PaymentRef = '{obj.Bill.PaymentRef!.ToUpper()}', 
					PaymentAmount = {obj.Bill.PaymentAmount}, 
					Description = '{obj.Bill.Description!.ToUpper()}', 
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
					IsTakeaway = {sql.IsTakeAway},
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

        return await dapper.Update(SQLUpdate);
    }
}

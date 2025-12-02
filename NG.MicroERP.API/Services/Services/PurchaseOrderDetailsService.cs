using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IPurchaseOrderDetailsService
{
    Task<(bool, List<PurchaseOrderDetailsModel>)>? Search(string Criteria = "");
    Task<(bool, PurchaseOrderDetailsModel?)>? Get(int id);
    Task<(bool, PurchaseOrderDetailsModel, string)> Post(PurchaseOrderDetailsModel obj);
    Task<(bool, PurchaseOrderDetailsModel, string)> Put(PurchaseOrderDetailsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PurchaseOrderDetailsModel obj);
}


public class PurchaseOrderDetailsService : IPurchaseOrderDetailsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PurchaseOrderDetailsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM PurchaseOrderDetails Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PurchaseOrderDetailsModel> result = (await dapper.SearchByQuery<PurchaseOrderDetailsModel>(SQL)) ?? new List<PurchaseOrderDetailsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PurchaseOrderDetailsModel?)>? Get(int id)
    {
        PurchaseOrderDetailsModel result = (await dapper.SearchByID<PurchaseOrderDetailsModel>("PurchaseOrderDetails", id)) ?? new PurchaseOrderDetailsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PurchaseOrderDetailsModel, string)> Post(PurchaseOrderDetailsModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO PurchaseOrderDetails 
			(
				PurchaseOrderId, 
				ItemId, 
				ItemDescription, 
				Quantity, 
				UnitPrice, 
				DiscountPercent, 
				TaxPercent, 
				DeliveryDate, 
				Remarks, 
				CreatedBy, 
				CreatedOn, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.PurchaseOrderId},
				{obj.ItemId},
				'{obj.ItemDescription!.ToUpper()}', 
				{obj.Quantity},
				{obj.UnitPrice},
				{obj.DiscountPercent},
				{obj.TaxPercent},
				'{obj.DeliveryDate.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.Remarks!.ToUpper()}', 
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PurchaseOrderDetailsModel> Output = new List<PurchaseOrderDetailsModel>();
                var result = await Search($"id={res.Item2}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                return (false, null!, "Duplicate Record Found.");
            }
        }
        catch (Exception ex)
        {
            return (true, null!, ex.Message);
        }
    }

    public async Task<(bool, PurchaseOrderDetailsModel, string)> Put(PurchaseOrderDetailsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE PurchaseOrderDetails SET 
					PurchaseOrderId = {obj.PurchaseOrderId}, 
					ItemId = {obj.ItemId}, 
					ItemDescription = '{obj.ItemDescription!.ToUpper()}', 
					Quantity = {obj.Quantity}, 
					UnitPrice = {obj.UnitPrice}, 
					DiscountPercent = {obj.DiscountPercent}, 
					TaxPercent = {obj.TaxPercent}, 
					DeliveryDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					Remarks = '{obj.Remarks!.ToUpper()}', 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<PurchaseOrderDetailsModel> Output = new List<PurchaseOrderDetailsModel>();
                var result = await Search($"id={res.Item2}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                return (false, null!, "Duplicate Record Found.");
            }
        }
        catch (Exception ex)
        {
            return (true, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("PurchaseOrderDetails", id);
    }

    public async Task<(bool, string)> SoftDelete(PurchaseOrderDetailsModel obj)
    {
        string SQLUpdate = $@"UPDATE PurchaseOrderDetails SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IPurchaseOrdersService
{
    Task<(bool, List<PurchaseOrdersModel>)>? Search(string Criteria = "");
    Task<(bool, PurchaseOrdersModel?)>? Get(int id);
    Task<(bool, PurchaseOrdersModel, string)> Post(PurchaseOrdersModel obj);
    Task<(bool, PurchaseOrdersModel, string)> Put(PurchaseOrdersModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PurchaseOrdersModel obj);
}


public class PurchaseOrdersService : IPurchaseOrdersService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PurchaseOrdersModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM PurchaseOrders Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PurchaseOrdersModel> result = (await dapper.SearchByQuery<PurchaseOrdersModel>(SQL)) ?? new List<PurchaseOrdersModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PurchaseOrdersModel?)>? Get(int id)
    {
        PurchaseOrdersModel result = (await dapper.SearchByID<PurchaseOrdersModel>("PurchaseOrders", id)) ?? new PurchaseOrdersModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PurchaseOrdersModel, string)> Post(PurchaseOrdersModel obj)
    {

        try
        {
            string Code = dapper.GetCode("", "PurchaseOrders", "PONumber")!;
            string SQLInsert = $@"INSERT INTO PurchaseOrders 
			(
				CustomerId, 
				PONumber, 
				PODate, 
				ExpectedDelivery, 
				ReferenceNo, 
				Status, 
				Priority, 
				CurrencyId, 
				ExchangeRate, 
				PaymentTerms, 
				DeliveryAddress, 
				Remarks, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.CustomerId},
				'{Code}', 
				'{obj.PODate.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.ExpectedDelivery.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.ReferenceNo!.ToUpper()}', 
				{obj.Status},
				{obj.Priority},
				{obj.CurrencyId},
				{obj.ExchangeRate},
				'{obj.PaymentTerms!.ToUpper()}', 
				'{obj.DeliveryAddress!.ToUpper()}', 
				'{obj.Remarks!.ToUpper()}', 
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PurchaseOrdersModel> Output = new List<PurchaseOrdersModel>();
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

    public async Task<(bool, PurchaseOrdersModel, string)> Put(PurchaseOrdersModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE PurchaseOrders SET 
					CustomerId = {obj.CustomerId}, 
					PONumber = '{obj.PONumber!.ToUpper()}', 
					PODate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					ExpectedDelivery = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					ReferenceNo = '{obj.ReferenceNo!.ToUpper()}', 
					Status = {obj.Status}, 
					Priority = {obj.Priority}, 
					CurrencyId = {obj.CurrencyId}, 
					ExchangeRate = {obj.ExchangeRate}, 
					PaymentTerms = '{obj.PaymentTerms!.ToUpper()}', 
					DeliveryAddress = '{obj.DeliveryAddress!.ToUpper()}', 
					Remarks = '{obj.Remarks!.ToUpper()}',  
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<PurchaseOrdersModel> Output = new List<PurchaseOrdersModel>();
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
        return await dapper.Delete("PurchaseOrders", id);
    }

    public async Task<(bool, string)> SoftDelete(PurchaseOrdersModel obj)
    {
        string SQLUpdate = $@"UPDATE PurchaseOrders SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



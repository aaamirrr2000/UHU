using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IPurchaseOrderChargesService
{
    Task<(bool, List<PurchaseOrderChargesModel>)>? Search(string Criteria = "");
    Task<(bool, PurchaseOrderChargesModel?)>? Get(int id);
    Task<(bool, PurchaseOrderChargesModel, string)> Post(PurchaseOrderChargesModel obj);
    Task<(bool, PurchaseOrderChargesModel, string)> Put(PurchaseOrderChargesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PurchaseOrderChargesModel obj);
}


public class PurchaseOrderChargesService : IPurchaseOrderChargesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PurchaseOrderChargesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM PurchaseOrderCharges Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PurchaseOrderChargesModel> result = (await dapper.SearchByQuery<PurchaseOrderChargesModel>(SQL)) ?? new List<PurchaseOrderChargesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PurchaseOrderChargesModel?)>? Get(int id)
    {
        PurchaseOrderChargesModel result = (await dapper.SearchByID<PurchaseOrderChargesModel>("PurchaseOrderCharges", id)) ?? new PurchaseOrderChargesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PurchaseOrderChargesModel, string)> Post(PurchaseOrderChargesModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO PurchaseOrderCharges 
			(
				PurchaseOrderId, 
				AccountId, 
				ChargeDescription, 
				Amount, 
				IsPercentage, 
				PercentageValue, 
				CreatedBy, 
				CreatedOn, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.PurchaseOrderId},
				{obj.AccountId},
				'{obj.ChargeDescription!.ToUpper()}', 
				{obj.Amount},
				{obj.IsPercentage},
				{obj.PercentageValue},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PurchaseOrderChargesModel> Output = new List<PurchaseOrderChargesModel>();
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

    public async Task<(bool, PurchaseOrderChargesModel, string)> Put(PurchaseOrderChargesModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE PurchaseOrderCharges SET 
					PurchaseOrderId = {obj.PurchaseOrderId}, 
					AccountId = {obj.AccountId}, 
					ChargeDescription = '{obj.ChargeDescription!.ToUpper()}', 
					Amount = {obj.Amount}, 
					IsPercentage = {obj.IsPercentage}, 
					PercentageValue = {obj.PercentageValue}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<PurchaseOrderChargesModel> Output = new List<PurchaseOrderChargesModel>();
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
        return await dapper.Delete("PurchaseOrderCharges", id);
    }

    public async Task<(bool, string)> SoftDelete(PurchaseOrderChargesModel obj)
    {
        string SQLUpdate = $@"UPDATE PurchaseOrderCharges SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



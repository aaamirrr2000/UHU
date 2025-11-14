using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IServiceChargesService
{
    Task<(bool, List<ServiceChargesModel>)>? Search(string Criteria = "");
    Task<(bool, ServiceChargesModel?)>? Get(int id);
    Task<(bool, ServiceChargesModel, string)> Post(ServiceChargesModel obj);
    Task<(bool, string)> Put(ServiceChargesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ServiceChargesModel obj);
}


public class ServiceChargesService : IServiceChargesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ServiceChargesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM ServiceCharges Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<ServiceChargesModel> result = (await dapper.SearchByQuery<ServiceChargesModel>(SQL)) ?? new List<ServiceChargesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, ServiceChargesModel?)>? Get(int id)
    {
        ServiceChargesModel result = (await dapper.SearchByID<ServiceChargesModel>("ServiceCharges", id)) ?? new ServiceChargesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, ServiceChargesModel, string)> Post(ServiceChargesModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO ServiceCharges 
			(
				ChargeName, 
				ChargeType, 
				Amount, 
				AppliesTo, 
				EffectiveFrom, 
				EffectiveTo, 
				OrganizationId, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				'{obj.ChargeName!.ToUpper()}', 
				'{obj.ChargeType!.ToUpper()}', 
				{obj.Amount},
				{obj.AppliesTo},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.OrganizationId},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<ServiceChargesModel> Output = new List<ServiceChargesModel>();
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

    public async Task<(bool, string)> Put(ServiceChargesModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE ServiceCharges SET 
					ChargeName = '{obj.ChargeName!.ToUpper()}', 
					ChargeType = '{obj.ChargeType!.ToUpper()}', 
					Amount = {obj.Amount}, 
					AppliesTo = {obj.AppliesTo}, 
					EffectiveFrom = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					EffectiveTo = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					OrganizationId = {obj.OrganizationId}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            return await dapper.Update(SQLUpdate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("ServiceCharges", id);
    }

    public async Task<(bool, string)> SoftDelete(ServiceChargesModel obj)
    {
        string SQLUpdate = $@"UPDATE ServiceCharges SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

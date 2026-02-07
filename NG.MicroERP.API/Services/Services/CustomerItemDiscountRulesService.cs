using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface ICustomerItemDiscountRulesService
{
    Task<(bool, List<CustomerItemDiscountRulesModel>)>? Search(string Criteria = "");
    Task<(bool, CustomerItemDiscountRulesModel?)>? Get(int id);
    Task<(bool, CustomerItemDiscountRulesModel, string)> Post(CustomerItemDiscountRulesModel obj);
    Task<(bool, CustomerItemDiscountRulesModel, string)> Put(CustomerItemDiscountRulesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(CustomerItemDiscountRulesModel obj);
}


public class CustomerItemDiscountRulesService : ICustomerItemDiscountRulesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<CustomerItemDiscountRulesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                          a.*,
                          p.Name as PartyName,
                          i.Name as ItemName,
                          i.Code as ItemCode
                        FROM CustomerItemDiscountRules as a
                        LEFT JOIN Parties as p on p.Id=a.PartyId 
                        LEFT JOIN Items as i on i.Id=a.ItemId
                        WHERE a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by a.Id Desc";

        List<CustomerItemDiscountRulesModel> result = (await dapper.SearchByQuery<CustomerItemDiscountRulesModel>(SQL)) ?? new List<CustomerItemDiscountRulesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, CustomerItemDiscountRulesModel?)>? Get(int id)
    {
        CustomerItemDiscountRulesModel result = (await dapper.SearchByID<CustomerItemDiscountRulesModel>("CustomerItemDiscountRules", id)) ?? new CustomerItemDiscountRulesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, CustomerItemDiscountRulesModel, string)> Post(CustomerItemDiscountRulesModel obj)
    {

        try
        {
            string partyIdValue = obj.PartyId.HasValue ? obj.PartyId.Value.ToString() : "NULL";
            string itemIdValue = obj.ItemId.HasValue ? obj.ItemId.Value.ToString() : "NULL";
            string effectiveToValue = obj.EffectiveTo.HasValue ? $"'{obj.EffectiveTo.Value.ToString("yyyy-MM-dd HH:mm:ss")}'" : "NULL";

            string SQLInsert = $@"INSERT INTO CustomerItemDiscountRules 
			(
				OrganizationId, 
				PartyId, 
				ItemId, 
				AmountType, 
				Amount, 
				EffectiveFrom, 
				EffectiveTo, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom
			) 
			VALUES 
			(
				{obj.OrganizationId},
				{partyIdValue},
				{itemIdValue}, 
				'{obj.AmountType!.ToUpper()}', 
				{obj.Amount}, 
				'{obj.EffectiveFrom.Value.ToString("yyyy-MM-dd HH:mm:ss")}',
				{effectiveToValue},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{(obj.CreatedFrom == null ? "null" : $"'{obj.CreatedFrom.ToUpper()}'")} 
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<CustomerItemDiscountRulesModel> Output = new List<CustomerItemDiscountRulesModel>();
                var result = await Search($"a.id={res.Item2}")!;
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

    public async Task<(bool, CustomerItemDiscountRulesModel, string)> Put(CustomerItemDiscountRulesModel obj)
    {
        try
        {
            string partyIdValue = obj.PartyId.HasValue ? obj.PartyId.Value.ToString() : "NULL";
            string itemIdValue = obj.ItemId.HasValue ? obj.ItemId.Value.ToString() : "NULL";
            string effectiveToValue = obj.EffectiveTo.HasValue ? $"'{obj.EffectiveTo.Value.ToString("yyyy-MM-dd HH:mm:ss")}'" : "NULL";

            string SQLUpdate = $@"UPDATE CustomerItemDiscountRules SET 
					OrganizationId = {obj.OrganizationId}, 
					PartyId = {partyIdValue}, 
					ItemId = {itemIdValue}, 
					AmountType = '{obj.AmountType!.ToUpper()}', 
					Amount = {obj.Amount}, 
					EffectiveFrom = '{obj.EffectiveFrom.Value.ToString("yyyy-MM-dd HH:mm:ss")}', 
					EffectiveTo = {effectiveToValue}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = {(obj.UpdatedFrom == null ? "null" : $"'{obj.UpdatedFrom.ToUpper()}'")} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<CustomerItemDiscountRulesModel> Output = new List<CustomerItemDiscountRulesModel>();
                var result = await Search($"a.id={obj.Id}")!;
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
        return await dapper.Delete("CustomerItemDiscountRules", id);
    }

    public async Task<(bool, string)> SoftDelete(CustomerItemDiscountRulesModel obj)
    {
        string SQLUpdate = $@"UPDATE CustomerItemDiscountRules SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

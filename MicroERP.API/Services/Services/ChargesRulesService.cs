using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;


public interface IChargesRulesService
{
    Task<(bool, List<ChargesRulesModel>)>? Search(string Criteria = "");
    Task<(bool, ChargesRulesModel?)>? Get(int id);
    Task<(bool, ChargesRulesModel, string)> Post(ChargesRulesModel obj);
    Task<(bool, ChargesRulesModel, string)> Put(ChargesRulesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ChargesRulesModel obj);
}


public class ChargesRulesService : IChargesRulesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ChargesRulesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                          a.*,
                          b.Name as Account
                        FROM ChargesRules as a
                        LEFT JOIN ChartOfAccounts as b on b.Id=a.AccountId 
                        WHERE a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by a.Id Desc";

        List<ChargesRulesModel> result = (await dapper.SearchByQuery<ChargesRulesModel>(SQL)) ?? new List<ChargesRulesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, ChargesRulesModel?)>? Get(int id)
    {
        ChargesRulesModel result = (await dapper.SearchByID<ChargesRulesModel>("ChargesRules", id)) ?? new ChargesRulesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, ChargesRulesModel, string)> Post(ChargesRulesModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO ChargesRules 
			(
				OrganizationId, 
				AccountId, 
				AmountType, 
				Amount, 
				ChargeCategory, 
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
				{obj.AccountId},
				'{obj.AmountType!.ToUpper()}', 
				{obj.Amount},
				'{obj.ChargeCategory!.ToUpper()}', 
				'{obj.EffectiveFrom.Value.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.EffectiveTo.Value.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{(obj.CreatedFrom == null ? "null" : $"'{obj.CreatedFrom.ToUpper()}'")} 
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<ChargesRulesModel> Output = new List<ChargesRulesModel>();
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

    public async Task<(bool, ChargesRulesModel, string)> Put(ChargesRulesModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE ChargesRules SET 
					OrganizationId = {obj.OrganizationId}, 
					AccountId = {obj.AccountId}, 
					AmountType = '{obj.AmountType!.ToUpper()}', 
					Amount = {obj.Amount}, 
					ChargeCategory = '{obj.ChargeCategory!.ToUpper()}', 
					EffectiveFrom = '{obj.EffectiveFrom.Value.ToString("yyyy-MM-dd HH:mm:ss")}', 
					EffectiveTo = '{obj.EffectiveTo.Value.ToString("yyyy-MM-dd HH:mm:ss")}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = {(obj.UpdatedFrom == null ? "null" : $"'{obj.UpdatedFrom.ToUpper()}'")} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<ChargesRulesModel> Output = new List<ChargesRulesModel>();
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
        return await dapper.Delete("ChargesRules", id);
    }

    public async Task<(bool, string)> SoftDelete(ChargesRulesModel obj)
    {
        string SQLUpdate = $@"UPDATE ChargesRules SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

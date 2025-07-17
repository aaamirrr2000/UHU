using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IChargeRulesService
{
    Task<(bool, List<ChargeRulesModel>)>? Search(string Criteria = "");
    Task<(bool, ChargeRulesModel?)>? Get(int id);
    Task<(bool, ChargeRulesModel, string)> Post(ChargeRulesModel obj);
    Task<(bool, string)> Put(ChargeRulesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ChargeRulesModel obj);
}


public class ChargeRulesService : IChargeRulesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ChargeRulesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM ChargeRules Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<ChargeRulesModel> result = (await dapper.SearchByQuery<ChargeRulesModel>(SQL)) ?? new List<ChargeRulesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, ChargeRulesModel?)>? Get(int id)
    {
        ChargeRulesModel result = (await dapper.SearchByID<ChargeRulesModel>("ChargeRules", id)) ?? new ChargeRulesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, ChargeRulesModel, string)> Post(ChargeRulesModel obj)
    {

        try
        {

            string Code = dapper.GetCode("XXX", "ChargeRules", "RuleName")!;
            string SQLDuplicate = $@"SELECT * FROM ChargeRules WHERE UPPER(RuleName) = '{obj.RuleName!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO ChargeRules 
			(
				OrganizationId, 
				RuleName, 
				RuleType, 
				AmountType, 
				Amount, 
				AppliesTo, 
				CalculationBase, 
				SequenceOrder, 
				ChargeCategory, 
				EffectiveFrom, 
				EffectiveTo, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.RuleName!.ToUpper()}', 
				'{obj.RuleType!.ToUpper()}', 
				'{obj.AmountType!.ToUpper()}', 
				{obj.Amount},
				{obj.AppliesTo},
				'{obj.CalculationBase!.ToUpper()}', 
				{obj.SequenceOrder},
				'{obj.ChargeCategory!.ToUpper()}', 
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ChargeRulesModel> Output = new List<ChargeRulesModel>();
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

    public async Task<(bool, string)> Put(ChargeRulesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM ChargeRules WHERE UPPER(RuleName) = '{obj.RuleName!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE ChargeRules SET 
					OrganizationId = {obj.OrganizationId}, 
					RuleName = '{obj.RuleName!.ToUpper()}', 
					RuleType = '{obj.RuleType!.ToUpper()}', 
					AmountType = '{obj.AmountType!.ToUpper()}', 
					Amount = {obj.Amount}, 
					AppliesTo = {obj.AppliesTo}, 
					CalculationBase = '{obj.CalculationBase!.ToUpper()}', 
					SequenceOrder = {obj.SequenceOrder}, 
					ChargeCategory = '{obj.ChargeCategory!.ToUpper()}', 
					EffectiveFrom = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					EffectiveTo = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            return await dapper.Update(SQLUpdate, SQLDuplicate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("ChargeRules", id);
    }

    public async Task<(bool, string)> SoftDelete(ChargeRulesModel obj)
    {
        string SQLUpdate = $@"UPDATE ChargeRules SET 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



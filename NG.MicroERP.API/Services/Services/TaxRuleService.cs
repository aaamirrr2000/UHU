
using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

public interface ITaxRuleService
{
    Task<(bool, List<TaxRuleModel>)>? Search(string Criteria = "");
    Task<(bool, TaxRuleModel?)>? Get(int id);
    Task<(bool, TaxRuleModel, string)> Post(TaxRuleModel obj);
    Task<(bool, TaxRuleModel, string)> Put(TaxRuleModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(TaxRuleModel obj);
}


public class TaxRuleService : ITaxRuleService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<TaxRuleModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM TaxRule Where IsSoftDeleted=0";
        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<TaxRuleModel> result = (await dapper.SearchByQuery<TaxRuleModel>(SQL)) ?? new List<TaxRuleModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, TaxRuleModel?)>? Get(int id)
    {
        TaxRuleModel result = (await dapper.SearchByID<TaxRuleModel>("TaxRule", id)) ?? new TaxRuleModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, TaxRuleModel, string)> Post(TaxRuleModel obj)
    {

        try
        {

            string Code = dapper.GetCode("", "TaxRule", "Code")!;
            string SQLDuplicate = $@"SELECT * FROM TaxRule WHERE UPPER(RuleName) = '{obj.RuleName!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO TaxRule 
			(
				OrganizationId, 
				RuleName, 
				AppliesTo, 
				IsRegistered, 
				IsFiler, 
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
				'{obj.RuleName!.ToUpper()}', 
				'{obj.AppliesTo!.ToUpper()}', 
				{obj.IsRegistered},
				{obj.IsFiler},
				'{obj.EffectiveFrom.Value.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.EffectiveTo.Value.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{(obj.CreatedFrom == null ? "null" : $"'{obj.CreatedFrom.ToUpper()}'")}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<TaxRuleModel> Output = new List<TaxRuleModel>();
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

    public async Task<(bool, TaxRuleModel, string)> Put(TaxRuleModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM TaxRule WHERE UPPER(RuleName) = '{obj.RuleName!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE TaxRule SET 
					OrganizationId = {obj.OrganizationId}, 
					RuleName = '{obj.RuleName!.ToUpper()}', 
					AppliesTo = '{obj.AppliesTo!.ToUpper()}', 
					IsRegistered = {obj.IsRegistered}, 
					IsFiler = {obj.IsFiler}, 
					EffectiveFrom = '{obj.EffectiveFrom.Value.ToString("yyyy-MM-dd HH:mm:ss")}', 
					EffectiveTo = '{obj.EffectiveTo.Value.ToString("yyyy-MM-dd HH:mm:ss")}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = {(obj.UpdatedFrom == null ? "null" : $"'{obj.UpdatedFrom.ToUpper()}'")}
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<TaxRuleModel> Output = new List<TaxRuleModel>();
                var result = await Search($"id={obj.Id}")!;
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
        return await dapper.Delete("TaxRule", id);
    }

    public async Task<(bool, string)> SoftDelete(TaxRuleModel obj)
    {
        string SQLUpdate = $@"UPDATE TaxRule SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
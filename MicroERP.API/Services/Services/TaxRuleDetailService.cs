using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;


public interface ITaxRuleDetailService
{
    Task<(bool, List<TaxRuleDetailModel>)>? Search(string Criteria = "");
    Task<(bool, TaxRuleDetailModel?)>? Get(int id);
    Task<(bool, TaxRuleDetailModel, string)> Post(TaxRuleDetailModel obj);
    Task<(bool, TaxRuleDetailModel, string)> Put(TaxRuleDetailModel obj);
    Task<(bool, string)> Delete(int id);
}


public class TaxRuleDetailService : ITaxRuleDetailService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<TaxRuleDetailModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                          a.Id,
                          a.TaxId,
                          a.SequenceNo,
                          a.TaxType,
                          a.TaxBaseType,
                          a.TaxAmount,
                          a.IsRegistered,
                          a.IsFiler,
                          b.TaxName,
                          b.Description,
                          b.ConditionType,
                          c.RuleName,
                          c.AppliesTo,
                          d.Name as Account,
                          a.TaxRuleId,
                          c.EffectiveFrom,
                          c.EffectiveTo
                        FROM TaxRuleDetail as a
                        LEFT JOIN TaxMaster as b on b.Id=a.TaxId
                        LEFT JOIN TaxRule as c on c.Id=a.TaxRuleId
                        LEFT JOIN ChartOfAccounts as d on d.Id=b.AccountId";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " Where " + Criteria;

        SQL += " Order by a.SequenceNo";

        List<TaxRuleDetailModel> result = (await dapper.SearchByQuery<TaxRuleDetailModel>(SQL)) ?? new List<TaxRuleDetailModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    //public async Task<(bool, TaxRuleDetailModel?)>? Get(int id)
    //{
    //    TaxRuleDetailModel result = (await dapper.SearchByID<TaxRuleDetailModel>("TaxRuleDetail", id)) ?? new TaxRuleDetailModel();
    //    if (result == null || result.Id == 0)
    //        return (false, null);
    //    else
    //        return (true, result);
    //}

    public async Task<(bool, TaxRuleDetailModel?)>? Get(int id)
    {
        List<TaxRuleDetailModel> Output = new List<TaxRuleDetailModel>();
        var result = await Search($"a.id={id}")!;
        Output = result.Item2;
        return (true, Output.FirstOrDefault()!);
    }


    public async Task<(bool, TaxRuleDetailModel, string)> Post(TaxRuleDetailModel obj)
    {

        try
        {
            string taxTypeValue = string.IsNullOrWhiteSpace(obj.TaxType) ? "NULL" : $"'{obj.TaxType.ToUpper()}'";
            string taxBaseTypeValue = string.IsNullOrWhiteSpace(obj.TaxBaseType) ? "NULL" : $"'{obj.TaxBaseType.ToUpper()}'";
            string isRegisteredValue = obj.IsRegistered.HasValue ? obj.IsRegistered.Value.ToString() : "NULL";
            string isFilerValue = obj.IsFiler.HasValue ? obj.IsFiler.Value.ToString() : "NULL";
            string taxAmountValue = obj.TaxAmount.HasValue ? obj.TaxAmount.Value.ToString() : "NULL";
            
            string SQLInsert = $@"INSERT INTO TaxRuleDetail 
			(
				TaxRuleId, 
				TaxId, 
				SequenceNo,
				TaxType,
				TaxBaseType,
				TaxAmount,
				IsRegistered,
				IsFiler
			) 
			VALUES 
			(
				{obj.TaxRuleId},
				{obj.TaxId},
				{obj.SequenceNo},
				{taxTypeValue},
				{taxBaseTypeValue},
				{taxAmountValue},
				{isRegisteredValue},
				{isFilerValue}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<TaxRuleDetailModel> Output = new List<TaxRuleDetailModel>();
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

    public async Task<(bool, TaxRuleDetailModel, string)> Put(TaxRuleDetailModel obj)
    {
        try
        {
            string taxTypeValue = string.IsNullOrWhiteSpace(obj.TaxType) ? "NULL" : $"'{obj.TaxType.ToUpper()}'";
            string taxBaseTypeValue = string.IsNullOrWhiteSpace(obj.TaxBaseType) ? "NULL" : $"'{obj.TaxBaseType.ToUpper()}'";
            string isRegisteredValue = obj.IsRegistered.HasValue ? obj.IsRegistered.Value.ToString() : "NULL";
            string isFilerValue = obj.IsFiler.HasValue ? obj.IsFiler.Value.ToString() : "NULL";
            string taxAmountValue = obj.TaxAmount.HasValue ? obj.TaxAmount.Value.ToString() : "NULL";
            
            string SQLUpdate = $@"UPDATE TaxRuleDetail SET 
					TaxRuleId = {obj.TaxRuleId}, 
					TaxId = {obj.TaxId}, 
					SequenceNo = {obj.SequenceNo},
					TaxType = {taxTypeValue},
					TaxBaseType = {taxBaseTypeValue},
					TaxAmount = {taxAmountValue},
					IsRegistered = {isRegisteredValue},
					IsFiler = {isFilerValue}
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<TaxRuleDetailModel> Output = new List<TaxRuleDetailModel>();
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
        return await dapper.Delete("TaxRuleDetail", id);
    }
}




using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IInvoiceChargesRulesService
{
    Task<(bool, List<InvoiceChargesRulesModel>)>? Search(string Criteria = "");
    Task<(bool, InvoiceChargesRulesModel?)>? Get(int id);
    Task<(bool, InvoiceChargesRulesModel, string)> Post(InvoiceChargesRulesModel obj);
    Task<(bool, InvoiceChargesRulesModel, string)> Put(InvoiceChargesRulesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(InvoiceChargesRulesModel obj);
}


public class InvoiceChargesRulesService : IInvoiceChargesRulesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<InvoiceChargesRulesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                          a.*,
                          b.Name as Account
                        FROM InvoiceChargesRules as a
                        LEFT JOIN ChartOfAccounts as b on b.Id=a.AccountId 
                        WHERE a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by a.Id Desc";

        List<InvoiceChargesRulesModel> result = (await dapper.SearchByQuery<InvoiceChargesRulesModel>(SQL)) ?? new List<InvoiceChargesRulesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, InvoiceChargesRulesModel?)>? Get(int id)
    {
        InvoiceChargesRulesModel result = (await dapper.SearchByID<InvoiceChargesRulesModel>("InvoiceChargesRules", id)) ?? new InvoiceChargesRulesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, InvoiceChargesRulesModel, string)> Post(InvoiceChargesRulesModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO InvoiceChargesRules 
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
                List<InvoiceChargesRulesModel> Output = new List<InvoiceChargesRulesModel>();
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

    public async Task<(bool, InvoiceChargesRulesModel, string)> Put(InvoiceChargesRulesModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE InvoiceChargesRules SET 
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
                List<InvoiceChargesRulesModel> Output = new List<InvoiceChargesRulesModel>();
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
        return await dapper.Delete("InvoiceChargesRules", id);
    }

    public async Task<(bool, string)> SoftDelete(InvoiceChargesRulesModel obj)
    {
        string SQLUpdate = $@"UPDATE InvoiceChargesRules SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



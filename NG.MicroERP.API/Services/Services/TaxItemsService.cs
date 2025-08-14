using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface ITaxItemsService
{
    Task<(bool, List<TaxItemsModel>)>? Search(string Criteria = "");
    Task<(bool, List<TaxItemConfigModel>)>? SearchTaxItemConfig(string Criteria = "");
    Task<(bool, TaxItemsModel?)>? Get(int id);
    Task<(bool, TaxItemsModel, string)> Post(TaxItemsModel obj);
    Task<(bool, string)> Put(TaxItemsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(TaxItemsModel obj);
}


public class TaxItemsService : ITaxItemsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<TaxItemsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM TaxItems Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<TaxItemsModel> result = (await dapper.SearchByQuery<TaxItemsModel>(SQL)) ?? new List<TaxItemsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, List<TaxItemConfigModel>)>? SearchTaxItemConfig(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM TaxItemConfig Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<TaxItemConfigModel> result = (await dapper.SearchByQuery<TaxItemConfigModel>(SQL)) ?? new List<TaxItemConfigModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, TaxItemsModel?)>? Get(int id)
    {
        TaxItemsModel result = (await dapper.SearchByID<TaxItemsModel>("TaxItems", id)) ?? new TaxItemsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, TaxItemsModel, string)> Post(TaxItemsModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO TaxItems 
			(
				OrganizationId, 
				TaxId, 
				ItemId,
                InvoiceType,
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				{obj.TaxId},
				{obj.ItemId},
				'{obj.InvoiceType}',
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<TaxItemsModel> Output = new List<TaxItemsModel>();
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

    public async Task<(bool, string)> Put(TaxItemsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE TaxItems SET 
					OrganizationId = {obj.OrganizationId}, 
					TaxId = {obj.TaxId}, 
					ItemId = {obj.ItemId}, 
					Invoicetype = '{obj.InvoiceType}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
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
        return await dapper.Delete("TaxItems", id);
    }

    public async Task<(bool, string)> SoftDelete(TaxItemsModel obj)
    {
        string SQLUpdate = $@"UPDATE TaxItems SET 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

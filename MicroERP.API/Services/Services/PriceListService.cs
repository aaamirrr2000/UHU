using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;


public interface IPriceListService
{
    Task<(bool, List<PriceListModel>)>? Search(string Criteria = "");
    Task<(bool, PriceListModel?)>? Get(int id);
    Task<(bool, PriceListModel?)>? GetByItemAndPriceList(int itemId, string priceListName, int organizationId = 1);
    Task<(bool, PriceListModel, string)> Post(PriceListModel obj);
    Task<(bool, PriceListModel, string)> Put(PriceListModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PriceListModel obj);
}


public class PriceListService : IPriceListService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PriceListModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT a.*, b.Name as ItemName FROM PriceList as a
                       LEFT JOIN Items as b on b.Id=a.ItemId Where a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PriceListModel> result = (await dapper.SearchByQuery<PriceListModel>(SQL)) ?? new List<PriceListModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PriceListModel?)>? Get(int id)
    {
        PriceListModel result = (await dapper.SearchByID<PriceListModel>("PriceList", id)) ?? new PriceListModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, PriceListModel?)>? GetByItemAndPriceList(int itemId, string priceListName, int organizationId = 1)
    {
        string priceListNameEscaped = priceListName?.Replace("'", "''") ?? "";
        string SQL = $@"SELECT a.*, b.Name as ItemName 
                        FROM PriceList as a
                        LEFT JOIN Items as b on b.Id=a.ItemId 
                        WHERE a.ItemId = {itemId} 
                        AND UPPER(a.PriceListName) = UPPER('{priceListNameEscaped}') 
                        AND a.OrganizationId = {organizationId}
                        AND a.IsSoftDeleted = 0
                        AND a.IsActive = 1
                        AND (a.ExpiryDate IS NULL OR a.ExpiryDate >= CAST(GETDATE() AS DATE))
                        AND a.EffectiveDate <= CAST(GETDATE() AS DATE)
                        ORDER BY a.MinQuantity ASC"; // Get the lowest quantity price first
        
        var result = await dapper.SearchByQuery<PriceListModel>(SQL);
        var item = result?.FirstOrDefault(); // Get the first matching record (lowest MinQuantity)
        
        if (item == null || item.Id == 0)
            return (false, null);
        else
            return (true, item);
    }


    public async Task<(bool, PriceListModel, string)> Post(PriceListModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO PriceList 
			(
				OrganizationId, 
				ItemId, 
				PriceListName, 
				MinQuantity, 
				OneQuantityPrice, 
				MinQuantityPrice, 
				EffectiveDate, 
				ExpiryDate, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom
			) 
			VALUES 
			(
				{obj.OrganizationId},
				{obj.ItemId},
				'{obj.PriceListName!.ToUpper()}', 
				{obj.MinQuantity},
				{obj.OneQuantityPrice},
				{obj.MinQuantityPrice},
				'{obj.EffectiveDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.ExpiryDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{(obj.CreatedFrom == null ? "null" : $"'{obj.CreatedFrom.ToUpper()}'")} 
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PriceListModel> Output = new List<PriceListModel>();
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
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, PriceListModel, string)> Put(PriceListModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE PriceList SET 
					OrganizationId = {obj.OrganizationId}, 
					ItemId = {obj.ItemId}, 
					PriceListName = '{obj.PriceListName!.ToUpper()}', 
					MinQuantity = {obj.MinQuantity}, 
					OneQuantityPrice = {obj.OneQuantityPrice}, 
					MinQuantityPrice = {obj.MinQuantityPrice}, 
					EffectiveDate = '{obj.EffectiveDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}', 
					ExpiryDate = '{obj.ExpiryDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = {(obj.UpdatedFrom == null ? "null" : $"'{obj.UpdatedFrom.ToUpper()}'")}, 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<PriceListModel> Output = new List<PriceListModel>();
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
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("PriceList", id);
    }

    public async Task<(bool, string)> SoftDelete(PriceListModel obj)
    {
        string SQLUpdate = $@"UPDATE PriceList SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Services;


public class StockReceivingService : IStockReceivingService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<StockReceivingModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"

                    SELECT
                      a.*,
                      d.name as Item,
                      b.name as Supplier,
                      c.name as Location
                     FROM StockReceiving AS a
                     LEFT JOIN Parties as b on b.id=a.partyid
                     LEFT JOIN Locations as c on c.id=a.locationid
                     LEFT JOIN Items as d on d.id=a.itemid

                    ";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<StockReceivingModel> result = (await dapper.SearchByQuery<StockReceivingModel>(SQL)) ?? new List<StockReceivingModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, StockReceivingModel?)>? Get(int id)
    {
        StockReceivingModel result = (await dapper.SearchByID<StockReceivingModel>("Stock", id)) ?? new StockReceivingModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, StockReceivingModel, string)> Post(StockReceivingModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO Stock 
			(
				OrganizationId, 
				TranDate, 
				ItemId, 
				StockCondition, 
				Qty, 
				Price, 
				Description, 
				ExpDate, 
				PartyId, 
				LocationId, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.ItemId},
				'{obj.StockCondition!.ToUpper()}', 
				{obj.Qty},
				{obj.Price},
				'{obj.Description!.ToUpper()}', 
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.PartyId},
				{obj.LocationId},
				{obj.CreatedBy},
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}' 
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<StockReceivingModel> Output = new List<StockReceivingModel>();
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

    public async Task<(bool, string)> Put(StockReceivingModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Stock SET 
					OrganizationId = {obj.OrganizationId}, 
					TranDate = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					ItemId = {obj.ItemId}, 
					StockCondition = '{obj.StockCondition!.ToUpper()}', 
					Qty = {obj.Qty}, 
					Price = {obj.Price}, 
					Description = '{obj.Description!.ToUpper()}', 
					ExpDate = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					PartyId = {obj.PartyId}, 
					LocationId = {obj.LocationId}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}' 
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
        return await dapper.Delete("Stock", id);
    }

    public async Task<(bool, string)> SoftDelete(StockReceivingModel obj)
    {
        string SQLUpdate = $@"UPDATE Stock SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
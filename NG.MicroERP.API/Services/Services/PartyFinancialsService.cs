using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IPartyFinancialsService
{
    Task<(bool, List<PartyFinancialsModel>)>? Search(string Criteria = "");
    Task<(bool, PartyFinancialsModel?)>? Get(int id);
    Task<(bool, PartyFinancialsModel, string)> Post(PartyFinancialsModel obj);
    Task<(bool, PartyFinancialsModel, string)> Put(PartyFinancialsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PartyFinancialsModel obj);
}


public class PartyFinancialsService : IPartyFinancialsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PartyFinancialsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM PartyFinancials Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PartyFinancialsModel> result = (await dapper.SearchByQuery<PartyFinancialsModel>(SQL)) ?? new List<PartyFinancialsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PartyFinancialsModel?)>? Get(int id)
    {
        PartyFinancialsModel result = (await dapper.SearchByID<PartyFinancialsModel>("PartyFinancials", id)) ?? new PartyFinancialsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PartyFinancialsModel, string)> Post(PartyFinancialsModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO PartyFinancials 
			(
				PartyId, 
				Description, 
				ValueType, 
				Value, 
                PercentageAmount,
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.PartyId},
				'{obj.Description!.ToUpper()}', 
				'{obj.ValueType!.ToUpper()}', 
				{obj.Value},
                '{obj.PercentageAmount}',
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PartyFinancialsModel> Output = new List<PartyFinancialsModel>();
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

    public async Task<(bool, PartyFinancialsModel, string)> Put(PartyFinancialsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE PartyFinancials SET 
					PartyId = {obj.PartyId}, 
					Description = '{obj.Description!.ToUpper()}', 
					ValueType = '{obj.ValueType!.ToUpper()}', 
					Value = {obj.Value},
                    PercentageAmount = '{obj.PercentageAmount}',
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<PartyFinancialsModel> Output = new List<PartyFinancialsModel>();
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
        return await dapper.Delete("PartyFinancials", id);
    }

    public async Task<(bool, string)> SoftDelete(PartyFinancialsModel obj)
    {
        string SQLUpdate = $@"UPDATE PartyFinancials SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}




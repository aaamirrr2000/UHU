using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface ICurrenciesService
{
    Task<(bool, List<CurrenciesModel>)>? Search(string Criteria = "");
    Task<(bool, CurrenciesModel?)>? Get(int id);
    Task<(bool, CurrenciesModel, string)> Post(CurrenciesModel obj);
    Task<(bool, CurrenciesModel, string)> Put(CurrenciesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(CurrenciesModel obj);
}


public class CurrenciesService : ICurrenciesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<CurrenciesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Currencies Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<CurrenciesModel> result = (await dapper.SearchByQuery<CurrenciesModel>(SQL)) ?? new List<CurrenciesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, CurrenciesModel?)>? Get(int id)
    {
        CurrenciesModel result = (await dapper.SearchByID<CurrenciesModel>("Currencies", id)) ?? new CurrenciesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, CurrenciesModel, string)> Post(CurrenciesModel obj)
    {

        try
        {

            string SQLDuplicate = $@"SELECT * FROM Currencies WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Currencies 
			(
				Code, 
				Name, 
				Symbol, 
				Country, 
				IsBaseCurrency, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				'{obj.Code!.ToUpper()}', 
				'{obj.Name!.ToUpper()}', 
				'{obj.Symbol!.ToUpper()}', 
				'{obj.Country!.ToUpper()}', 
				{obj.IsBaseCurrency},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<CurrenciesModel> Output = new List<CurrenciesModel>();
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
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, CurrenciesModel, string)> Put(CurrenciesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Currencies WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE Currencies SET 
					Code = '{obj.Code!.ToUpper()}', 
					Name = '{obj.Name!.ToUpper()}', 
					Symbol = '{obj.Symbol!.ToUpper()}', 
					Country = '{obj.Country!.ToUpper()}', 
					IsBaseCurrency = {obj.IsBaseCurrency}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<CurrenciesModel> Output = new List<CurrenciesModel>();
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
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Currencies", id);
    }

    public async Task<(bool, string)> SoftDelete(CurrenciesModel obj)
    {
        string SQLUpdate = $@"UPDATE Currencies SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

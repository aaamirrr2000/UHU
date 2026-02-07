using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface ITaxService
{
    Task<(bool, List<TaxModel>)>? Search(string Criteria = "");
    Task<(bool, TaxModel?)>? Get(int id);
    Task<(bool, TaxModel, string)> Post(TaxModel obj);
    Task<(bool, TaxModel, string)> Put(TaxModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(TaxModel obj);
}


public class TaxService : ITaxService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<TaxModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Tax Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<TaxModel> result = (await dapper.SearchByQuery<TaxModel>(SQL)) ?? new List<TaxModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, TaxModel?)>? Get(int id)
    {
        TaxModel result = (await dapper.SearchByID<TaxModel>("Tax", id)) ?? new TaxModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, TaxModel, string)> Post(TaxModel obj)
    {

        try
        {
            string Code = dapper.GetCode("TAX", "Tax", "Code")!;
            string SQLDuplicate = $@"SELECT * FROM Tax WHERE UPPER(TaxCode) = '{obj.TaxCode!.ToUpper()}' AND IsSoftDeleted = 0;";
            string SQLInsert = $@"INSERT INTO Tax 
			(
				TaxName, 
				TaxCode, 
				RatePercent, 
				IsCompound, 
				AppliesTo, 
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
				'{obj.TaxName!.ToUpper()}', 
				'{obj.TaxCode!.ToUpper()}', 
				{obj.RatePercent},
				{obj.IsCompound},
				'{obj.AppliesTo!.ToUpper()}',
				'{obj.EffectiveFrom.ToString("yyyy-MM-dd")}',
				'{obj.EffectiveTo.ToString("yyyy-MM-dd")}',
                {obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<TaxModel> Output = new List<TaxModel>();
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

    public async Task<(bool, TaxModel, string)> Put(TaxModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Tax WHERE UPPER(TaxCode) = '{obj.TaxCode!.ToUpper()}' AND Id != {obj.Id} AND IsSoftDeleted = 0;";
            string SQLUpdate = $@"UPDATE Tax SET 
					TaxName = '{obj.TaxName!.ToUpper()}', 
					TaxCode = '{obj.TaxCode!.ToUpper()}', 
					RatePercent = {obj.RatePercent}, 
					IsCompound = {obj.IsCompound}, 
					AppliesTo = '{obj.AppliesTo!.ToUpper()}', 
                    EffectiveFrom = '{obj.EffectiveFrom.ToString("yyyy-MM-dd")}',    
                    EffectiveTo = '{obj.EffectiveTo.ToString("yyyy-MM-dd")}',
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted}, 
					RowVersion = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<TaxModel> Output = new List<TaxModel>();
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
            return (false, null!, ex.Message);
        }

    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Tax", id);
    }

    public async Task<(bool, string)> SoftDelete(TaxModel obj)
    {
        string SQLUpdate = $@"UPDATE Tax SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



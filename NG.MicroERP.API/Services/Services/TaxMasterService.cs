
using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

public interface ITaxMasterService
{
    Task<(bool, List<TaxMasterModel>)>? Search(string Criteria = "");
    Task<(bool, TaxMasterModel?)>? Get(int id);
    Task<(bool, TaxMasterModel, string)> Post(TaxMasterModel obj);
    Task<(bool, string)> Put(TaxMasterModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(TaxMasterModel obj);
}


public class TaxMasterService : ITaxMasterService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<TaxMasterModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM TaxMaster Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<TaxMasterModel> result = (await dapper.SearchByQuery<TaxMasterModel>(SQL)) ?? new List<TaxMasterModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, TaxMasterModel?)>? Get(int id)
    {
        TaxMasterModel result = (await dapper.SearchByID<TaxMasterModel>("TaxMaster", id)) ?? new TaxMasterModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, TaxMasterModel, string)> Post(TaxMasterModel obj)
    {
        try
        {

            string SQLInsert = $@"INSERT INTO TaxMaster 
			(
				OrganizationId, 
				TaxType, 
				TaxName, 
				TaxRate, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId}, 
				'{obj.TaxType!.ToUpper()}', 
				'{obj.TaxName!.ToUpper()}', 
				{obj.TaxRate},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<TaxMasterModel> Output = new List<TaxMasterModel>();
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

    public async Task<(bool, string)> Put(TaxMasterModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE TaxMaster SET 
					OrganizationId = {obj.OrganizationId}, 
					TaxType = '{obj.TaxType!.ToUpper()}', 
					TaxName = '{obj.TaxName!.ToUpper()}', 
					TaxRate = {obj.TaxRate}, 
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
        return await dapper.Delete("TaxMaster", id);
    }

    public async Task<(bool, string)> SoftDelete(TaxMasterModel obj)
    {
        string SQLUpdate = $@"UPDATE TaxMaster SET 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
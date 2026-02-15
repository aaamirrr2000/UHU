
using MicroERP.API.Helper;
using MicroERP.Shared.Models;


public interface ITaxMasterService
{
    Task<(bool, List<TaxMasterModel>)>? Search(string Criteria = "");
    Task<(bool, TaxMasterModel?)>? Get(int id);
    Task<(bool, TaxMasterModel, string)> Post(TaxMasterModel obj);
    Task<(bool, TaxMasterModel, string)> Put(TaxMasterModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(TaxMasterModel obj);
}


public class TaxMasterService : ITaxMasterService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<TaxMasterModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM TaxMaster Where IsSoftDeleted=0";

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
            string SQLDuplicate = $@"SELECT * FROM TaxMaster WHERE UPPER(TaxName) = '{obj.TaxName!.ToUpper()}' AND IsSoftDeleted = 0;";
            string conditionTypeValue = string.IsNullOrWhiteSpace(obj.ConditionType) ? "NULL" : $"'{obj.ConditionType.ToUpper()}'";
            
            string SQLInsert = $@"INSERT INTO TaxMaster 
			(
				OrganizationId, 
				AccountId, 
				TaxName, 
				Description, 
				ConditionType,
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom
			) 
			VALUES 
			(
				{obj.OrganizationId},
				{obj.AccountId},
				'{obj.TaxName!.ToUpper()}', 
				{(obj.Description == null ? "null" : $"'{obj.Description.ToUpper()}'")}, 
				{conditionTypeValue},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{(obj.CreatedFrom == null ? "null" : $"'{obj.CreatedFrom.ToUpper()}'")}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
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

    public async Task<(bool, TaxMasterModel, string)> Put(TaxMasterModel obj)
    {
        try
        {
            string conditionTypeValue = string.IsNullOrWhiteSpace(obj.ConditionType) ? "NULL" : $"'{obj.ConditionType.ToUpper()}'";
            
            string SQLDuplicate = $@"SELECT * FROM TaxMaster WHERE UPPER(TaxName) = '{obj.TaxName!.ToUpper()}' AND Id != {obj.Id} AND IsSoftDeleted = 0;";
            string SQLUpdate = $@"UPDATE TaxMaster SET 
					OrganizationId = {obj.OrganizationId}, 
					AccountId = {obj.AccountId}, 
					TaxName = '{obj.TaxName!.ToUpper()}', 
					Description = {(obj.Description == null ? "null" : $"'{obj.Description.ToUpper()}'")}, 
					ConditionType = {conditionTypeValue},
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = {(obj.UpdatedFrom == null ? "null" : $"'{obj.UpdatedFrom.ToUpper()}'")}
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<TaxMasterModel> Output = new List<TaxMasterModel>();
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
        return await dapper.Delete("TaxMaster", id);
    }

    public async Task<(bool, string)> SoftDelete(TaxMasterModel obj)
    {
        string SQLUpdate = $@"UPDATE TaxMaster SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



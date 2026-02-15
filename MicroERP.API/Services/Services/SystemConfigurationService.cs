using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;

public interface ISystemConfigurationService
{
    Task<(bool, List<SystemConfigurationModel>)>? Search(string Criteria = "");
    Task<(bool, SystemConfigurationModel?)>? Get(int id);
    Task<(bool, List<SystemConfigurationModel>)>? GetByCategory(string category, int organizationId = 1);
    Task<(bool, SystemConfigurationModel, string)> Post(SystemConfigurationModel obj);
    Task<(bool, SystemConfigurationModel, string)> Put(SystemConfigurationModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(SystemConfigurationModel obj);
}

public class SystemConfigurationService : ISystemConfigurationService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<SystemConfigurationModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM SystemConfiguration WHERE IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Category, ConfigKey";

        List<SystemConfigurationModel> result = (await dapper.SearchByQuery<SystemConfigurationModel>(SQL)) ?? new List<SystemConfigurationModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, SystemConfigurationModel?)>? Get(int id)
    {
        SystemConfigurationModel result = (await dapper.SearchByID<SystemConfigurationModel>("SystemConfiguration", id)) ?? new SystemConfigurationModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, List<SystemConfigurationModel>)>? GetByCategory(string category, int organizationId = 1)
    {
        string categoryEscaped = category?.Replace("'", "''") ?? "";
        string SQL = $@"SELECT * FROM SystemConfiguration 
                        WHERE IsSoftDeleted=0 
                        AND Category = '{categoryEscaped}' 
                        AND OrganizationId = {organizationId}
                        Order by ConfigKey";

        List<SystemConfigurationModel> result = (await dapper.SearchByQuery<SystemConfigurationModel>(SQL)) ?? new List<SystemConfigurationModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, SystemConfigurationModel, string)> Post(SystemConfigurationModel obj)
    {
        try
        {
            string categoryEscaped = obj.Category?.Replace("'", "''") ?? "";
            string configKeyEscaped = obj.ConfigKey?.Replace("'", "''") ?? "";
            string configValueEscaped = obj.ConfigValue?.Replace("'", "''") ?? "";
            string descriptionEscaped = obj.Description?.Replace("'", "''") ?? "";
            string createdFromEscaped = obj.CreatedFrom?.Replace("'", "''") ?? "";
            
            string SQLDuplicate = $@"SELECT * FROM SystemConfiguration 
                                    WHERE OrganizationId = {obj.OrganizationId} 
                                    AND Category = '{categoryEscaped}' 
                                    AND ConfigKey = '{configKeyEscaped}';";
            
            string SQLInsert = $@"INSERT INTO SystemConfiguration 
			(
				OrganizationId, 
				Category, 
				ConfigKey, 
				ConfigValue, 
				Description, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{categoryEscaped}', 
				'{configKeyEscaped}', 
				'{configValueEscaped}', 
				'{descriptionEscaped}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{createdFromEscaped}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                // Clear cache when configuration is updated
                SystemConfigurationHelper.ClearCache();
                
                List<SystemConfigurationModel> Output = new List<SystemConfigurationModel>();
                var result = await Search($"Id={res.Item2}")!;
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

    public async Task<(bool, SystemConfigurationModel, string)> Put(SystemConfigurationModel obj)
    {
        try
        {
            string categoryEscaped = obj.Category?.Replace("'", "''") ?? "";
            string configKeyEscaped = obj.ConfigKey?.Replace("'", "''") ?? "";
            string configValueEscaped = obj.ConfigValue?.Replace("'", "''") ?? "";
            string descriptionEscaped = obj.Description?.Replace("'", "''") ?? "";
            string updatedFromEscaped = obj.UpdatedFrom?.Replace("'", "''") ?? "";
            
            string SQLDuplicate = $@"SELECT * FROM SystemConfiguration 
                                    WHERE OrganizationId = {obj.OrganizationId} 
                                    AND Category = '{categoryEscaped}' 
                                    AND ConfigKey = '{configKeyEscaped}' 
                                    AND Id != {obj.Id};";
            
            string SQLUpdate = $@"UPDATE SystemConfiguration SET 
					OrganizationId = {obj.OrganizationId}, 
					Category = '{categoryEscaped}', 
					ConfigKey = '{configKeyEscaped}', 
					ConfigValue = '{configValueEscaped}', 
					Description = '{descriptionEscaped}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{updatedFromEscaped}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                // Clear cache when configuration is updated
                SystemConfigurationHelper.ClearCache();
                
                List<SystemConfigurationModel> Output = new List<SystemConfigurationModel>();
                var result = await Search($"Id={obj.Id}")!;
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
        return await dapper.Delete("SystemConfiguration", id);
    }

    public async Task<(bool, string)> SoftDelete(SystemConfigurationModel obj)
    {
        string SQLUpdate = $@"UPDATE SystemConfiguration SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = {obj.UpdatedBy},
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}


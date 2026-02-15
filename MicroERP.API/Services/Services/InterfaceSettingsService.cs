using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;

public interface IInterfaceSettingsService
{
    Task<(bool, List<InterfaceSettingsModel>)>? Search(string Criteria = "");
    Task<(bool, InterfaceSettingsModel?)>? Get(int id);
    Task<(bool, List<InterfaceSettingsModel>)>? GetByCategory(string category, int organizationId = 1);
    Task<(bool, InterfaceSettingsModel, string)> Post(InterfaceSettingsModel obj);
    Task<(bool, InterfaceSettingsModel, string)> Put(InterfaceSettingsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(InterfaceSettingsModel obj);
}

public class InterfaceSettingsService : IInterfaceSettingsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<InterfaceSettingsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM InterfaceSettings WHERE IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Category, SettingKey";

        List<InterfaceSettingsModel> result = (await dapper.SearchByQuery<InterfaceSettingsModel>(SQL)) ?? new List<InterfaceSettingsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, InterfaceSettingsModel?)>? Get(int id)
    {
        InterfaceSettingsModel result = (await dapper.SearchByID<InterfaceSettingsModel>("InterfaceSettings", id)) ?? new InterfaceSettingsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, List<InterfaceSettingsModel>)>? GetByCategory(string category, int organizationId = 1)
    {
        string categoryEscaped = category?.Replace("'", "''") ?? "";
        string SQL = $@"SELECT * FROM InterfaceSettings 
                        WHERE IsSoftDeleted=0 
                        AND Category = '{categoryEscaped}' 
                        AND OrganizationId = {organizationId}
                        Order by SettingKey";

        List<InterfaceSettingsModel> result = (await dapper.SearchByQuery<InterfaceSettingsModel>(SQL)) ?? new List<InterfaceSettingsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, InterfaceSettingsModel, string)> Post(InterfaceSettingsModel obj)
    {
        try
        {
            string categoryEscaped = obj.Category?.Replace("'", "''") ?? "";
            string settingKeyEscaped = obj.SettingKey?.Replace("'", "''") ?? "";
            string settingValueEscaped = obj.SettingValue?.Replace("'", "''") ?? "";
            string descriptionEscaped = obj.Description?.Replace("'", "''") ?? "";
            string createdFromEscaped = obj.CreatedFrom?.Replace("'", "''") ?? "";
            string dataType = string.IsNullOrWhiteSpace(obj.DataType) ? "STRING" : obj.DataType;
            
            string SQLDuplicate = $@"SELECT * FROM InterfaceSettings 
                                    WHERE OrganizationId = {obj.OrganizationId} 
                                    AND Category = '{categoryEscaped}' 
                                    AND SettingKey = '{settingKeyEscaped}' 
                                    AND IsSoftDeleted = 0;";
            
            string SQLInsert = $@"INSERT INTO InterfaceSettings 
			(
				OrganizationId, 
				Category, 
				SettingKey, 
				SettingValue, 
				DataType,
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
				'{settingKeyEscaped}', 
				'{settingValueEscaped}', 
				'{dataType}',
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
                List<InterfaceSettingsModel> Output = new List<InterfaceSettingsModel>();
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

    public async Task<(bool, InterfaceSettingsModel, string)> Put(InterfaceSettingsModel obj)
    {
        try
        {
            string categoryEscaped = obj.Category?.Replace("'", "''") ?? "";
            string settingKeyEscaped = obj.SettingKey?.Replace("'", "''") ?? "";
            string settingValueEscaped = obj.SettingValue?.Replace("'", "''") ?? "";
            string descriptionEscaped = obj.Description?.Replace("'", "''") ?? "";
            string updatedFromEscaped = obj.UpdatedFrom?.Replace("'", "''") ?? "";
            string dataType = string.IsNullOrWhiteSpace(obj.DataType) ? "STRING" : obj.DataType;
            
            string SQLDuplicate = $@"SELECT * FROM InterfaceSettings 
                                    WHERE OrganizationId = {obj.OrganizationId} 
                                    AND Category = '{categoryEscaped}' 
                                    AND SettingKey = '{settingKeyEscaped}' 
                                    AND Id != {obj.Id} 
                                    AND IsSoftDeleted = 0;";
            
            string SQLUpdate = $@"UPDATE InterfaceSettings SET 
					OrganizationId = {obj.OrganizationId}, 
					Category = '{categoryEscaped}', 
					SettingKey = '{settingKeyEscaped}', 
					SettingValue = '{settingValueEscaped}', 
					DataType = '{dataType}',
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
                List<InterfaceSettingsModel> Output = new List<InterfaceSettingsModel>();
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
        return await dapper.Delete("InterfaceSettings", id);
    }

    public async Task<(bool, string)> SoftDelete(InterfaceSettingsModel obj)
    {
        string SQLUpdate = $@"UPDATE InterfaceSettings SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = {obj.UpdatedBy},
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}


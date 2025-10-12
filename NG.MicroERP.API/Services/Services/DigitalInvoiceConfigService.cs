using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;

public interface IDigitalInvoiceConfigService
{
    Task<(bool, List<DigitalInvoiceConfigModel>)>? Search(string Criteria = "");
    Task<(bool, DigitalInvoiceConfigModel?)>? Get(int id);
    Task<(bool, DigitalInvoiceConfigModel, string)> Post(DigitalInvoiceConfigModel obj);
    Task<(bool, string)> Put(DigitalInvoiceConfigModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(DigitalInvoiceConfigModel obj);
}


public class DigitalInvoiceConfigService : IDigitalInvoiceConfigService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<DigitalInvoiceConfigModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM DigitalInvoiceConfig Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<DigitalInvoiceConfigModel> result = (await dapper.SearchByQuery<DigitalInvoiceConfigModel>(SQL)) ?? new List<DigitalInvoiceConfigModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, DigitalInvoiceConfigModel?)>? Get(int id)
    {
        DigitalInvoiceConfigModel result = (await dapper.SearchByID<DigitalInvoiceConfigModel>("DigitalInvoiceConfig", id)) ?? new DigitalInvoiceConfigModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, DigitalInvoiceConfigModel, string)> Post(DigitalInvoiceConfigModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO DigitalInvoiceConfig 
			(
				OrganizationId, 
				Country, 
				PosId, 
				ClientId, 
				ClientSecret, 
				Username, 
				Password, 
				Target, 
				TargetApi, 
				IsDefault, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted 
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.Country!.ToUpper()}', 
				'{obj.PosId!.ToUpper()}', 
				'{obj.ClientId!.ToUpper()}', 
				'{obj.ClientSecret!.ToUpper()}', 
				'{obj.Username!.ToUpper()}', 
				'{obj.Password!.ToUpper()}', 
				'{obj.Target!.ToUpper()}', 
				'{obj.TargetApi!.ToUpper()}', 
				{obj.IsDefault},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<DigitalInvoiceConfigModel> Output = new List<DigitalInvoiceConfigModel>();
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

    public async Task<(bool, string)> Put(DigitalInvoiceConfigModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE DigitalInvoiceConfig SET 
					OrganizationId = {obj.OrganizationId}, 
					Country = '{obj.Country!.ToUpper()}', 
					PosId = '{obj.PosId!.ToUpper()}', 
					ClientId = '{obj.ClientId!.ToUpper()}', 
					ClientSecret = '{obj.ClientSecret!.ToUpper()}', 
					Username = '{obj.Username!.ToUpper()}', 
					Password = '{obj.Password!.ToUpper()}', 
					Target = '{obj.Target!.ToUpper()}', 
					TargetApi = '{obj.TargetApi!.ToUpper()}', 
					IsDefault = {obj.IsDefault}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
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
        return await dapper.Delete("DigitalInvoiceConfig", id);
    }

    public async Task<(bool, string)> SoftDelete(DigitalInvoiceConfigModel obj)
    {
        string SQLUpdate = $@"UPDATE DigitalInvoiceConfig SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
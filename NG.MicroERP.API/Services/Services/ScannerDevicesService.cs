using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;

public interface IScannerDevicesService
{
    Task<(bool, List<ScannerDevicesModel>)>? Search(string Criteria = "");
    Task<(bool, ScannerDevicesModel?)>? Get(int id);
    Task<(bool, ScannerDevicesModel, string)> Post(ScannerDevicesModel obj);
    Task<(bool, string)> Put(ScannerDevicesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ScannerDevicesModel obj);
}


public class ScannerDevicesService : IScannerDevicesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ScannerDevicesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                          a.*,
                          b.Name as LocationName
                        FROM scannerdevices as a
                        LEFT JOIN locations as b on b.id=a.LocationId
                        Where a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<ScannerDevicesModel> result = (await dapper.SearchByQuery<ScannerDevicesModel>(SQL)) ?? new List<ScannerDevicesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, ScannerDevicesModel?)>? Get(int id)
    {
        ScannerDevicesModel result = (await dapper.SearchByID<ScannerDevicesModel>("scannerdevices", id)) ?? new ScannerDevicesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, ScannerDevicesModel, string)> Post(ScannerDevicesModel obj)
    {

        try
        {
            string SQLDuplicate = $@"SELECT * FROM scannerdevices WHERE UPPER(DeviceIpAddress) = '{obj.DeviceIpAddress!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO scannerdevices 
			(
				OrganizationId, 
				DeviceIpAddress, 
				UserName, 
				Password, 
				LocationId, 
				Make, 
				Model, 
				Serial, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted, 
				InOutAll
			) 
			VALUES 
			(
				{obj.Id},
				{obj.OrganizationId},
				'{obj.DeviceIpAddress!.ToUpper()}', 
				'{obj.UserName!.ToUpper()}', 
				'{obj.Password!.ToUpper()}', 
				{obj.LocationId},
				'{obj.Make!.ToUpper()}', 
				'{obj.Model!.ToUpper()}', 
				'{obj.Serial!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted},
				'{obj.InOutAll!.ToUpper()}',
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ScannerDevicesModel> Output = new List<ScannerDevicesModel>();
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

    public async Task<(bool, string)> Put(ScannerDevicesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM scannerdevices WHERE UPPER(DeviceIpAddress) = '{obj.DeviceIpAddress!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE scannerdevices SET 
					OrganizationId = {obj.OrganizationId}, 
					DeviceIpAddress = '{obj.DeviceIpAddress!.ToUpper()}', 
					UserName = '{obj.UserName!.ToUpper()}', 
					Password = '{obj.Password!.ToUpper()}', 
					LocationId = {obj.LocationId}, 
					Make = '{obj.Make!.ToUpper()}', 
					Model = '{obj.Model!.ToUpper()}', 
					Serial = '{obj.Serial!.ToUpper()}', 
					InOutAll = '{obj.InOutAll!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted}
				WHERE Id = {obj.Id};";

            return await dapper.Update(SQLUpdate, SQLDuplicate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("scannerdevices", id);
    }

    public async Task<(bool, string)> SoftDelete(ScannerDevicesModel obj)
    {
        string SQLUpdate = $@"UPDATE scannerdevices SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
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

public interface IEmployeesDevicesService
{
    Task<(bool, List<EmployeesDevicesModel>)>? Search(string Criteria = "");
    Task<(bool, EmployeesDevicesModel?)>? Get(int id);
    Task<(bool, EmployeesDevicesModel, string)> Post(EmployeesDevicesModel obj);
    Task<(bool, string)> Put(EmployeesDevicesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(EmployeesDevicesModel obj);
}
public class EmployeesDevicesService : IEmployeesDevicesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<EmployeesDevicesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM EmployeesDevices Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<EmployeesDevicesModel> result = (await dapper.SearchByQuery<EmployeesDevicesModel>(SQL)) ?? new List<EmployeesDevicesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, EmployeesDevicesModel?)>? Get(int id)
    {
        EmployeesDevicesModel result = (await dapper.SearchByID<EmployeesDevicesModel>("EmployeesDevices", id)) ?? new EmployeesDevicesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, EmployeesDevicesModel, string)> Post(EmployeesDevicesModel obj)
    {

        try
        {   string SQLInsert = $@"INSERT INTO EmployeesDevices 
			(
				OrganizationId, 
				EmpId, 
				Device, 
				DeviceOs, 
				DeviceType, 
				DeviceSr, 
				IssuedOn, 
				IssueBy, 
				DeviceLife, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				{obj.EmpId},
				'{obj.Device!.ToUpper()}', 
				'{obj.DeviceOs!.ToUpper()}', 
				'{obj.DeviceType!.ToUpper()}', 
				'{obj.DeviceSr!.ToUpper()}', 
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.IssueBy},
				{obj.DeviceLife},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<EmployeesDevicesModel> Output = new List<EmployeesDevicesModel>();
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

    public async Task<(bool, string)> Put(EmployeesDevicesModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE EmployeesDevices SET 
					OrganizationId = {obj.OrganizationId}, 
					EmpId = {obj.EmpId}, 
					Device = '{obj.Device!.ToUpper()}', 
					DeviceOs = '{obj.DeviceOs!.ToUpper()}', 
					DeviceType = '{obj.DeviceType!.ToUpper()}', 
					DeviceSr = '{obj.DeviceSr!.ToUpper()}', 
					IssuedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					IssueBy = {obj.IssueBy}, 
					DeviceLife = {obj.DeviceLife}, 
					IsActive = {obj.IsActive}, 
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
        return await dapper.Delete("EmployeesDevices", id);
    }

    public async Task<(bool, string)> SoftDelete(EmployeesDevicesModel obj)
    {
        string SQLUpdate = $@"UPDATE EmployeesDevices SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
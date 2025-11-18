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

public interface IEmployeesService
{
    Task<(bool, List<EmployeesModel>)>? Search(string Criteria = "");
    Task<(bool, EmployeesModel?)>? Get(int id);
    Task<(bool, EmployeesModel, string)> Post(EmployeesModel obj);
    Task<(bool, EmployeesModel, string)> Put(EmployeesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(EmployeesModel obj);
}


public class EmployeesService : IEmployeesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<EmployeesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT a.*, b.DepartmentName, c.DesignationName
                        FROM employees as a
                        LEFT JOIN departments as b on b.Id=a.DepartmentId
                        LEFT JOIN designations as c on c.Id=a.DesignationId
                        Where a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by a.Fullname";

        List<EmployeesModel> result = (await dapper.SearchByQuery<EmployeesModel>(SQL)) ?? new List<EmployeesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, EmployeesModel?)>? Get(int id)
    {
        EmployeesModel result = (await dapper.SearchByID<EmployeesModel>("employees", id)) ?? new EmployeesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, EmployeesModel, string)> Post(EmployeesModel obj)
    {

        try
        {
            string SQLDuplicate = $@"SELECT * FROM employees WHERE UPPER(EmpId) = '{obj.EmpId!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO employees 
			(
				OrganizationId, 
				EmpId, 
				Fullname, 
				Pic, 
				Phone, 
				Email, 
				Cnic, 
				Address, 
				EmpType, 
				DepartmentId, 
				DesignationId, 
				ShiftId, 
				LocationId, 
				ParentId, 
				ExcludeFromAttendance, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.EmpId!.ToUpper()}', 
				'{obj.Fullname!.ToUpper()}', 
				'{obj.Pic!.ToUpper()}', 
				'{obj.Phone!.ToUpper()}', 
				'{obj.Email!}', 
				'{obj.Cnic!.ToUpper()}', 
				'{obj.Address!.ToUpper()}', 
				'{obj.EmpType!.ToUpper()}', 
				{obj.DepartmentId},
				{obj.DesignationId},
				{obj.ShiftId},
				{obj.LocationId},
				{obj.ParentId},
				{obj.ExcludeFromAttendance},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<EmployeesModel> Output = new List<EmployeesModel>();
                var result = await Search($"a.id={res.Item2}")!;
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

    public async Task<(bool, EmployeesModel, string)> Put(EmployeesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM employees WHERE UPPER(EmpId) = '{obj.EmpId!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE employees SET 
					OrganizationId = {obj.OrganizationId}, 
					EmpId = '{obj.EmpId!.ToUpper()}', 
					Fullname = '{obj.Fullname!.ToUpper()}', 
					Pic = '{obj.Pic!.ToUpper()}', 
					Phone = '{obj.Phone!.ToUpper()}', 
					Email = '{obj.Email!}', 
					Cnic = '{obj.Cnic!.ToUpper()}', 
					Address = '{obj.Address!.ToUpper()}', 
					EmpType = '{obj.EmpType!.ToUpper()}', 
					DepartmentId = {obj.DepartmentId}, 
					DesignationId = {obj.DesignationId}, 
					ShiftId = {obj.ShiftId}, 
					LocationId = {obj.LocationId}, 
					ParentId = {obj.ParentId}, 
					ExcludeFromAttendance = {obj.ExcludeFromAttendance}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}' 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<EmployeesModel> Output = new List<EmployeesModel>();
                var result = await Search($"a.id={obj.Id}")!;
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
        return await dapper.Delete("employees", id);
    }

    public async Task<(bool, string)> SoftDelete(EmployeesModel obj)
    {
        string SQLUpdate = $@"UPDATE employees SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
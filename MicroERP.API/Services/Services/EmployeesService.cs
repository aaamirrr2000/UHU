using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroERP.Shared.Helper;
using MicroERP.Shared.Models;
using MicroERP.API.Services;
using MicroERP.API.Helper;
using Serilog;

namespace MicroERP.API.Services;

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
                        LEFT JOIN Departments as b on b.Id=a.DepartmentId
                        LEFT JOIN Designations as c on c.Id=a.DesignationId
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
            string Code = dapper.GetCode("", "Employees", "EmpId", 5)!;
            
            string SQLDuplicate = $@"SELECT * FROM employees WHERE (UPPER(EmpId) = '{obj.EmpId!.ToUpper()}' OR (UPPER(Cnic) = '{obj.Cnic?.ToUpper()}' AND Cnic IS NOT NULL AND Cnic != '')) AND IsSoftDeleted = 0;";
            
            string dateOfBirth = obj.DateOfBirth.HasValue ? $"'{obj.DateOfBirth.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            string hireDate = obj.HireDate.HasValue ? $"'{obj.HireDate.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            string terminationDate = obj.TerminationDate.HasValue ? $"'{obj.TerminationDate.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            
            string SQLInsert = $@"INSERT INTO employees 
			(
				OrganizationId, 
				EmpId, 
				Fullname, 
				Pic, 
				Phone,
				Mobile, 
				Email, 
				Cnic,
				Gender,
				MaritalStatus,
				DateOfBirth,
				Address,
				City,
				Country,
				PostalCode,
				EmpType, 
				DepartmentId, 
				DesignationId, 
				ShiftId, 
				LocationId,
				HireDate,
				TerminationDate,
				BasicSalary,
				BankAccountId,
				BankAccountNumber,
				EmergencyContactName,
				EmergencyContactPhone,
				EmergencyContactRelation,
				Notes,
				ParentId, 
				ExcludeFromAttendance, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{Code}', 
				'{obj.Fullname?.Replace("'", "''").ToUpper() ?? string.Empty}', 
				'{obj.Pic?.Replace("'", "''") ?? string.Empty}', 
				'{obj.Phone?.Replace("'", "''") ?? string.Empty}',
				'{obj.Mobile?.Replace("'", "''") ?? string.Empty}', 
				'{obj.Email?.Replace("'", "''") ?? string.Empty}', 
				'{obj.Cnic?.Replace("'", "''") ?? string.Empty}',
				'{obj.Gender?.ToUpper() ?? string.Empty}',
				'{obj.MaritalStatus?.ToUpper() ?? string.Empty}',
				{dateOfBirth},
				'{obj.Address?.Replace("'", "''") ?? string.Empty}', 
				'{obj.City?.Replace("'", "''") ?? string.Empty}',
				'{obj.Country?.Replace("'", "''") ?? string.Empty}',
				'{obj.PostalCode?.Replace("'", "''") ?? string.Empty}',
				'{obj.EmpType?.ToUpper() ?? string.Empty}', 
				{obj.DepartmentId},
				{obj.DesignationId},
				{obj.ShiftId},
				{obj.LocationId},
				{hireDate},
				{terminationDate},
				{obj.BasicSalary},
				{(obj.BankAccountId > 0 ? obj.BankAccountId.ToString() : "NULL")},
				'{obj.BankAccountNumber?.Replace("'", "''") ?? string.Empty}',
				'{obj.EmergencyContactName?.Replace("'", "''") ?? string.Empty}',
				'{obj.EmergencyContactPhone?.Replace("'", "''") ?? string.Empty}',
				'{obj.EmergencyContactRelation?.Replace("'", "''") ?? string.Empty}',
				'{obj.Notes?.Replace("'", "''") ?? string.Empty}',
				{obj.ParentId},
				{obj.ExcludeFromAttendance},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom?.Replace("'", "''") ?? string.Empty}'
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
                // Check which field is duplicate
                if (!string.IsNullOrEmpty(res.Item3) && !res.Item3.Contains("Duplicate"))
                {
                    return (false, null!, $"Failed to create employee. {res.Item3}");
                }
                
                // Check duplicate conditions to provide specific message
                if (!string.IsNullOrEmpty(obj.EmpId))
                {
                    var existingEmpId = await dapper.SearchByQuery<dynamic>($"SELECT * FROM employees WHERE UPPER(EmpId) = '{obj.EmpId.ToUpper()}' AND IsSoftDeleted = 0", "Default");
                    if (existingEmpId != null && existingEmpId.Any())
                    {
                        return (false, null!, $"Employee ID '{obj.EmpId}' already exists. Please use a different Employee ID.");
                    }
                }
                
                if (!string.IsNullOrEmpty(obj.Cnic))
                {
                    var existingCnic = await dapper.SearchByQuery<dynamic>($"SELECT * FROM employees WHERE UPPER(Cnic) = '{obj.Cnic.ToUpper()}' AND Cnic IS NOT NULL AND Cnic != '' AND IsSoftDeleted = 0", "Default");
                    if (existingCnic != null && existingCnic.Any())
                    {
                        return (false, null!, $"CNIC '{obj.Cnic}' is already registered with another employee. Please verify the CNIC number.");
                    }
                }
                
                return (false, null!, $"Employee could not be created. A duplicate record was found (Employee ID or CNIC already exists).");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "EmployeesService.Post Error for Employee: {EmpId}", obj.EmpId);
            return (false, null!, $"Failed to create employee: {ex.Message}");
        }
    }

    public async Task<(bool, EmployeesModel, string)> Put(EmployeesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM employees WHERE ((UPPER(EmpId) = '{obj.EmpId!.ToUpper()}' AND ID != {obj.Id}) OR (UPPER(Cnic) = '{obj.Cnic?.ToUpper()}' AND Cnic IS NOT NULL AND Cnic != '' AND ID != {obj.Id})) AND IsSoftDeleted = 0;";
            
            string dateOfBirth = obj.DateOfBirth.HasValue ? $"'{obj.DateOfBirth.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            string hireDate = obj.HireDate.HasValue ? $"'{obj.HireDate.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            string terminationDate = obj.TerminationDate.HasValue ? $"'{obj.TerminationDate.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            
            string SQLUpdate = $@"UPDATE employees SET 
					OrganizationId = {obj.OrganizationId}, 
					EmpId = '{obj.EmpId!.ToUpper()}', 
					Fullname = '{obj.Fullname?.Replace("'", "''").ToUpper() ?? string.Empty}', 
					Pic = '{obj.Pic?.Replace("'", "''") ?? string.Empty}', 
					Phone = '{obj.Phone?.Replace("'", "''") ?? string.Empty}',
					Mobile = '{obj.Mobile?.Replace("'", "''") ?? string.Empty}', 
					Email = '{obj.Email?.Replace("'", "''") ?? string.Empty}', 
					Cnic = '{obj.Cnic?.Replace("'", "''") ?? string.Empty}',
					Gender = '{obj.Gender?.ToUpper() ?? string.Empty}',
					MaritalStatus = '{obj.MaritalStatus?.ToUpper() ?? string.Empty}',
					DateOfBirth = {dateOfBirth},
					Address = '{obj.Address?.Replace("'", "''") ?? string.Empty}', 
					City = '{obj.City?.Replace("'", "''") ?? string.Empty}',
					Country = '{obj.Country?.Replace("'", "''") ?? string.Empty}',
					PostalCode = '{obj.PostalCode?.Replace("'", "''") ?? string.Empty}',
					EmpType = '{obj.EmpType?.ToUpper() ?? string.Empty}', 
					DepartmentId = {obj.DepartmentId}, 
					DesignationId = {obj.DesignationId}, 
					ShiftId = {obj.ShiftId}, 
					LocationId = {obj.LocationId},
					HireDate = {hireDate},
					TerminationDate = {terminationDate},
					BasicSalary = {obj.BasicSalary},
					BankAccountId = {(obj.BankAccountId > 0 ? obj.BankAccountId.ToString() : "NULL")},
					BankAccountNumber = '{obj.BankAccountNumber?.Replace("'", "''") ?? string.Empty}',
					EmergencyContactName = '{obj.EmergencyContactName?.Replace("'", "''") ?? string.Empty}',
					EmergencyContactPhone = '{obj.EmergencyContactPhone?.Replace("'", "''") ?? string.Empty}',
					EmergencyContactRelation = '{obj.EmergencyContactRelation?.Replace("'", "''") ?? string.Empty}',
					Notes = '{obj.Notes?.Replace("'", "''") ?? string.Empty}',
					ParentId = {obj.ParentId}, 
					ExcludeFromAttendance = {obj.ExcludeFromAttendance}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom?.Replace("'", "''") ?? string.Empty}' 
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
                // Check which field is duplicate or what went wrong
                if (!string.IsNullOrEmpty(res.Item2) && !res.Item2.Contains("Duplicate") && !res.Item2.Contains("Record Not Saved"))
                {
                    return (false, null!, $"Failed to update employee (ID: {obj.Id}). {res.Item2}");
                }
                
                if (!string.IsNullOrEmpty(res.Item2) && res.Item2.Contains("Record Not Saved"))
                {
                    return (false, null!, $"Employee record (ID: {obj.Id}) could not be updated. The record may not exist, no changes were made, or the update query failed.");
                }
                
                // Check duplicate conditions to provide specific message
                if (!string.IsNullOrEmpty(obj.EmpId))
                {
                    var existingEmpId = await dapper.SearchByQuery<dynamic>($"SELECT * FROM employees WHERE UPPER(EmpId) = '{obj.EmpId.ToUpper()}' AND ID != {obj.Id} AND IsSoftDeleted = 0", "Default");
                    if (existingEmpId != null && existingEmpId.Any())
                    {
                        return (false, null!, $"Employee ID '{obj.EmpId}' is already assigned to another employee (ID: {existingEmpId.FirstOrDefault()?.Id}). Please use a different Employee ID.");
                    }
                }
                
                if (!string.IsNullOrEmpty(obj.Cnic))
                {
                    var existingCnic = await dapper.SearchByQuery<dynamic>($"SELECT * FROM employees WHERE UPPER(Cnic) = '{obj.Cnic.ToUpper()}' AND Cnic IS NOT NULL AND Cnic != '' AND ID != {obj.Id} AND IsSoftDeleted = 0", "Default");
                    if (existingCnic != null && existingCnic.Any())
                    {
                        return (false, null!, $"CNIC '{obj.Cnic}' is already registered with another employee (ID: {existingCnic.FirstOrDefault()?.Id}). Please verify the CNIC number.");
                    }
                }
                
                return (false, null!, $"Employee record (ID: {obj.Id}) could not be updated. A duplicate record was found (Employee ID or CNIC already exists).");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "EmployeesService.Put Error for Employee ID: {EmpId}, Record ID: {Id}", obj.EmpId, obj.Id);
            return (false, null!, $"Failed to update employee (ID: {obj.Id}): {ex.Message}");
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

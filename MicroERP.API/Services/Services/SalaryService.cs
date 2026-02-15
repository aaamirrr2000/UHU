using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroERP.API.Helper;
using MicroERP.Shared.Models;
using Serilog;

namespace MicroERP.API.Services;

public interface ISalaryService
{
    Task<(bool, List<SalaryModel>)>? Search(string Criteria = "");
    Task<(bool, SalaryModel?)>? Get(int id);
    Task<(bool, SalaryModel, string)> Post(SalaryModel obj);
    Task<(bool, SalaryModel, string)> Put(SalaryModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(SalaryModel obj);
}

public class SalaryService : ISalaryService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<SalaryModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT s.*, e.EmpId as EmployeeCode, e.Fullname as EmployeeName,
                        d.Name as DepartmentName, des.Name as DesignationName
                        FROM Salary as s
                        LEFT JOIN Employees as e on e.Id=s.EmployeeId
                        LEFT JOIN Departments as d on d.Id=s.DepartmentId
                        LEFT JOIN Designations as des on des.Id=s.DesignationId
                        Where s.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by s.SalaryMonth Desc, s.PayDate Desc, s.Id Desc";

        List<SalaryModel> result = (await dapper.SearchByQuery<SalaryModel>(SQL)) ?? new List<SalaryModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, SalaryModel?)>? Get(int id)
    {
        SalaryModel result = (await dapper.SearchByID<SalaryModel>("Salary", id)) ?? new SalaryModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, SalaryModel, string)> Post(SalaryModel obj)
    {
        try
        {
            // Calculate totals
            obj.GrossSalary = obj.BasicSalary + obj.Allowances + obj.Bonuses + obj.Overtime;
            obj.TotalDeductions = obj.Tax + obj.ProvidentFund + obj.Insurance + obj.Loans + obj.Deductions;
            obj.NetSalary = obj.GrossSalary - obj.TotalDeductions;
            
            string Code = dapper.GetCode("SAL", "Salary", "Code") ?? string.Empty;
            obj.Code = Code;
            
            string SQLDuplicate = $@"SELECT * FROM Salary WHERE (UPPER(Code) = '{Code.ToUpper()}' OR (EmployeeId = {obj.EmployeeId} AND SalaryMonth = '{obj.SalaryMonth}')) AND IsSoftDeleted = 0;";
            
            string payDate = obj.PayDate.HasValue ? $"'{obj.PayDate.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            string chequeDate = obj.ChequeDate.HasValue ? $"'{obj.ChequeDate.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            
            string SQLInsert = $@"INSERT INTO Salary 
                (
                    OrganizationId, Code, SalaryMonth, EmployeeId, DepartmentId, DesignationId,
                    PayDate, BasicSalary, Allowances, Bonuses, Overtime, GrossSalary,
                    Tax, ProvidentFund, Insurance, Loans, Deductions, TotalDeductions, NetSalary,
                    PaymentMethod, BankAccountId, BankAccountNumber, ChequeNumber, ChequeDate,
                    Status, Notes, CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                ) 
                VALUES 
                (
                    {obj.OrganizationId}, '{Code}', '{obj.SalaryMonth?.Replace("'", "''") ?? string.Empty}', 
                    {obj.EmployeeId}, {obj.DepartmentId}, {obj.DesignationId},
                    {payDate}, 
                    {obj.BasicSalary}, {obj.Allowances}, {obj.Bonuses}, {obj.Overtime}, {obj.GrossSalary},
                    {obj.Tax}, {obj.ProvidentFund}, {obj.Insurance}, {obj.Loans}, {obj.Deductions}, 
                    {obj.TotalDeductions}, {obj.NetSalary},
                    '{obj.PaymentMethod?.ToUpper() ?? string.Empty}', 
                    {obj.BankAccountId}, 
                    '{obj.BankAccountNumber?.Replace("'", "''") ?? string.Empty}', 
                    '{obj.ChequeNumber?.Replace("'", "''") ?? string.Empty}', 
                    {chequeDate},
                    '{obj.Status?.ToUpper() ?? "PENDING"}', 
                    '{obj.Notes?.Replace("'", "''") ?? string.Empty}', 
                    {obj.CreatedBy},
                    '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    '{obj.CreatedFrom?.Replace("'", "''") ?? string.Empty}', 
                    {obj.IsSoftDeleted}
                );";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<SalaryModel> Output = new List<SalaryModel>();
                var result = await Search($"s.id={res.Item2}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                // Check if it's a duplicate or other error
                if (!string.IsNullOrEmpty(res.Item3) && !res.Item3.Contains("Duplicate"))
                {
                    return (false, null!, $"Failed to create salary record for Employee ID {obj.EmployeeId}, Month {obj.SalaryMonth}. {res.Item3}");
                }
                
                // Check for duplicate Salary Code
                var existingCode = await dapper.SearchByQuery<dynamic>($"SELECT * FROM Salary WHERE UPPER(Code) = '{Code.ToUpper()}' AND IsSoftDeleted = 0", "Default");
                if (existingCode != null && existingCode.Any())
                {
                    return (false, null!, $"Salary Code '{Code}' already exists. Please use a different Salary Code.");
                }
                
                // Check for duplicate Employee + Month combination
                var existingSalary = await dapper.SearchByQuery<dynamic>($"SELECT * FROM Salary WHERE EmployeeId = {obj.EmployeeId} AND SalaryMonth = '{obj.SalaryMonth}' AND IsSoftDeleted = 0", "Default");
                if (existingSalary != null && existingSalary.Any())
                {
                    return (false, null!, $"A salary record already exists for Employee ID {obj.EmployeeId} for the month '{obj.SalaryMonth}'. Please use a different month or update the existing record.");
                }
                
                return (false, null!, $"Salary record could not be created for Employee ID {obj.EmployeeId}, Month '{obj.SalaryMonth}'. A duplicate record was found (Salary Code or Employee+Month combination already exists).");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SalaryService.Post Error for Employee ID: {EmployeeId}, Month: {SalaryMonth}", obj.EmployeeId, obj.SalaryMonth);
            return (false, null!, $"Failed to create salary record for Employee ID {obj.EmployeeId}, Month '{obj.SalaryMonth}': {ex.Message}");
        }
    }

    public async Task<(bool, SalaryModel, string)> Put(SalaryModel obj)
    {
        try
        {
            // Calculate totals
            obj.GrossSalary = obj.BasicSalary + obj.Allowances + obj.Bonuses + obj.Overtime;
            obj.TotalDeductions = obj.Tax + obj.ProvidentFund + obj.Insurance + obj.Loans + obj.Deductions;
            obj.NetSalary = obj.GrossSalary - obj.TotalDeductions;
            
            string SQLDuplicate = $@"SELECT * FROM Salary WHERE ((UPPER(Code) = '{obj.Code?.ToUpper()}' AND ID != {obj.Id}) OR (EmployeeId = {obj.EmployeeId} AND SalaryMonth = '{obj.SalaryMonth}' AND ID != {obj.Id})) AND IsSoftDeleted = 0;";
            
            string payDate = obj.PayDate.HasValue ? $"'{obj.PayDate.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            string chequeDate = obj.ChequeDate.HasValue ? $"'{obj.ChequeDate.Value.ToString("yyyy-MM-dd")}'" : "NULL";
            
            string SQLUpdate = $@"UPDATE Salary SET 
                    OrganizationId = {obj.OrganizationId}, 
                    Code = '{obj.Code?.Replace("'", "''") ?? string.Empty}', 
                    SalaryMonth = '{obj.SalaryMonth?.Replace("'", "''") ?? string.Empty}', 
                    EmployeeId = {obj.EmployeeId}, 
                    DepartmentId = {obj.DepartmentId}, 
                    DesignationId = {obj.DesignationId},
                    PayDate = {payDate}, 
                    BasicSalary = {obj.BasicSalary}, 
                    Allowances = {obj.Allowances}, 
                    Bonuses = {obj.Bonuses}, 
                    Overtime = {obj.Overtime}, 
                    GrossSalary = {obj.GrossSalary},
                    Tax = {obj.Tax}, 
                    ProvidentFund = {obj.ProvidentFund}, 
                    Insurance = {obj.Insurance}, 
                    Loans = {obj.Loans}, 
                    Deductions = {obj.Deductions}, 
                    TotalDeductions = {obj.TotalDeductions}, 
                    NetSalary = {obj.NetSalary},
                    PaymentMethod = '{obj.PaymentMethod?.ToUpper() ?? string.Empty}', 
                    BankAccountId = {obj.BankAccountId}, 
                    BankAccountNumber = '{obj.BankAccountNumber?.Replace("'", "''") ?? string.Empty}', 
                    ChequeNumber = '{obj.ChequeNumber?.Replace("'", "''") ?? string.Empty}', 
                    ChequeDate = {chequeDate},
                    Status = '{obj.Status?.ToUpper() ?? "PENDING"}', 
                    Notes = '{obj.Notes?.Replace("'", "''") ?? string.Empty}', 
                    UpdatedBy = {obj.UpdatedBy}, 
                    UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
                    UpdatedFrom = '{obj.UpdatedFrom?.Replace("'", "''") ?? string.Empty}', 
                    IsSoftDeleted = {obj.IsSoftDeleted} 
                WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<SalaryModel> Output = new List<SalaryModel>();
                var result = await Search($"s.id={obj.Id}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                // Check if it's a duplicate or other error
                if (!string.IsNullOrEmpty(res.Item2) && !res.Item2.Contains("Duplicate") && !res.Item2.Contains("Record Not Saved"))
                {
                    return (false, null!, $"Failed to update salary record (ID: {obj.Id}) for Employee ID {obj.EmployeeId}, Month '{obj.SalaryMonth}'. {res.Item2}");
                }
                
                if (!string.IsNullOrEmpty(res.Item2) && res.Item2.Contains("Record Not Saved"))
                {
                    return (false, null!, $"Salary record (ID: {obj.Id}) for Employee ID {obj.EmployeeId}, Month '{obj.SalaryMonth}' could not be updated. The record may not exist, no changes were made, or the update query failed.");
                }
                
                // Check for duplicate Salary Code
                var existingCode = await dapper.SearchByQuery<dynamic>($"SELECT * FROM Salary WHERE UPPER(Code) = '{obj.Code!.ToUpper()}' AND ID != {obj.Id} AND IsSoftDeleted = 0", "Default");
                if (existingCode != null && existingCode.Any())
                {
                    return (false, null!, $"Salary Code '{obj.Code}' is already assigned to another salary record (ID: {existingCode.FirstOrDefault()?.Id}). Please use a different Salary Code.");
                }
                
                // Check for duplicate Employee + Month combination
                var existingSalary = await dapper.SearchByQuery<dynamic>($"SELECT * FROM Salary WHERE EmployeeId = {obj.EmployeeId} AND SalaryMonth = '{obj.SalaryMonth}' AND ID != {obj.Id} AND IsSoftDeleted = 0", "Default");
                if (existingSalary != null && existingSalary.Any())
                {
                    return (false, null!, $"A salary record (ID: {existingSalary.FirstOrDefault()?.Id}) already exists for Employee ID {obj.EmployeeId} for the month '{obj.SalaryMonth}'. Please use a different month or update the existing record.");
                }
                
                return (false, null!, $"Salary record (ID: {obj.Id}) for Employee ID {obj.EmployeeId}, Month '{obj.SalaryMonth}' could not be updated. A duplicate record was found (Salary Code or Employee+Month combination already exists).");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, $"Failed to update salary record (ID: {obj.Id}) for Employee ID {obj.EmployeeId}, Month '{obj.SalaryMonth}': {ex.Message}");
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Salary", id);
    }

    public async Task<(bool, string)> SoftDelete(SalaryModel obj)
    {
        string SQLUpdate = $@"UPDATE Salary SET 
                    UpdatedOn = '{DateTime.UtcNow}', 
                    UpdatedBy = {obj.UpdatedBy},
                    IsSoftDeleted = 1 
                WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}


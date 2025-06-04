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
    Task<(bool, string)> Put(EmployeesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(EmployeesModel obj);
}


public class EmployeesService : IEmployeesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<EmployeesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Employees Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<EmployeesModel> result = (await dapper.SearchByQuery<EmployeesModel>(SQL)) ?? new List<EmployeesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, EmployeesModel?)>? Get(int id)
    {
        EmployeesModel result = (await dapper.SearchByID<EmployeesModel>("Employees", id)) ?? new EmployeesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, EmployeesModel, string)> Post(EmployeesModel obj)
    {

        try
        {
            string Code = dapper.GetCode("EMP", "Employees", "EmpId")!;
            string SQLDuplicate = $@"SELECT * FROM Employees WHERE UPPER(EmpId) = '{obj.EmpId!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Employees 
			(
				OrganizationId, 
				EmpId, 
				Fullname, 
				Pic, 
				Phone, 
				Email, 
				Cnic, 
				EmpAddress, 
				EmpType, 
				EmpDept, 
				LocationId, 
				ParentId, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{Code}', 
				'{obj.Fullname!.ToUpper()}', 
				'{obj.Pic!.ToUpper()}', 
				'{obj.Phone!.ToUpper()}', 
				'{obj.Email!}', 
				'{obj.Cnic!.ToUpper()}', 
				'{obj.EmpAddress!.ToUpper()}', 
				'{obj.EmpType!.ToUpper()}', 
				'{obj.EmpDept!.ToUpper()}', 
				{obj.LocationId},
				{obj.ParentId},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<EmployeesModel> Output = new List<EmployeesModel>();
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

    public async Task<(bool, string)> Put(EmployeesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Employees WHERE UPPER(code) = '{obj.EmpId!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE Employees SET 
					OrganizationId = {obj.OrganizationId}, 
					EmpId = '{obj.EmpId!.ToUpper()}', 
					Fullname = '{obj.Fullname!.ToUpper()}', 
					Pic = '{obj.Pic!.ToUpper()}', 
					Phone = '{obj.Phone!.ToUpper()}', 
					Email = '{obj.Email!}', 
					Cnic = '{obj.Cnic!.ToUpper()}', 
					EmpAddress = '{obj.EmpAddress!.ToUpper()}', 
					EmpType = '{obj.EmpType!.ToUpper()}', 
					EmpDept = '{obj.EmpDept!.ToUpper()}', 
					LocationId = {obj.LocationId}, 
					ParentId = {obj.ParentId}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
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
        return await dapper.Delete("Employees", id);
    }

    public async Task<(bool, string)> SoftDelete(EmployeesModel obj)
    {
        string SQLUpdate = $@"UPDATE Employees SET 
					UpdatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}
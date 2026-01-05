using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Services;


public interface IUsersService
{
    Task<(bool, List<UsersModel>)>? Search(string Criteria = "");
    Task<(bool, UsersModel?)>? Get(int id);
    Task<(bool, UsersModel, string)> Post(UsersModel obj);
    Task<(bool, UsersModel, string)> Put(UsersModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(UsersModel obj);
    Task<(bool, string)> ResetPassword(int User_id);
    Task<(bool, UsersModel)> Login(string UserName, string Password);
    Task<(bool, string)> SetTheme(int user_id, int theme);
}

public class UsersService : IUsersService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<UsersModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"

                    SELECT
                       Users.*,
                       Groups.Name as GroupName,
                       Locations.Name as LocationName,
                       Employees.Fullname as Fullname,
                       Employees.Email as Email,
                       Groups.OrganizationId as OrganizationId,
                       Groups.Dashboard as Dashboard
                     FROM Users
                     LEFT JOIN Groups on Groups.Id = Users.GroupId
                     LEFT JOIN Locations on Locations.Id = Users.LocationId
                     LEFT JOIN Employees on Employees.Id = Users.EmpId
                     Where Users.IsSoftDeleted=0

                    ";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Users.Id Desc";

        List<UsersModel> result = (await dapper.SearchByQuery<UsersModel>(SQL)) ?? new List<UsersModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);

    }

    public async Task<(bool, UsersModel?)>? Get(int id)
    {
        UsersModel result = (await dapper.SearchByID<UsersModel>("Users", id)) ?? new UsersModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, UsersModel, string)> Post(UsersModel obj)
    {

        try
        {
            string SQLDuplicate = $@"SELECT * FROM Users WHERE UPPER(Username) = '{obj.Username!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Users 
			(
				Username, 
				Password, 
				UserType, 
				DarKLightTheme, 
				EmpId, 
				GroupId, 
				LocationId, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				UpdatedBy, 
				UpdatedOn, 
				UpdatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				'{obj.Username!.ToUpper()}', 
				'{Config.Encrypt(Config.GenerateRandomPassword())}', 
				'{obj.UserType!.ToUpper()}', 
				0,
				{obj.EmpId},
				{obj.GroupId},
				{obj.LocationId},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.UpdatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.UpdatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<UsersModel> Output = new List<UsersModel>();
                var result = await Search($"Users.id={res.Item2}")!;
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

    public async Task<(bool, UsersModel, string)> Put(UsersModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Users WHERE UPPER(Username) = '{obj.Username!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE Users SET 
					Username = '{obj.Username!.ToUpper()}', 
					UserType = '{obj.UserType!.ToUpper()}', 
					DarKLightTheme = 0, 
					EmpId = {obj.EmpId}, 
					GroupId = {obj.GroupId}, 
					LocationId = {obj.LocationId}, 
					IsActive = {obj.IsActive}, 
					CreatedBy = {obj.CreatedBy}, 
					CreatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					CreatedFrom = '{obj.CreatedFrom!.ToUpper()}', 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<UsersModel> Output = new List<UsersModel>();
                var result = await Search($"Users.id={obj.Id}")!;
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
        return await dapper.Delete("Users", id);
    }

    public async Task<(bool, string)> SoftDelete(UsersModel obj)
    {
        string SQLUpdate = $@"UPDATE Users SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }

    public async Task<(bool, string)> SetTheme(int user_id, int theme)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Users SET DarKLightTheme = {theme} WHERE Id = {user_id};";
            return await dapper.Update(SQLUpdate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

   

    public async Task<(bool, UsersModel)> Login(string UserName, string Password)
    {
        string SQL = $@"Select * from Users where lower(username) = '{UserName.ToLower()}' and password = '{Config.Encrypt( Password)}';";
        List<UsersModel>? res = await dapper.SearchByQuery<UsersModel>(SQL);

        if (res != null)
        {
            if (res.Count > 0)
            {
                UsersModel result = res[0];

                var result1 = await Search($"Users.Id = {result.Id}")!;
                UsersModel result2 = result1.Item2.FirstOrDefault()!;
                return (true, result2);

            }
            return (false, new UsersModel { });
        }
        else
        {
            return (false, new UsersModel { });
        }
    }

    public async Task<(bool, EmployeesModel)> ForgotPassword(string Email)
    {
        string SQL = $@"Select * from Employees where lower(email) = '{Email.ToLower()}';";
        List<EmployeesModel>? res = await dapper.SearchByQuery<EmployeesModel>(SQL);
        
        if (res != null)
        {
            if (res.Count > 0)
            {
                EmployeesModel result = res[0];

                string NewPassword = Config.GenerateRandomPassword();
                string SQLUpdate = $@"UPDATE Users SET Password = '{Config.Encrypt( NewPassword)}' WHERE EmpId = {result.Id};";

                var result1 = await dapper.Update(SQLUpdate);
                if(result1.Item1 == true)
                {
                    bool EmailSent = Config.sendEmail(Email, "Password Change", $"New Password is {NewPassword}");
                    return (true, result);
                }
                return (false, new EmployeesModel { });
            }
            return (false, new EmployeesModel { });
        }
        else
        {
            return (false, new EmployeesModel { });
        }
    }

    public async Task<(bool, string)> ResetPassword(int UserId)
    {
        UsersService usersService = new();
        var usrs = await Search($"Users.Id = {UserId}")!;

        if (usrs.Item1 == true)
        {
            if (usrs.Item2.Count > 0)
            {
                _ = new UsersModel();
                UsersModel u = usrs.Item2.FirstOrDefault()!;
                string cpass = Config.GenerateRandomPassword();
                string SQL = $@"Update Users set password = '" + Config.Encrypt(cpass) + "' where Id = " + u.Id + ";";
                (bool, string) r = await dapper.ExecuteQuery(SQL);
                if (r.Item1 == false)
                {
                    return (false, "User Not Found");
                }
                else
                {
                    if (u.Email.Length > 0)
                    {
                        Config.sendEmail(u.Email, $"Password Reset", $"Your new Passord is {cpass}.");
                    }
                    return (true, "Password Saved and Emailed to User.");
                }
            }
            else
            {
                return (false, "User Not found.");
            }
        }
        else
        {
            return (false, "User Not found.");
        }
    }

    public async Task<(bool, string)> ChangePassword(int UserId, string NewPassword)
    {
        string SQL = $@"Select * from Users where Id = {UserId};";
        List<UsersModel>? Users = await dapper.SearchByQuery<UsersModel>(SQL);
        UsersModel User = Users.FirstOrDefault();

        if(User!=null)
        {

            SQL = $@"Select * from Employees where Id = {User!.EmpId};";
            List<EmployeesModel>? Employees = await dapper.SearchByQuery<EmployeesModel>(SQL);
            EmployeesModel Employee = Employees.FirstOrDefault();

            //Changing Password and Sending Email
            SQL = $@"Update Users set password = '" + Config.Encrypt(NewPassword) + "' where Id = " + UserId + ";";
            (bool, string) result = await dapper.ExecuteQuery(SQL);
            if (result.Item1 == false)
            {
                return (false, "User Not Updated");
            }
            else
            {
                Config.sendEmail(Employee.Email, $"Password Reset", $"Your new Passord is {NewPassword}.");
                return (true, "Password Saved and Emailed to User.");
            }
        }
        else
            return (false, "User Not Found");
    }
}

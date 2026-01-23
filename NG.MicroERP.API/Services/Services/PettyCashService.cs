using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Models;
using Serilog;

namespace NG.MicroERP.API.Services.Services;

public interface IPettyCashService
{
    Task<(bool, List<PettyCashModel>)>? Search(string Criteria = "");
    Task<(bool, PettyCashModel?)>? Get(int id);
    Task<(bool, PettyCashReportModel?)>? GetPettyCashReport(int id);
    Task<(bool, PettyCashModel, string)> Post(PettyCashModel obj);
    Task<(bool, PettyCashModel, string)> Put(PettyCashModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PettyCashModel obj);
}

public class PettyCashService : IPettyCashService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PettyCashModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT 'PETTY CASH' as Source, a.*, b.Name as LocationName, e.Fullname as EmployeeName, coa.Name as AccountName
                        FROM PettyCash as a
                        LEFT JOIN Locations as b on b.id=a.LocationId
                        LEFT JOIN Employees as e on e.Id=a.EmployeeId
                        LEFT JOIN ChartOfAccounts as coa on coa.Id=a.AccountId
                        Where a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PettyCashModel> result = (await dapper.SearchByQuery<PettyCashModel>(SQL)) ?? new List<PettyCashModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PettyCashModel?)>? Get(int id)
    {
        try
        {
            if (id == 0)
            {
                return (false, null);
            }

            string SQL = $@"SELECT a.*, b.Name as LocationName, e.Fullname as EmployeeName, coa.Name as AccountName
                            FROM PettyCash as a
                            LEFT JOIN Locations as b on b.id=a.LocationId
                            LEFT JOIN Employees as e on e.Id=a.EmployeeId
                            LEFT JOIN ChartOfAccounts as coa on coa.Id=a.AccountId
                            Where a.Id={id} AND a.IsSoftDeleted=0";
            
            PettyCashModel result = (await dapper.SearchByQuery<PettyCashModel>(SQL))?.FirstOrDefault() ?? new PettyCashModel();
            if (result == null || result.Id == 0)
                return (false, null);
            else
                return (true, result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PettyCashService.Get Error: {Message}", ex.Message);
            return (false, null);
        }
    }

    public async Task<(bool, PettyCashReportModel?)>? GetPettyCashReport(int id)
    {
        PettyCashReportModel result = (await dapper.SearchByID<PettyCashReportModel>("vw_PettyCashReport", id)) ?? new PettyCashReportModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, PettyCashModel, string)> Post(PettyCashModel obj)
    {
        try
        {
            // Validate required fields
            if (obj.OrganizationId == 0)
            {
                return (false, null!, "Organization is required. Please select an organization.");
            }

            if (obj.EmployeeId == 0)
            {
                return (false, null!, "Employee is required. Please select an employee.");
            }

            if (obj.AccountId == 0)
            {
                return (false, null!, "Account is required. Please select an account from Chart of Accounts.");
            }

            if (obj.Amount == 0)
            {
                return (false, null!, "Amount is required. Please enter a valid amount.");
            }

            if (string.IsNullOrWhiteSpace(obj.TranType))
            {
                return (false, null!, "Transaction Type is required. Please select either 'Receipt' or 'Payment'.");
            }

            if (string.IsNullOrWhiteSpace(obj.PaymentMethod))
            {
                return (false, null!, "Payment Method is required. Please select a payment method.");
            }

            if (!obj.TranDate.HasValue)
            {
                return (false, null!, "Transaction Date is required. Please select a valid date.");
            }

            // Validate period for pettycash creation
            var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.OrganizationId, obj.TranDate.Value, "PETTYCASH");
            if (!periodCheck.Item1)
            {
                return (false, null!, periodCheck.Item2);
            }

            // Validate employee exists
            var employeeCheck = await dapper.SearchByQuery<EmployeesModel>($"SELECT * FROM Employees WHERE Id = {obj.EmployeeId} AND IsSoftDeleted = 0");
            if (employeeCheck == null || !employeeCheck.Any())
            {
                return (false, null!, $"Employee with ID {obj.EmployeeId} not found or has been deleted. Please select a valid employee.");
            }

            // Validate account exists
            var accountCheck = await dapper.SearchByQuery<ChartOfAccountsModel>($"SELECT * FROM ChartOfAccounts WHERE Id = {obj.AccountId} AND IsActive = 1 AND IsSoftDeleted = 0");
            if (accountCheck == null || !accountCheck.Any())
            {
                return (false, null!, $"Account with ID {obj.AccountId} not found, inactive, or has been deleted. Please select a valid account from Chart of Accounts.");
            }

            string Code = dapper.GetCode("PTC", "PettyCash", "SeqNo")!;
            if (string.IsNullOrWhiteSpace(Code))
            {
                return (false, null!, "Failed to generate sequence number. Please contact system administrator.");
            }

            // Get currency fields with defaults
            int baseCurrencyId = obj.BaseCurrencyId > 0 ? obj.BaseCurrencyId : 0;
            int enteredCurrencyId = obj.EnteredCurrencyId > 0 ? obj.EnteredCurrencyId : 0;
            double exchangeRate = obj.ExchangeRate > 0 ? obj.ExchangeRate : 1.0;

            string SQLDuplicate = $@"SELECT * FROM PettyCash WHERE UPPER(SeqNo) = '{Code.ToUpper()}' AND IsSoftDeleted = 0;";
            string SQLInsert = $@"INSERT INTO PettyCash 
			(
				OrganizationId, 
				SeqNo,
                FileAttachment,
				LocationId, 
				EmployeeId, 
				TranDate, 
				Description, 
				Amount, 
				AccountId, 
				TranType, 
				PaymentMethod, 
				RefNo, 
				TranRef, 
				BaseCurrencyId,
				EnteredCurrencyId,
				ExchangeRate,
				CreatedBy, 
				CreatedOn, 
				CreatedFrom
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{Code!}', 
                '{obj.FileAttachment?.Replace("'", "''") ?? ""}',
				{obj.LocationId},
				{obj.EmployeeId},
				'{obj.TranDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")}',
				'{obj.Description?.Replace("'", "''").ToUpper() ?? ""}', 
				{obj.Amount},
				{obj.AccountId},
				'{obj.TranType?.ToUpper() ?? ""}', 
				'{obj.PaymentMethod?.Replace("'", "''").ToUpper() ?? ""}', 
				'{obj.RefNo?.Replace("'", "''").ToUpper() ?? ""}', 
				'{obj.TranRef?.Replace("'", "''").ToUpper() ?? ""}', 
				{(baseCurrencyId > 0 ? baseCurrencyId.ToString() : "NULL")},
				{(enteredCurrencyId > 0 ? enteredCurrencyId.ToString() : "NULL")},
				{exchangeRate.ToString(System.Globalization.CultureInfo.InvariantCulture)},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom?.Replace("'", "''").ToUpper() ?? ""}'
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<PettyCashModel> Output = new List<PettyCashModel>();
                var result = await Search($"a.id={res.Item2}")!;
                if (result.Item1 && result.Item2 != null && result.Item2.Any())
                {
                    Output = result.Item2;
                    return (true, Output.FirstOrDefault()!, "");
                }
                else
                {
                    return (false, null!, $"PettyCash entry was created successfully but could not be retrieved. ID: {res.Item2}");
                }
            }
            else
            {
                // Check if it's a duplicate or another error
                if (!string.IsNullOrWhiteSpace(res.Item3) && res.Item3.Contains("Duplicate"))
                {
                    return (false, null!, $"Duplicate PettyCash entry found. Sequence Number '{Code}' already exists.");
                }
                else if (!string.IsNullOrWhiteSpace(res.Item3))
                {
                    return (false, null!, $"Failed to save PettyCash entry: {res.Item3}");
                }
                else
                {
                    return (false, null!, "Failed to save PettyCash entry. Please check all required fields and try again.");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PettyCashService.Post Error: {Message}", ex.Message);
            return (false, null!, $"An error occurred while saving PettyCash entry: {ex.Message}");
        }
    }

    public async Task<(bool, PettyCashModel, string)> Put(PettyCashModel obj)
    {
        try
        {
            // Validate required fields
            if (obj.Id == 0)
            {
                return (false, null!, "PettyCash entry ID is required. Cannot update without a valid ID.");
            }

            // Check if record exists
            var existingPettyCash = await dapper.SearchByQuery<PettyCashModel>($"SELECT * FROM PettyCash WHERE Id = {obj.Id} AND IsSoftDeleted = 0");
            if (existingPettyCash == null || !existingPettyCash.Any())
            {
                return (false, null!, $"PettyCash entry with ID {obj.Id} not found or has been deleted. Cannot update non-existent record.");
            }

            var existing = existingPettyCash.First();

            // Check if pettycash is posted to GL - prevent updates
            if (existing.IsPostedToGL == 1)
            {
                return (false, null!, $"Cannot update PettyCash entry '{existing.SeqNo}' because it has already been posted to General Ledger (GL Entry: {existing.GLEntryNo ?? "N/A"}). Please reverse the GL entry first if you need to make changes.");
            }

            // Validate required fields
            if (obj.OrganizationId == 0)
            {
                return (false, null!, "Organization is required. Please select an organization.");
            }

            if (obj.EmployeeId == 0)
            {
                return (false, null!, "Employee is required. Please select an employee.");
            }

            if (obj.AccountId == 0)
            {
                return (false, null!, "Account is required. Please select an account from Chart of Accounts.");
            }

            if (obj.Amount == 0)
            {
                return (false, null!, "Amount is required. Please enter a valid amount.");
            }

            if (string.IsNullOrWhiteSpace(obj.TranType))
            {
                return (false, null!, "Transaction Type is required. Please select either 'Receipt' or 'Payment'.");
            }

            if (string.IsNullOrWhiteSpace(obj.PaymentMethod))
            {
                return (false, null!, "Payment Method is required. Please select a payment method.");
            }

            if (string.IsNullOrWhiteSpace(obj.SeqNo))
            {
                return (false, null!, "Sequence Number is required. Cannot update without a valid sequence number.");
            }

            if (!obj.TranDate.HasValue)
            {
                return (false, null!, "Transaction Date is required. Please select a valid date.");
            }

            // Validate period for pettycash update
            var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.OrganizationId, obj.TranDate.Value, "PETTYCASH");
            if (!periodCheck.Item1)
            {
                return (false, null!, periodCheck.Item2);
            }

            // Validate employee exists
            var employeeCheck = await dapper.SearchByQuery<EmployeesModel>($"SELECT * FROM Employees WHERE Id = {obj.EmployeeId} AND IsSoftDeleted = 0");
            if (employeeCheck == null || !employeeCheck.Any())
            {
                return (false, null!, $"Employee with ID {obj.EmployeeId} not found or has been deleted. Please select a valid employee.");
            }

            // Validate account exists
            var accountCheck = await dapper.SearchByQuery<ChartOfAccountsModel>($"SELECT * FROM ChartOfAccounts WHERE Id = {obj.AccountId} AND IsActive = 1 AND IsSoftDeleted = 0");
            if (accountCheck == null || !accountCheck.Any())
            {
                return (false, null!, $"Account with ID {obj.AccountId} not found, inactive, or has been deleted. Please select a valid account from Chart of Accounts.");
            }

            string SQLDuplicate = $@"SELECT * FROM PettyCash WHERE UPPER(SeqNo) = '{obj.SeqNo!.ToUpper()}' AND Id != {obj.Id} AND IsSoftDeleted = 0;";
            
            // Get currency fields with defaults
            int baseCurrencyId = obj.BaseCurrencyId > 0 ? obj.BaseCurrencyId : 0;
            int enteredCurrencyId = obj.EnteredCurrencyId > 0 ? obj.EnteredCurrencyId : 0;
            double exchangeRate = obj.ExchangeRate > 0 ? obj.ExchangeRate : 1.0;
            
            string SQLUpdate = $@"UPDATE PettyCash SET 
					OrganizationId = {obj.OrganizationId}, 
					SeqNo = '{obj.SeqNo?.Replace("'", "''").ToUpper() ?? ""}', 
                    FileAttachment = '{obj.FileAttachment?.Replace("'", "''") ?? ""}', 
					LocationId = {obj.LocationId}, 
					EmployeeId = {obj.EmployeeId}, 
					TranDate = '{obj.TranDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")}', 
					Description = '{obj.Description?.Replace("'", "''").ToUpper() ?? ""}', 
					Amount = {obj.Amount}, 
					AccountId = {obj.AccountId}, 
					TranType = '{obj.TranType?.ToUpper() ?? ""}', 
					PaymentMethod = '{obj.PaymentMethod?.Replace("'", "''").ToUpper() ?? ""}', 
					RefNo = '{obj.RefNo?.Replace("'", "''").ToUpper() ?? ""}', 
					TranRef = '{obj.TranRef?.Replace("'", "''").ToUpper() ?? ""}',  
					BaseCurrencyId = {(baseCurrencyId > 0 ? baseCurrencyId.ToString() : "NULL")},
					EnteredCurrencyId = {(enteredCurrencyId > 0 ? enteredCurrencyId.ToString() : "NULL")},
					ExchangeRate = {exchangeRate.ToString(System.Globalization.CultureInfo.InvariantCulture)},
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom?.Replace("'", "''").ToUpper() ?? ""}'
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<PettyCashModel> Output = new List<PettyCashModel>();
                var result = await Search($"a.id={obj.Id}")!;
                if (result.Item1 && result.Item2 != null && result.Item2.Any())
                {
                    Output = result.Item2;
                    return (true, Output.FirstOrDefault()!, "");
                }
                else
                {
                    return (false, null!, $"PettyCash entry was updated successfully but could not be retrieved. ID: {obj.Id}");
                }
            }
            else
            {
                // Check if it's a duplicate or another error
                if (!string.IsNullOrWhiteSpace(res.Item2) && res.Item2.Contains("Duplicate"))
                {
                    return (false, null!, $"Duplicate PettyCash entry found. Sequence Number '{obj.SeqNo}' already exists for another record.");
                }
                else if (!string.IsNullOrWhiteSpace(res.Item2))
                {
                    return (false, null!, $"Failed to update PettyCash entry: {res.Item2}");
                }
                else
                {
                    return (false, null!, "Failed to update PettyCash entry. The record may not exist or no changes were detected.");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PettyCashService.Put Error: {Message}", ex.Message);
            return (false, null!, $"An error occurred while updating PettyCash entry: {ex.Message}");
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        try
        {
            if (id == 0)
            {
                return (false, "PettyCash entry ID is required. Cannot delete without a valid ID.");
            }

            // Check if record exists
            var pettyCash = await dapper.SearchByQuery<PettyCashModel>($"SELECT * FROM PettyCash WHERE Id = {id} AND IsSoftDeleted = 0");
            if (pettyCash == null || !pettyCash.Any())
            {
                return (false, $"PettyCash entry with ID {id} not found or has already been deleted.");
            }

            var existing = pettyCash.First();

            // Check if pettycash is posted to GL - prevent deletion
            if (existing.IsPostedToGL == 1)
            {
                return (false, $"Cannot delete PettyCash entry '{existing.SeqNo}' because it has already been posted to General Ledger (GL Entry: {existing.GLEntryNo ?? "N/A"}). Please reverse the GL entry first if you need to delete this record.");
            }

            var result = await dapper.Delete("PettyCash", id);
            if (result.Item1)
            {
                return (true, $"PettyCash entry '{existing.SeqNo}' has been deleted successfully.");
            }
            else
            {
                return (false, $"Failed to delete PettyCash entry: {result.Item2}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PettyCashService.Delete Error: {Message}", ex.Message);
            return (false, $"An error occurred while deleting PettyCash entry: {ex.Message}");
        }
    }

    public async Task<(bool, string)> SoftDelete(PettyCashModel obj)
    {
        try
        {
            if (obj.Id == 0)
            {
                return (false, "PettyCash entry ID is required. Cannot delete without a valid ID.");
            }

            // Check if record exists
            var pettyCash = await dapper.SearchByQuery<PettyCashModel>($"SELECT * FROM PettyCash WHERE Id = {obj.Id} AND IsSoftDeleted = 0");
            if (pettyCash == null || !pettyCash.Any())
            {
                return (false, $"PettyCash entry with ID {obj.Id} not found or has already been deleted.");
            }

            var existing = pettyCash.First();

            // Check if pettycash is posted to GL - prevent deletion
            if (existing.IsPostedToGL == 1)
            {
                return (false, $"Cannot delete PettyCash entry '{existing.SeqNo}' because it has already been posted to General Ledger (GL Entry: {existing.GLEntryNo ?? "N/A"}). Please reverse the GL entry first if you need to delete this record.");
            }

            string SQLUpdate = $@"UPDATE PettyCash SET 
					UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
					UpdatedBy = {obj.UpdatedBy},
					UpdatedFrom = '{obj.UpdatedFrom?.Replace("'", "''").ToUpper() ?? ""}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

            var result = await dapper.Update(SQLUpdate);
            if (result.Item1)
            {
                return (true, $"PettyCash entry '{existing.SeqNo}' has been deleted successfully.");
            }
            else
            {
                return (false, $"Failed to delete PettyCash entry: {result.Item2}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PettyCashService.SoftDelete Error: {Message}", ex.Message);
            return (false, $"An error occurred while deleting PettyCash entry: {ex.Message}");
        }
    }
}

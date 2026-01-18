using NG.MicroERP.API.Helper;
using Serilog;

namespace NG.MicroERP.API.Services.Services;

public interface IControlCenterService
{
    Task<bool> IsAccountActiveAsync(string code);
    Task<(bool IsActive, string? Message)> CheckAccountStatusAsync(string code);
    Task<(bool Success, int UserId, string Message)> RegisterUserAsync(string fullName, string email, string? phone, string passwordHash);
    Task<bool> CheckEmailExistsAsync(string email);
    Task<(bool Success, int OrganizationId, string Message)> CreateOrganizationAsync(int ownerUserId, string organizationName, string? email, string? phone);
}

public class ControlCenterService : IControlCenterService
{
    private readonly DapperFunctions _dapper;

    public ControlCenterService()
    {
        _dapper = new DapperFunctions();
    }

    public async Task<bool> IsAccountActiveAsync(string code)
    {
        try
        {
            var result = await CheckAccountStatusAsync(code);
            return result.IsActive;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking account status for code: {Code}", code);
            // Return true as fallback to allow login if ControlCenter is unavailable
            return true;
        }
    }

    public async Task<(bool IsActive, string? Message)> CheckAccountStatusAsync(string code)
    {
        try
        {
            if (string.IsNullOrEmpty(code))
            {
                return (false, "Organization code is required");
            }

            string codeEscaped = code.Replace("'", "''");
            string sql = $@"
               SELECT TOP 1 
                    IsActive, 
                    IsSoftDeleted,
                    Expiry,
                    OrganizationName
                FROM Organizations
                WHERE UPPER(Code) = UPPER('{codeEscaped}')";

            var results = await _dapper.SearchByQuery<dynamic>(sql, "ControlCenter");
            
            if (results == null || results.Count == 0)
            {
                return (false, "Account not found in ControlCenter");
            }

            var result = results.FirstOrDefault();
            if (result == null)
            {
                return (false, "Account not found in ControlCenter");
            }

            bool isActive = result.IsActive == 1;
            bool isSoftDeleted = result.IsSoftDeleted == 1;
            DateTime? expiry = result.Expiry != null ? (DateTime?)result.Expiry : null;

            if (isSoftDeleted)
            {
                return (false, "Account has been deactivated");
            }

            // Check expiry date
            if (expiry.HasValue && expiry.Value < DateTime.Now)
            {
                string message = $"Account has expired on {expiry.Value:yyyy-MM-dd}";
                if (result.PaymentInstructions != null)
                {
                    message += $". {result.PaymentInstructions}";
                }
                return (false, message);
            }

            if (!isActive)
            {
                string message = "Account is inactive";
                if (result.PaymentInstructions != null)
                {
                    message += $". {result.PaymentInstructions}";
                }
                return (false, message);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking account status for code: {Code}", code);
            // Return true as fallback to allow login if ControlCenter check fails
            return (true, null);
        }
    }

    public async Task<(bool Success, int UserId, string Message)> RegisterUserAsync(string fullName, string email, string? phone, string passwordHash)
    {
        try
        {
            // Check if email already exists
            var emailExists = await CheckEmailExistsAsync(email);
            if (emailExists)
            {
                return (false, 0, "Email already exists");
            }

            // Escape SQL values
            string fullNameEscaped = fullName.Replace("'", "''");
            string emailEscaped = email.Replace("'", "''");
            string phoneEscaped = phone?.Replace("'", "''") ?? "";
            string passwordHashEscaped = passwordHash.Replace("'", "''");

            string sqlInsert = $@"
                INSERT INTO Users (FullName, Email, Phone, PasswordHash, IsSuperAdmin, IsActive, CreatedBy, CreatedFrom, IsSoftDeleted)
                VALUES ('{fullNameEscaped}', '{emailEscaped}', '{phoneEscaped}', '{passwordHashEscaped}', 0, 1, 1, 'WebSite', 0);";

            var insertResult = await _dapper.ExecuteQuery(sqlInsert, "ControlCenter");
            
            if (insertResult.Item1)
            {
                // Get the inserted ID
                string sqlGetId = "SELECT MAX(Id) FROM Users WHERE Email = @Email";
                var idResults = await _dapper.SearchByQuery<int>(sqlGetId.Replace("@Email", $"'{emailEscaped}'"), "ControlCenter");
                
                if (idResults != null && idResults.Count > 0)
                {
                    int userId = idResults[0];
                    return (true, userId, "User registered successfully");
                }
            }

            return (false, 0, "Failed to register user");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering user: {Email}", email);
            return (false, 0, $"Error: {ex.Message}");
        }
    }

    public async Task<bool> CheckEmailExistsAsync(string email)
    {
        try
        {
            string emailEscaped = email.Replace("'", "''");
            string sql = $"SELECT COUNT(*) FROM Users WHERE UPPER(Email) = UPPER('{emailEscaped}') AND IsSoftDeleted = 0";
            
            var results = await _dapper.SearchByQuery<int>(sql, "ControlCenter");
            return results != null && results.Count > 0 && results[0] > 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking email: {Email}", email);
            return false;
        }
    }

    public async Task<(bool Success, int OrganizationId, string Message)> CreateOrganizationAsync(int ownerUserId, string organizationName, string? email, string? phone)
    {
        try
        {
            // Generate organization code
            string orgCode = await GenerateOrganizationCodeAsync();
            
            // Escape SQL values
            string orgNameEscaped = organizationName.Replace("'", "''");
            string emailEscaped = email?.Replace("'", "''") ?? "";
            string phoneEscaped = phone?.Replace("'", "''") ?? "";

            // Set expiry to 1 year from now
            DateTime expiryDate = DateTime.Now.AddYears(1);

            string sqlInsert = $@"
                INSERT INTO Organizations (Code, OrganizationName, OwnerUserId, Email, Phone, IsActive, Expiry, CreatedBy, CreatedFrom, IsSoftDeleted)
                VALUES ('{orgCode}', '{orgNameEscaped}', {ownerUserId}, '{emailEscaped}', '{phoneEscaped}', 1, '{expiryDate:yyyy-MM-dd HH:mm:ss}', {ownerUserId}, 'WebSite', 0);";

            var insertResult = await _dapper.ExecuteQuery(sqlInsert, "ControlCenter");
            
            if (insertResult.Item1)
            {
                // Get the inserted ID
                string sqlGetId = $"SELECT MAX(Id) FROM Organizations WHERE Code = '{orgCode}'";
                var idResults = await _dapper.SearchByQuery<int>(sqlGetId, "ControlCenter");
                
                if (idResults != null && idResults.Count > 0)
                {
                    int orgId = idResults[0];
                    return (true, orgId, "Organization created successfully");
                }
            }

            return (false, 0, "Failed to create organization");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating organization: {OrganizationName}", organizationName);
            return (false, 0, $"Error: {ex.Message}");
        }
    }

    private async Task<string> GenerateOrganizationCodeAsync()
    {
        try
        {
            string sql = "SELECT MAX(CAST(SUBSTRING(Code, 4, LEN(Code) - 3) AS INT)) FROM Organizations WHERE LEFT(Code, 3) = 'ORG' AND ISNUMERIC(SUBSTRING(Code, 4, LEN(Code) - 3)) = 1";
            var results = await _dapper.SearchByQuery<int?>(sql, "ControlCenter");
            
            int nextNumber = (results != null && results.Count > 0 && results[0].HasValue) ? results[0].Value + 1 : 1;
            return $"ORG{nextNumber:D6}";
        }
        catch
        {
            return $"ORG{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}

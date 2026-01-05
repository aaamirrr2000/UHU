using Dapper;
using Microsoft.Data.SqlClient;
using NG.MicroERP.API.Helper;
using NG.MicroERP.API.Services;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services;

public interface IChartOfAccountsService
{
    Task<(bool, List<ChartOfAccountsModel>)>? Search(string Criteria = "");
    Task<(bool, ChartOfAccountsModel?)>? Get(int id);
    Task<(bool, ChartOfAccountsModel, string)> Post(ChartOfAccountsModel obj);
    Task<(bool, ChartOfAccountsModel, string)> Put(ChartOfAccountsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ChartOfAccountsModel obj);
}

public class ChartOfAccountsService : IChartOfAccountsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ChartOfAccountsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM ChartOfAccounts Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by code, Id";

        List<ChartOfAccountsModel> result = (await dapper.SearchByQuery<ChartOfAccountsModel>(SQL)) ?? new List<ChartOfAccountsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, ChartOfAccountsModel?)>? Get(int id)
    {
        ChartOfAccountsModel result = (await dapper.SearchByID<ChartOfAccountsModel>("ChartOfAccounts", id)) ?? new ChartOfAccountsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, ChartOfAccountsModel, string)> Post(ChartOfAccountsModel obj)
    {

        try
        {
            string Code = GetAccountCode(obj.Type.ToUpper());
            string SQLDuplicate = $@"SELECT * FROM ChartOfAccounts WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO ChartOfAccounts 
			(
				OrganizationId, 
				Code,
                Pic,
				Name, 
				Type, 
				InterfaceType, 
				Description, 
				ParentId, 
				OpeningBalance, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{Code.ToUpper()}', 
				'{obj.Pic}', 
				'{obj.Name!.ToUpper()}', 
				'{obj.Type!.ToUpper()}', 
				'{obj.InterfaceType!.ToUpper()}', 
				'{obj.Description!.ToUpper()}', 
				{obj.ParentId},
				{obj.OpeningBalance},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ChartOfAccountsModel> Output = new List<ChartOfAccountsModel>();
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
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, ChartOfAccountsModel, string)> Put(ChartOfAccountsModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM ChartOfAccounts WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE ChartOfAccounts SET 
					OrganizationId = {obj.OrganizationId}, 
					Code = '{obj.Code!.ToUpper()}',
                    Pic = '{obj.Pic}',
					Name = '{obj.Name!.ToUpper()}', 
					Type = '{obj.Type!.ToUpper()}', 
					InterfaceType = '{obj.InterfaceType!.ToUpper()}', 
					Description = '{obj.Description!.ToUpper()}', 
					ParentId = {obj.ParentId}, 
					OpeningBalance = {obj.OpeningBalance}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ChartOfAccountsModel> Output = new List<ChartOfAccountsModel>();
                var result = await Search($"id={obj.Id}")!;
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
        return await dapper.Delete("ChartOfAccounts", id);
    }

    public async Task<(bool, string)> SoftDelete(ChartOfAccountsModel obj)
    {
        string SQLUpdate = $@"UPDATE ChartOfAccounts SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }

    public string? GetAccountCode(string accountType)
    {
        // Define base series by account type
        int startRange = accountType.ToUpper() switch
        {
            "ASSET" => 10000,
            "LIABILITY" => 20000,
            "REVENUE" => 30000,
            "EXPENSE" => 40000,
            "EQUITY" => 50000,
            _ => 90000 // Fallback for unknown types
        };

        // Define the upper boundary for each range
        int endRange = startRange + 9999;

        string sql = $@"SELECT MAX(CAST(Code AS INT)) AS MaxCode FROM ChartOfAccounts WHERE TRY_CAST(Code AS INT) BETWEEN {startRange} AND {endRange};";

        try
        {
            Config cfg = new Config();
            using var cnn = new SqlConnection(cfg.DefaultConnectionString);
            cnn.Open();

            int? maxCode = cnn.QueryFirstOrDefault<int?>(sql);
            int nextCode = (maxCode ?? startRange) + 1;

            if (nextCode > endRange)
                throw new Exception($"Code limit reached for account type {accountType}.");

            return nextCode.ToString();
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"GetAccountCode Error for type {accountType}, table ChartOfAccounts", accountType, "ChartOfAccounts");
            return null;
        }
    }

}


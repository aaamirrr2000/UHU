using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IBankService
{
    Task<(bool, List<BankModel>)>? Search(string Criteria = "");
    Task<(bool, BankModel?)>? Get(int id);
    Task<(bool, BankModel, string)> Post(BankModel obj);
    Task<(bool, BankModel, string)> Put(BankModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(BankModel obj);
}


public class BankService : IBankService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<BankModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                          a.*,
                          b.AreaName,
                          c.Name as Account
                        from Bank as a 
                        left join Areas b on b.id=a.CityId
                        left join ChartOfAccounts c on c.id=a.AccountId
                        Where a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<BankModel> result = (await dapper.SearchByQuery<BankModel>(SQL)) ?? new List<BankModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, BankModel?)>? Get(int id)
    {
        BankModel result = (await dapper.SearchByID<BankModel>("Bank", id)) ?? new BankModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, BankModel, string)> Post(BankModel obj)
    {

        try
        {

            string Code = dapper.GetCode("", "Bank", "Code", 3)!;
            string SQLDuplicate = $@"SELECT * FROM Bank WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Bank 
			(
				OrganizationId, 
				Code, 
				BankName, 
				BranchCode, 
				BranchName, 
				AccountTitle, 
				AccountNumber, 
				IBAN, 
				Address, 
				CityId, 
				AccountId, 
				Phone, 
				Email, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.Code!.ToUpper()}', 
				'{obj.BankName!.ToUpper()}', 
				'{obj.BranchCode!.ToUpper()}', 
				'{obj.BranchName!.ToUpper()}', 
				'{obj.AccountTitle!.ToUpper()}', 
				'{obj.AccountNumber!.ToUpper()}', 
				'{obj.IBAN!.ToUpper()}', 
				'{obj.Address!.ToUpper()}', 
				{obj.CityId},
				{obj.AccountId},
				'{obj.Phone!.ToUpper()}', 
				'{obj.Email!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<BankModel> Output = new List<BankModel>();
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

    public async Task<(bool, BankModel, string)> Put(BankModel obj)

    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Bank WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE Bank SET 
					OrganizationId = {obj.OrganizationId}, 
					Code = '{obj.Code!.ToUpper()}', 
					BankName = '{obj.BankName!.ToUpper()}', 
					BranchCode = '{obj.BranchCode!.ToUpper()}', 
					BranchName = '{obj.BranchName!.ToUpper()}', 
					AccountTitle = '{obj.AccountTitle!.ToUpper()}', 
					AccountNumber = '{obj.AccountNumber!.ToUpper()}', 
					IBAN = '{obj.IBAN!.ToUpper()}', 
					Address = '{obj.Address!.ToUpper()}', 
					CityId = {obj.CityId}, 
					AccountId = {obj.AccountId}, 
					Phone = '{obj.Phone!.ToUpper()}', 
					Email = '{obj.Email!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<BankModel> Output = new List<BankModel>();
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
            return (true, null!, ex.Message);
        }

    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Bank", id);
    }

    public async Task<(bool, string)> SoftDelete(BankModel obj)
    {
        string SQLUpdate = $@"UPDATE Bank SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

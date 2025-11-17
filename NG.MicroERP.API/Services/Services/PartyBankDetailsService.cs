using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IPartyBankDetailsService
{
    Task<(bool, List<PartyBankDetailsModel>)>? Search(string Criteria = "");
    Task<(bool, PartyBankDetailsModel?)>? Get(int id);
    Task<(bool, PartyBankDetailsModel, string)> Post(PartyBankDetailsModel obj);
    Task<(bool, PartyBankDetailsModel, string)> Put(PartyBankDetailsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PartyBankDetailsModel obj);
}


public class PartyBankDetailsService : IPartyBankDetailsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PartyBankDetailsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                          a.*,
                          b.name as BankName
                        FROM PartyBankDetails as a
                        left join parties b on b.Id=a.BankId and b.PartyType='BANK'
                        Where a.IsSoftDeleted=0 and a.IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PartyBankDetailsModel> result = (await dapper.SearchByQuery<PartyBankDetailsModel>(SQL)) ?? new List<PartyBankDetailsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PartyBankDetailsModel?)>? Get(int id)
    {
        PartyBankDetailsModel result = (await dapper.SearchByID<PartyBankDetailsModel>("PartyBankDetails", id)) ?? new PartyBankDetailsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PartyBankDetailsModel, string)> Post(PartyBankDetailsModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO PartyBankDetails 
			(
				PartyId, 
				BankId, 
				AccountTitle, 
				AccountNumber, 
				IBAN, 
				BranchCode, 
				IsPrimary, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted 
			) 
			VALUES 
			(
				{obj.PartyId},
				{obj.BankId},
				'{obj.AccountTitle!.ToUpper()}', 
				'{obj.AccountNumber!.ToUpper()}', 
				'{obj.IBAN!.ToUpper()}', 
				'{obj.BranchCode!.ToUpper()}', 
				{obj.IsPrimary},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PartyBankDetailsModel> Output = new List<PartyBankDetailsModel>();
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

    public async Task<(bool, PartyBankDetailsModel, string)> Put(PartyBankDetailsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE PartyBankDetails SET 
					PartyId = {obj.PartyId}, 
					BankId = {obj.BankId}, 
					AccountTitle = '{obj.AccountTitle!.ToUpper()}', 
					AccountNumber = '{obj.AccountNumber!.ToUpper()}', 
					IBAN = '{obj.IBAN!.ToUpper()}', 
					BranchCode = '{obj.BranchCode!.ToUpper()}', 
					IsPrimary = {obj.IsPrimary}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted}
                    WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<PartyBankDetailsModel> Output = new List<PartyBankDetailsModel>();
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
        return await dapper.Delete("PartyBankDetails", id);
    }

    public async Task<(bool, string)> SoftDelete(PartyBankDetailsModel obj)
    {
        string SQLUpdate = $@"UPDATE PartyBankDetails SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



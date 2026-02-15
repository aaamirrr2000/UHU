using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;


public interface IPaymentTermsService
{
    Task<(bool, List<PaymentTermsModel>)>? Search(string Criteria = "");
    Task<(bool, PaymentTermsModel?)>? Get(int id);
    Task<(bool, PaymentTermsModel, string)> Post(PaymentTermsModel obj);
    Task<(bool, PaymentTermsModel, string)> Put(PaymentTermsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PaymentTermsModel obj);
}


public class PaymentTermsService : IPaymentTermsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PaymentTermsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM PaymentTerms Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PaymentTermsModel> result = (await dapper.SearchByQuery<PaymentTermsModel>(SQL)) ?? new List<PaymentTermsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PaymentTermsModel?)>? Get(int id)
    {
        PaymentTermsModel result = (await dapper.SearchByID<PaymentTermsModel>("PaymentTerms", id)) ?? new PaymentTermsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PaymentTermsModel, string)> Post(PaymentTermsModel obj)
    {

        try
        {
            string Code = dapper.GetCode("", "PaymentTerms", "Code")!;
            string SQLDuplicate = $@"SELECT * FROM PaymentTerms WHERE UPPER(code) = '{obj.Code!.ToUpper()}' AND IsSoftDeleted = 0;";
            string SQLInsert = $@"INSERT INTO PaymentTerms 
			(
				Code, 
				Description, 
				DaysDue, 
				IsDefault, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				IsSoftDeleted
			) 
			VALUES 
			(
				'{obj.Code!.ToUpper()}', 
				'{obj.Description!.ToUpper()}', 
				{obj.DaysDue},
				{obj.IsDefault},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<PaymentTermsModel> Output = new List<PaymentTermsModel>();
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

    public async Task<(bool, PaymentTermsModel, string)> Put(PaymentTermsModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM PaymentTerms WHERE UPPER(code) = '{obj.Code!.ToUpper()}' AND Id != {obj.Id} AND IsSoftDeleted = 0;";
            string SQLUpdate = $@"UPDATE PaymentTerms SET 
					Code = '{obj.Code!.ToUpper()}', 
					Description = '{obj.Description!.ToUpper()}', 
					DaysDue = {obj.DaysDue}, 
					IsDefault = {obj.IsDefault}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<PaymentTermsModel> Output = new List<PaymentTermsModel>();
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

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("PaymentTerms", id);
    }

    public async Task<(bool, string)> SoftDelete(PaymentTermsModel obj)
    {
        string SQLUpdate = $@"UPDATE PaymentTerms SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}




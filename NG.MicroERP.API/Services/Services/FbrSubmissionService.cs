using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IFbrSubmissionService
{
    Task<(bool, List<FbrSubmissionModel>)>? Search(string Criteria = "");
    Task<(bool, FbrSubmissionModel?)>? Get(int id);
    Task<(bool, FbrSubmissionModel, string)> Post(FbrSubmissionModel obj);
    Task<(bool, string)> Put(FbrSubmissionModel obj);
    Task<(bool, string)> Delete(int id);
}


public class FbrSubmissionService : IFbrSubmissionService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<FbrSubmissionModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM FbrSubmission Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<FbrSubmissionModel> result = (await dapper.SearchByQuery<FbrSubmissionModel>(SQL)) ?? new List<FbrSubmissionModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, FbrSubmissionModel?)>? Get(int id)
    {
        FbrSubmissionModel result = (await dapper.SearchByID<FbrSubmissionModel>("FbrSubmission", id)) ?? new FbrSubmissionModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, FbrSubmissionModel, string)> Post(FbrSubmissionModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO FbrSubmission 
			(
				BillId, 
				JsonPayload, 
				SubmissionDateTime, 
				ResponseCode, 
				IRN, 
				DigitalInvoiceUrl, 
				QRCodeData, 
				ErrorMessage, 
				RetryCount, 
				SubmissionMachineInfo, 
				CreatedOn
			) 
			VALUES 
			(
				{obj.BillId},
				'{obj.JsonPayload!.ToUpper()}', 
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.ResponseCode!.ToUpper()}', 
				'{obj.IRN!.ToUpper()}', 
				'{obj.DigitalInvoiceUrl!.ToUpper()}', 
				'{obj.QRCodeData!.ToUpper()}', 
				'{obj.ErrorMessage!.ToUpper()}', 
				{obj.RetryCount},
				'{obj.SubmissionMachineInfo!.ToUpper()}', 
				'{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}'
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<FbrSubmissionModel> Output = new List<FbrSubmissionModel>();
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

    public async Task<(bool, string)> Put(FbrSubmissionModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE FbrSubmission SET 
					BillId = {obj.BillId}, 
					JsonPayload = '{obj.JsonPayload!.ToUpper()}', 
					SubmissionDateTime = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', 
					ResponseCode = '{obj.ResponseCode!.ToUpper()}', 
					IRN = '{obj.IRN!.ToUpper()}', 
					DigitalInvoiceUrl = '{obj.DigitalInvoiceUrl!.ToUpper()}', 
					QRCodeData = '{obj.QRCodeData!.ToUpper()}', 
					ErrorMessage = '{obj.ErrorMessage!.ToUpper()}', 
					RetryCount = {obj.RetryCount}, 
					SubmissionMachineInfo = '{obj.SubmissionMachineInfo!.ToUpper()}', 
					CreatedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}' 
				WHERE Id = {obj.Id};";

            return await dapper.Update(SQLUpdate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("FbrSubmission", id);
    }


}



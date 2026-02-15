using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;


public interface IPartyDocumentsService
{
    Task<(bool, List<PartyDocumentsModel>)>? Search(string Criteria = "");
    Task<(bool, PartyDocumentsModel?)>? Get(int id);
    Task<(bool, PartyDocumentsModel, string)> Post(PartyDocumentsModel obj);
    Task<(bool, PartyDocumentsModel, string)> Put(PartyDocumentsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PartyDocumentsModel obj);
}


public class PartyDocumentsService : IPartyDocumentsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PartyDocumentsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM PartyDocuments Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PartyDocumentsModel> result = (await dapper.SearchByQuery<PartyDocumentsModel>(SQL)) ?? new List<PartyDocumentsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PartyDocumentsModel?)>? Get(int id)
    {
        PartyDocumentsModel result = (await dapper.SearchByID<PartyDocumentsModel>("PartyDocuments", id)) ?? new PartyDocumentsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PartyDocumentsModel, string)> Post(PartyDocumentsModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO PartyDocuments 
			(
				PartyId, 
				DocumentType, 
				DocumentNumber, 
				PointOfContact, 
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
				{obj.PartyId},
				'{obj.DocumentType!.ToUpper()}', 
				'{obj.DocumentNumber!.ToUpper()}', 
				'{obj.PointOfContact!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.UpdatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.UpdatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PartyDocumentsModel> Output = new List<PartyDocumentsModel>();
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

    public async Task<(bool, PartyDocumentsModel, string)> Put(PartyDocumentsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE PartyDocuments SET 
					PartyId = {obj.PartyId}, 
					DocumentType = '{obj.DocumentType!.ToUpper()}', 
					DocumentNumber = '{obj.DocumentNumber!.ToUpper()}', 
					PointOfContact = '{obj.PointOfContact!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					CreatedBy = {obj.CreatedBy}, 
					CreatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					CreatedFrom = '{obj.CreatedFrom!.ToUpper()}', 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<PartyDocumentsModel> Output = new List<PartyDocumentsModel>();
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
        return await dapper.Delete("PartyDocuments", id);
    }

    public async Task<(bool, string)> SoftDelete(PartyDocumentsModel obj)
    {
        string SQLUpdate = $@"UPDATE PartyDocuments SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}




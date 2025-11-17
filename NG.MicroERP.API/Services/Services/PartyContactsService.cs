using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IPartyContactsService
{
    Task<(bool, List<PartyContactsModel>)>? Search(string Criteria = "");
    Task<(bool, PartyContactsModel?)>? Get(int id);
    Task<(bool, PartyContactsModel, string)> Post(PartyContactsModel obj);
    Task<(bool, PartyContactsModel, string)> Put(PartyContactsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PartyContactsModel obj);
}


public class PartyContactsService : IPartyContactsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PartyContactsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM PartyContacts Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PartyContactsModel> result = (await dapper.SearchByQuery<PartyContactsModel>(SQL)) ?? new List<PartyContactsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PartyContactsModel?)>? Get(int id)
    {
        PartyContactsModel result = (await dapper.SearchByID<PartyContactsModel>("PartyContacts", id)) ?? new PartyContactsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PartyContactsModel, string)> Post(PartyContactsModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO PartyContacts 
			(
				PartyId, 
				ContactType, 
				ContactValue, 
				IsPrimary, 
				PointOfContact, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.PartyId},
				'{obj.ContactType!.ToUpper()}', 
				'{obj.ContactValue!}', 
				{obj.IsPrimary},
				'{obj.PointOfContact!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PartyContactsModel> Output = new List<PartyContactsModel>();
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

    public async Task<(bool, PartyContactsModel, string)> Put(PartyContactsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE PartyContacts SET 
					PartyId = {obj.PartyId}, 
					ContactType = '{obj.ContactType!.ToUpper()}', 
					ContactValue = '{obj.ContactValue!}', 
					IsPrimary = {obj.IsPrimary}, 
					PointOfContact = '{obj.PointOfContact!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<PartyContactsModel> Output = new List<PartyContactsModel>();
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
        return await dapper.Delete("PartyContacts", id);
    }

    public async Task<(bool, string)> SoftDelete(PartyContactsModel obj)
    {
        string SQLUpdate = $@"UPDATE PartyContacts SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



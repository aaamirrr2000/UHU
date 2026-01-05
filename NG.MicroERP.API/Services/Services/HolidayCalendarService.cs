using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IHolidayCalendarService
{
    Task<(bool, List<HolidayCalendarModel>)>? Search(string Criteria = "");
    Task<(bool, HolidayCalendarModel?)>? Get(int id);
    Task<(bool, HolidayCalendarModel, string)> Post(HolidayCalendarModel obj);
    Task<(bool, HolidayCalendarModel, string)> Put(HolidayCalendarModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(HolidayCalendarModel obj);
}


public class HolidayCalendarService : IHolidayCalendarService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<HolidayCalendarModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM HolidayCalendar Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<HolidayCalendarModel> result = (await dapper.SearchByQuery<HolidayCalendarModel>(SQL)) ?? new List<HolidayCalendarModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, HolidayCalendarModel?)>? Get(int id)
    {
        HolidayCalendarModel result = (await dapper.SearchByID<HolidayCalendarModel>("HolidayCalendar", id)) ?? new HolidayCalendarModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, HolidayCalendarModel, string)> Post(HolidayCalendarModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO HolidayCalendar 
			(
				OrganizationId, 
				HolidayDate, 
				Description, 
				IsRecurring, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.Description!.ToUpper()}', 
				{obj.IsRecurring},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<HolidayCalendarModel> Output = new List<HolidayCalendarModel>();
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

    public async Task<(bool, HolidayCalendarModel, string)> Put(HolidayCalendarModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE HolidayCalendar SET 
					OrganizationId = {obj.OrganizationId}, 
					HolidayDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					Description = '{obj.Description!.ToUpper()}', 
					IsRecurring = {obj.IsRecurring}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<HolidayCalendarModel> Output = new List<HolidayCalendarModel>();
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
        return await dapper.Delete("HolidayCalendar", id);
    }

    public async Task<(bool, string)> SoftDelete(HolidayCalendarModel obj)
    {
        string SQLUpdate = $@"UPDATE HolidayCalendar SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



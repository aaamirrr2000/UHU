using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;


public interface IShiftsService
{
    Task<(bool, List<ShiftsModel>)>? Search(string Criteria = "");
    Task<(bool, ShiftsModel?)>? Get(int id);
    Task<(bool, ShiftsModel, string)> Post(ShiftsModel obj);
    Task<(bool, ShiftsModel, string)> Put(ShiftsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ShiftsModel obj);
}


public class ShiftsService : IShiftsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ShiftsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"
                    SELECT 
                      Id,
                      OrganizationId,
                      ShiftName,
                      CONVERT(VARCHAR(8), StartTime, 108) AS StartTime,
                      CONVERT(VARCHAR(8), EndTime, 108)   AS EndTime,
                      FlexiTime,
                      IsActive,
                      CreatedBy,
                      CreatedOn,
                      CreatedFrom,
                      UpdatedBy,
                      UpdatedOn,
                      UpdatedFrom,
                      IsSoftDeleted
                    FROM shifts
                    WHERE IsSoftDeleted = 0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<ShiftsModel> result = (await dapper.SearchByQuery<ShiftsModel>(SQL)) ?? new List<ShiftsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, ShiftsModel?)>? Get(int id)
    {
        string SQL = $@"
                        SELECT 
                            Id,
                            OrganizationId,
                            ShiftName,
                      CONVERT(VARCHAR(8), StartTime, 108) AS StartTime,
                      CONVERT(VARCHAR(8), EndTime, 108)   AS EndTime,
                            FlexiTime,
                            IsActive,
                            CreatedBy,
                            CreatedOn,
                            CreatedFrom,
                            UpdatedBy,
                            UpdatedOn,
                            UpdatedFrom,
                            IsSoftDeleted
                        FROM shifts
                        WHERE IsSoftDeleted = 0
                        AND Id = {id} 
                        ORDER BY Id Desc";

        List<ShiftsModel> result = (await dapper.SearchByQuery<ShiftsModel>(SQL)) ?? new List<ShiftsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
        {
            var r = result.FirstOrDefault();
            return (true, r);
        }

    }


    public async Task<(bool, ShiftsModel, string)> Post(ShiftsModel obj)
    {

        try
        {
            string SQLDuplicate = $@"SELECT * FROM shifts WHERE UPPER(ShiftName) = '{obj.ShiftName!.ToUpper()}' AND IsSoftDeleted = 0;";
            string SQLInsert = $@"INSERT INTO shifts 
			(
				OrganizationId, 
				ShiftName, 
				StartTime, 
				EndTime, 
				FlexiTime, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.ShiftName!.ToUpper()}', 
				'{DateTime.Parse(obj.StartTime).ToString("HH:mm:ss")}',
				'{DateTime.Parse(obj.EndTime).ToString("HH:mm:ss")}',
				{obj.FlexiTime},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 0
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ShiftsModel> Output = new List<ShiftsModel>();
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

    public async Task<(bool, ShiftsModel, string)> Put(ShiftsModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM shifts WHERE UPPER(ShiftName) = '{obj.ShiftName!.ToUpper()}' AND Id != {obj.Id} AND IsSoftDeleted = 0;";
            string SQLUpdate = $@"UPDATE shifts SET 
					OrganizationId = {obj.OrganizationId}, 
					ShiftName = '{obj.ShiftName!.ToUpper()}', 
					StartTime = '{DateTime.Parse(obj.StartTime).ToString("HH:mm:ss")}', 
					EndTime = '{DateTime.Parse(obj.EndTime).ToString("HH:mm:ss")}', 
					FlexiTime = {obj.FlexiTime}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}'
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<ShiftsModel> Output = new List<ShiftsModel>();
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
        return await dapper.Delete("shifts", id);
    }

    public async Task<(bool, string)> SoftDelete(ShiftsModel obj)
    {
        string SQLUpdate = $@"UPDATE shifts SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}




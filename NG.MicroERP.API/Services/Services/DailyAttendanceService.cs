using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;

public interface IDailyAttendanceService
{
    Task<(bool, List<DailyAttendanceModel>)>? Search(string Criteria = "");
    Task<(bool, DailyAttendanceModel?)>? Get(int id);
}


public class DailyAttendanceService : IDailyAttendanceService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<DailyAttendanceModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM dailyattendance";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " where " + Criteria;

        SQL += " Order by EmpId Desc";

        List<DailyAttendanceModel> result = (await dapper.SearchByQuery<DailyAttendanceModel>(SQL)) ?? new List<DailyAttendanceModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, DailyAttendanceModel?)>? Get(int id)
    {
        DailyAttendanceModel result = (await dapper.SearchByID<DailyAttendanceModel>("Areas", id)) ?? new DailyAttendanceModel();
        if (result == null)
            return (false, null);
        else
            return (true, result);
    }
}



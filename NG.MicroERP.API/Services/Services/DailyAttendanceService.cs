using Dapper;
using MySql.Data.MySqlClient;
using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;
using System.Data;

namespace NG.MicroERP.API.Services.Services;

public interface IDailyAttendanceService
{
    Task<(bool, List<DailyAttendanceModel>)>? Search(string Criteria = "");
    Task<List<DailyAttendanceModel>> GetAttendanceReportAsync(string startDate, string endDate);
}

public class DailyAttendanceService : IDailyAttendanceService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<DailyAttendanceModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM dailyattendance Where IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<DailyAttendanceModel> result = (await dapper.SearchByQuery<DailyAttendanceModel>(SQL)) ?? new List<DailyAttendanceModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<List<DailyAttendanceModel>> GetAttendanceReportAsync(string startDate, string endDate)
    {
        if (!DateTime.TryParse(startDate, out DateTime startDateValue) ||
            !DateTime.TryParse(endDate, out DateTime endDateValue))
        {
            throw new ArgumentException("Invalid date format provided to GetAttendanceReportAsync.");
        }

        var parameters = new DynamicParameters();
        parameters.Add("start_date", startDateValue.Date, DbType.Date);
        parameters.Add("end_date", endDateValue.Date, DbType.Date);

        Config cfg = new Config();

        string DBConnection = cfg.DefaultDB();

        using IDbConnection connection = new MySqlConnection(DBConnection);

        var result = await connection.QueryAsync<DailyAttendanceModel>(
            "GetAttendanceReport",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        return result.AsList();
    }
}


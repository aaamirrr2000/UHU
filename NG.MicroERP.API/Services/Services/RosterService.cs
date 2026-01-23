using System;
using System.Collections.Generic;
using System.Linq;
using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;
using Serilog;

namespace NG.MicroERP.API.Services.Services;

public interface IRosterService
{
    Task<(bool, List<RosterModel>)>? Search(string Criteria = "");
    Task<(bool, RosterModel?)>? Get(int id);
    Task<(bool, RosterModel, string)> Post(RosterModel obj);
    Task<(bool, RosterModel, string)> Put(RosterModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(RosterModel obj);
}

public class RosterService : IRosterService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<RosterModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"
            SELECT 
                r.Id,
                r.OrganizationId,
                r.EmployeeId,
                ISNULL(e.Fullname, '') AS EmployeeName,
                ISNULL(e.EmpId, '') AS EmployeeCode,
                r.ShiftId,
                ISNULL(s.ShiftName, '') AS ShiftName,
                CONVERT(VARCHAR(8), s.StartTime, 108) AS StartTime,
                CONVERT(VARCHAR(8), s.EndTime, 108) AS EndTime,
                r.RosterDate,
                r.LocationId,
                ISNULL(l.Name, '') AS LocationName,
                r.Notes,
                r.IsActive,
                r.CreatedBy,
                r.CreatedOn,
                r.CreatedFrom,
                r.UpdatedBy,
                r.UpdatedOn,
                r.UpdatedFrom,
                r.IsSoftDeleted
            FROM Roster AS r
            LEFT JOIN Employees AS e ON e.Id = r.EmployeeId
            LEFT JOIN Shifts AS s ON s.Id = r.ShiftId
            LEFT JOIN Locations AS l ON l.Id = r.LocationId
            WHERE r.IsSoftDeleted = 0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " AND " + Criteria;

        SQL += " ORDER BY r.RosterDate DESC, e.Fullname, s.ShiftName";

        List<RosterModel> result = (await dapper.SearchByQuery<RosterModel>(SQL)) ?? new List<RosterModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, RosterModel?)>? Get(int id)
    {
        string SQL = $@"
            SELECT 
                r.Id,
                r.OrganizationId,
                r.EmployeeId,
                ISNULL(e.Fullname, '') AS EmployeeName,
                ISNULL(e.EmpId, '') AS EmployeeCode,
                r.ShiftId,
                ISNULL(s.ShiftName, '') AS ShiftName,
                CONVERT(VARCHAR(8), s.StartTime, 108) AS StartTime,
                CONVERT(VARCHAR(8), s.EndTime, 108) AS EndTime,
                r.RosterDate,
                r.LocationId,
                ISNULL(l.Name, '') AS LocationName,
                r.Notes,
                r.IsActive,
                r.CreatedBy,
                r.CreatedOn,
                r.CreatedFrom,
                r.UpdatedBy,
                r.UpdatedOn,
                r.UpdatedFrom,
                r.IsSoftDeleted
            FROM Roster AS r
            LEFT JOIN Employees AS e ON e.Id = r.EmployeeId
            LEFT JOIN Shifts AS s ON s.Id = r.ShiftId
            LEFT JOIN Locations AS l ON l.Id = r.LocationId
            WHERE r.IsSoftDeleted = 0
            AND r.Id = {id}";

        List<RosterModel> result = (await dapper.SearchByQuery<RosterModel>(SQL)) ?? new List<RosterModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
        {
            var r = result.FirstOrDefault();
            return (true, r);
        }
    }

    public async Task<(bool, RosterModel, string)> Post(RosterModel obj)
    {
        try
        {
            // Check for duplicate: same employee on same date
            string SQLDuplicate = $@"
                SELECT * FROM Roster 
                WHERE EmployeeId = {obj.EmployeeId} 
                AND CAST(RosterDate AS DATE) = '{obj.RosterDate:yyyy-MM-dd}'
                AND IsSoftDeleted = 0;";

            string locationIdValue = obj.LocationId.HasValue ? obj.LocationId.Value.ToString() : "NULL";
            string SQLInsert = $@"
                INSERT INTO Roster 
                (
                    OrganizationId, 
                    EmployeeId, 
                    ShiftId, 
                    RosterDate, 
                    LocationId, 
                    Notes, 
                    IsActive, 
                    CreatedBy, 
                    CreatedOn, 
                    CreatedFrom, 
                    IsSoftDeleted
                ) 
                VALUES 
                (
                    {obj.OrganizationId},
                    {obj.EmployeeId}, 
                    {obj.ShiftId}, 
                    '{obj.RosterDate:yyyy-MM-dd}', 
                    {locationIdValue}, 
                    '{obj.Notes?.Replace("'", "''") ?? ""}', 
                    {obj.IsActive},
                    {obj.CreatedBy},
                    '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    '{obj.CreatedFrom?.Replace("'", "''").ToUpper() ?? ""}', 
                    0
                );";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<RosterModel> Output = new List<RosterModel>();
                var result = await Search($"r.Id={res.Item2}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                return (false, null!, "Duplicate Record Found. Employee already has a roster entry for this date.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in RosterService.Post");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, RosterModel, string)> Put(RosterModel obj)
    {
        try
        {
            // Check for duplicate: same employee on same date, excluding current record
            string SQLDuplicate = $@"
                SELECT * FROM Roster 
                WHERE EmployeeId = {obj.EmployeeId} 
                AND CAST(RosterDate AS DATE) = '{obj.RosterDate:yyyy-MM-dd}'
                AND Id != {obj.Id}
                AND IsSoftDeleted = 0;";

            string locationIdValue = obj.LocationId.HasValue ? obj.LocationId.Value.ToString() : "NULL";
            string SQLUpdate = $@"
                UPDATE Roster SET 
                    OrganizationId = {obj.OrganizationId}, 
                    EmployeeId = {obj.EmployeeId}, 
                    ShiftId = {obj.ShiftId}, 
                    RosterDate = '{obj.RosterDate:yyyy-MM-dd}', 
                    LocationId = {locationIdValue}, 
                    Notes = '{obj.Notes?.Replace("'", "''") ?? ""}', 
                    IsActive = {obj.IsActive}, 
                    UpdatedBy = {obj.UpdatedBy}, 
                    UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
                    UpdatedFrom = '{obj.UpdatedFrom?.Replace("'", "''").ToUpper() ?? ""}'
                WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<RosterModel> Output = new List<RosterModel>();
                var result = await Search($"r.Id={obj.Id}")!;
                Output = result.Item2;
                return (true, Output.FirstOrDefault()!, "");
            }
            else
            {
                return (false, null!, "Duplicate Record Found. Employee already has a roster entry for this date.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in RosterService.Put");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Roster", id);
    }

    public async Task<(bool, string)> SoftDelete(RosterModel obj)
    {
        string SQLUpdate = $@"
            UPDATE Roster SET 
                UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
                UpdatedBy = {obj.UpdatedBy},
                IsSoftDeleted = 1 
            WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

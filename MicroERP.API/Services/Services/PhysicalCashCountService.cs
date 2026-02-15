using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MicroERP.API.Helper;
using MicroERP.Shared.Models;
using Serilog;

namespace MicroERP.API.Services;

public interface IPhysicalCashCountService
{
    Task<(bool, List<PhysicalCashCountModel>)>? Search(string Criteria = "");
    Task<(bool, List<PhysicalCashCountModel>)>? SearchIndividual(string Criteria = "");
    Task<(bool, PhysicalCashCountModel?)>? Get(int id);
    Task<(bool, PhysicalCashCountModel, string)> Post(PhysicalCashCountModel obj);
    Task<(bool, PhysicalCashCountModel, string)> Put(PhysicalCashCountModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PhysicalCashCountModel obj);
}

public class PhysicalCashCountService : IPhysicalCashCountService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PhysicalCashCountModel>)> Search(string Criteria = "")
    {
        // Group by SessionId to show one entry per cash count session
        // Aggregates all denomination rows into a single session row
        string SQL = $@"
                    SELECT 
                        MIN(pcc.Id) AS Id,
                        MIN(pcc.OrganizationId) AS OrganizationId,
                        pcc.SessionId AS SessionId,
                        MIN(pcc.CountDate) AS CountDate,
                        MIN(pcc.LocationId) AS LocationId,
                        MIN(l.Name) AS LocationName,
                        MIN(pcc.Locker) AS Locker,
                        MIN(pcc.Notes) AS Notes,
                        MAX(pcc.Comments) AS Comments,
                        MIN(pcc.CountedBy) AS CountedBy,
                        MIN(e.Fullname) AS CountedByName,
                        MIN(pcc.VerifiedBy) AS VerifiedBy,
                        MIN(u2.Username) AS VerifiedByName,
                        MIN(pcc.VerifiedOn) AS VerifiedOn,
                        MIN(pcc.Status) AS Status,
                        MIN(pcc.CreatedBy) AS CreatedBy,
                        MIN(pcc.CreatedOn) AS CreatedOn,
                        MIN(pcc.CreatedFrom) AS CreatedFrom,
                        MIN(pcc.UpdatedBy) AS UpdatedBy,
                        MAX(pcc.UpdatedOn) AS UpdatedOn,
                        MIN(pcc.UpdatedFrom) AS UpdatedFrom,
                        MIN(pcc.IsSoftDeleted) AS IsSoftDeleted,
                        SUM(pcc.Amount) AS Amount,
                        0 AS Denomination,
                        0 AS Quantity
                    FROM PhysicalCashCount AS pcc
                    LEFT JOIN Locations AS l ON l.Id = pcc.LocationId
                    LEFT JOIN Employees AS e ON e.Id = pcc.CountedBy
                    LEFT JOIN Users AS u2 ON u2.Id = pcc.VerifiedBy
                    WHERE pcc.IsSoftDeleted = 0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " AND " + Criteria;

        SQL += @"
                GROUP BY pcc.SessionId
                ORDER BY 
                    MIN(pcc.CountDate) DESC,
                    MIN(pcc.LocationId),
                    MIN(pcc.Locker);";

        var result = await dapper.SearchByQuery<PhysicalCashCountModel>(SQL)
                     ?? new List<PhysicalCashCountModel>();

        return result.Count > 0
            ? (true, result)
            : (false, new List<PhysicalCashCountModel>());
    }


    public async Task<(bool, List<PhysicalCashCountModel>)>? SearchIndividual(string Criteria = "")
    {
        // Return individual records without grouping - used for editing/viewing sessions
        string SQL = $@"SELECT 
                        pcc.Id,
                        pcc.OrganizationId,
                        pcc.SessionId,
                        pcc.CountDate,
                        pcc.LocationId,
                        l.Name as LocationName,
                        pcc.Locker,
                        pcc.Notes,
                        pcc.Comments,
                        pcc.CountedBy,
                        e.Fullname as CountedByName,
                        pcc.VerifiedBy,
                        u2.Username as VerifiedByName,
                        pcc.VerifiedOn,
                        pcc.Status,
                        pcc.CreatedBy,
                        pcc.CreatedOn,
                        pcc.CreatedFrom,
                        pcc.UpdatedBy,
                        pcc.UpdatedOn,
                        pcc.UpdatedFrom,
                        pcc.IsSoftDeleted,
                        pcc.Amount,
                        pcc.Denomination,
                        pcc.Quantity
                        FROM PhysicalCashCount as pcc
                        LEFT JOIN Locations as l on l.Id = pcc.LocationId
                        LEFT JOIN Employees as e on e.Id = pcc.CountedBy
                        LEFT JOIN Users as u2 on u2.Id = pcc.VerifiedBy
                        Where pcc.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by pcc.CreatedOn, pcc.Denomination Desc";

        List<PhysicalCashCountModel> result = (await dapper.SearchByQuery<PhysicalCashCountModel>(SQL)) ?? new List<PhysicalCashCountModel>();

        if (result == null || result.Count == 0)
            return (false, new List<PhysicalCashCountModel>());
        else
            return (true, result);
    }

    public async Task<(bool, PhysicalCashCountModel?)>? Get(int id)
    {
        string SQL = $@"SELECT pcc.*, l.Name as LocationName, 
                        e.Fullname as CountedByName, u2.Username as VerifiedByName
                        FROM PhysicalCashCount as pcc
                        LEFT JOIN Locations as l on l.Id = pcc.LocationId
                        LEFT JOIN Employees as e on e.Id = pcc.CountedBy
                        LEFT JOIN Users as u2 on u2.Id = pcc.VerifiedBy
                        Where pcc.Id = {id} AND pcc.IsSoftDeleted=0";

        PhysicalCashCountModel result = (await dapper.SearchByQuery<PhysicalCashCountModel>(SQL))?.FirstOrDefault() ?? new PhysicalCashCountModel();
        
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, PhysicalCashCountModel, string)> Post(PhysicalCashCountModel obj)
    {
        try
        {
            // Calculate amount
            obj.Amount = obj.Denomination * obj.Quantity;

            // Generate SessionId if not provided (for new sessions)
            // If SessionId is provided, use it (for adding more denominations to existing session)
            Guid sessionId = obj.SessionId ?? Guid.NewGuid();
            string sessionIdStr = sessionId.ToString();

            string SQLInsert = $@"INSERT INTO PhysicalCashCount 
            (
                OrganizationId,
                SessionId,
                LocationId,
                Locker,
                CountDate, 
                Denomination, 
                Quantity,
                Amount,
                Notes,
                Comments,
                CountedBy,
                VerifiedBy,
                VerifiedOn,
                Status,
                CreatedBy, 
                CreatedOn, 
                CreatedFrom, 
                IsSoftDeleted
            ) 
            VALUES 
            (
                {obj.OrganizationId},
                '{sessionIdStr}',
                {obj.LocationId},
                '{obj.Locker!.Replace("'", "''")}',
                '{obj.CountDate:yyyy-MM-dd HH:mm:ss}',
                {obj.Denomination},
                {obj.Quantity},
                {obj.Amount},
                '{obj.Notes?.Replace("'", "''") ?? ""}',
                '{obj.Comments?.Replace("'", "''") ?? ""}',
                {obj.CountedBy},
                {(obj.VerifiedBy > 0 ? obj.VerifiedBy.ToString() : "NULL")},
                {(obj.VerifiedOn.HasValue ? $"'{obj.VerifiedOn:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
                '{obj.Status ?? "NOT RECONCILED"}',
                {obj.CreatedBy},
                '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                '{obj.CreatedFrom?.Replace("'", "''") ?? ""}', 
                0
            );";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                var result = await Get(res.Item2)!;
                // Ensure SessionId is set in the returned object
                if (result.Item2 != null && !result.Item2.SessionId.HasValue)
                {
                    result.Item2.SessionId = sessionId;
                }
                return (true, result.Item2!, "");
            }
            else
            {
                string errorMsg = !string.IsNullOrEmpty(res.Item3) 
                    ? $"Failed to create physical cash count for Location ID {obj.LocationId}, Locker '{obj.Locker}', Date {obj.CountDate:yyyy-MM-dd HH:mm}. {res.Item3}" 
                    : $"Failed to create physical cash count for Location ID {obj.LocationId}, Locker '{obj.Locker}', Date {obj.CountDate:yyyy-MM-dd HH:mm}. The record may already exist or a database constraint was violated.";
                return (false, null!, errorMsg);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PhysicalCashCountService.Post Error for Location: {LocationId}, Locker: {Locker}", obj.LocationId, obj.Locker);
            return (false, null!, $"Failed to create physical cash count for Location ID {obj.LocationId}, Locker '{obj.Locker}': {ex.Message}");
        }
    }

    public async Task<(bool, PhysicalCashCountModel, string)> Put(PhysicalCashCountModel obj)
    {
        try
        {
            // Calculate amount
            obj.Amount = obj.Denomination * obj.Quantity;

            // Get existing record to preserve SessionId if not provided
            var existingRecord = await Get(obj.Id);
            Guid sessionIdToUse = obj.SessionId ?? existingRecord.Item2?.SessionId ?? Guid.NewGuid();
            
            string SQLUpdate = $@"UPDATE PhysicalCashCount SET 
                OrganizationId = {obj.OrganizationId},
                SessionId = '{sessionIdToUse}',
                LocationId = {obj.LocationId},
                Locker = '{obj.Locker!.Replace("'", "''")}',
                CountDate = '{obj.CountDate:yyyy-MM-dd HH:mm:ss}',
                Denomination = {obj.Denomination},
                Quantity = {obj.Quantity},
                Amount = {obj.Amount},
                Notes = '{obj.Notes?.Replace("'", "''") ?? ""}',
                Comments = '{obj.Comments?.Replace("'", "''") ?? ""}',
                CountedBy = {obj.CountedBy},
                VerifiedBy = {(obj.VerifiedBy > 0 ? obj.VerifiedBy.ToString() : "NULL")},
                VerifiedOn = {(obj.VerifiedOn.HasValue ? $"'{obj.VerifiedOn:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
                Status = '{obj.Status ?? "NOT RECONCILED"}',
                UpdatedBy = {obj.UpdatedBy}, 
                UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
                UpdatedFrom = '{obj.UpdatedFrom?.Replace("'", "''") ?? ""}'
            WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                var result = await Get(obj.Id)!;
                return (true, result.Item2!, "");
            }
            else
            {
                string errorMsg = !string.IsNullOrEmpty(res.Item2) 
                    ? $"Failed to update physical cash count (ID: {obj.Id}) for Location ID {obj.LocationId}, Locker '{obj.Locker}'. {res.Item2}" 
                    : $"Failed to update physical cash count (ID: {obj.Id}) for Location ID {obj.LocationId}, Locker '{obj.Locker}'. The record may not exist, no changes were made, or a database constraint was violated.";
                return (false, null!, errorMsg);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PhysicalCashCountService.Put Error for ID: {Id}, Location: {LocationId}, Locker: {Locker}", obj.Id, obj.LocationId, obj.Locker);
            return (false, null!, $"Failed to update physical cash count (ID: {obj.Id}) for Location ID {obj.LocationId}, Locker '{obj.Locker}': {ex.Message}");
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("PhysicalCashCount", id);
    }

    public async Task<(bool, string)> SoftDelete(PhysicalCashCountModel obj)
    {
        try
        {
            // First, get the record to get SessionId
            var recordResult = await Get(obj.Id);
            if (!recordResult.Item1 || recordResult.Item2 == null)
            {
                return (false, $"Physical cash count record with ID {obj.Id} not found.");
            }

            var record = recordResult.Item2;
            
            // If SessionId is not available, fall back to old method (for backward compatibility)
            if (!record.SessionId.HasValue)
            {
                // Fallback: Delete by CountDate, LocationId, Locker, CountedBy
                string lockerEscaped = record.Locker?.Replace("'", "''") ?? string.Empty;
                string countDateStr = record.CountDate.HasValue ? record.CountDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
                int countedById = record.CountedBy > 0 ? record.CountedBy : 0;
                
                string SQLUpdateFallback = $@"UPDATE PhysicalCashCount SET 
                    UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
                    UpdatedBy = {obj.UpdatedBy},
                    IsSoftDeleted = 1 
                WHERE CAST(CountDate AS DATETIME) = '{countDateStr}' 
                    AND LocationId = {record.LocationId} 
                    AND Locker = '{lockerEscaped}' 
                    AND CountedBy = {countedById}
                    AND IsSoftDeleted = 0;";

                var fallbackResult = await dapper.Update(SQLUpdateFallback);
                if (fallbackResult.Item1)
                {
                    return (true, "All records in the cash count session have been deleted successfully.");
                }
                else
                {
                    return (false, fallbackResult.Item2 ?? "Failed to delete cash count session records.");
                }
            }
            
            // Delete all records with the same SessionId
            string sessionIdStr = record.SessionId.Value.ToString();
            string SQLUpdate = $@"UPDATE PhysicalCashCount SET 
                UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
                UpdatedBy = {obj.UpdatedBy},
                IsSoftDeleted = 1 
            WHERE SessionId = '{sessionIdStr}' 
                AND IsSoftDeleted = 0;";

            var result = await dapper.Update(SQLUpdate);
            if (result.Item1)
            {
                return (true, "All records in the cash count session have been deleted successfully.");
            }
            else
            {
                return (false, result.Item2 ?? "Failed to delete cash count session records.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PhysicalCashCountService.SoftDelete Error for ID: {Id}", obj.Id);
            return (false, $"Failed to delete physical cash count session: {ex.Message}");
        }
    }
}


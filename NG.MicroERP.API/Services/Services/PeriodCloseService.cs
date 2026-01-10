using Dapper;
using Microsoft.Data.SqlClient;
using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services;

public interface IPeriodCloseService
{
    Task<(bool, List<PeriodCloseModel>)>? Search(string Criteria = "");
    Task<(bool, PeriodCloseModel?)>? Get(int id);
    Task<(bool, PeriodCloseModel, string)> Post(PeriodCloseModel obj);
    Task<(bool, PeriodCloseModel, string)> Put(PeriodCloseModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PeriodCloseModel obj);
    Task<(bool, PeriodCloseModel?)>? GetOpenPeriod(int organizationId, DateTime date, string moduleType = "ALL");
    Task<(bool, string)> ClosePeriod(int id, int userId);
}

public class PeriodCloseService : IPeriodCloseService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PeriodCloseModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT pc.*, u.Username as ClosedByName
                        FROM PeriodClose as pc
                        LEFT JOIN Users as u on u.Id=pc.ClosedBy
                        Where pc.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by pc.StartDate Desc, pc.PeriodName";

        List<PeriodCloseModel> result = (await dapper.SearchByQuery<PeriodCloseModel>(SQL)) ?? new List<PeriodCloseModel>();

        if (result == null || result.Count == 0)
            return (false, new List<PeriodCloseModel>());
        else
            return (true, result);
    }

    public async Task<(bool, PeriodCloseModel?)>? Get(int id)
    {
        string SQL = $@"SELECT pc.*, u.Username as ClosedByName
                       FROM PeriodClose as pc
                       LEFT JOIN Users as u on u.Id=pc.ClosedBy
                       Where pc.Id={id} AND pc.IsSoftDeleted=0";

        PeriodCloseModel result = (await dapper.SearchByQuery<PeriodCloseModel>(SQL))?.FirstOrDefault() ?? new PeriodCloseModel();
        
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, PeriodCloseModel, string)> Post(PeriodCloseModel obj)
    {
        try
        {
            // Normalize ModuleType and Status (uppercase, trimmed, with defaults)
            string moduleTypeValue = string.IsNullOrWhiteSpace(obj.ModuleType) ? "ALL" : obj.ModuleType.ToUpper().Trim();
            string statusValue = string.IsNullOrWhiteSpace(obj.Status) ? "OPEN" : obj.Status.ToUpper().Trim();
            string moduleTypeEscaped = moduleTypeValue.Replace("'", "''");
            string statusValueEscaped = statusValue.Replace("'", "''");

            // Check for overlapping periods (same module type or ALL)
            string SQLOverlap = $@"SELECT * FROM PeriodClose 
                                   WHERE OrganizationId = {obj.OrganizationId}
                                   AND IsSoftDeleted = 0
                                   AND Status IN ('OPEN', 'OPEN_PENDING')
                                   AND (ModuleType = '{moduleTypeEscaped}' OR ModuleType = 'ALL' OR '{moduleTypeEscaped}' = 'ALL')
                                   AND (
                                       (StartDate <= '{obj.StartDate:yyyy-MM-dd}' AND EndDate >= '{obj.StartDate:yyyy-MM-dd}')
                                       OR (StartDate <= '{obj.EndDate:yyyy-MM-dd}' AND EndDate >= '{obj.EndDate:yyyy-MM-dd}')
                                       OR (StartDate >= '{obj.StartDate:yyyy-MM-dd}' AND EndDate <= '{obj.EndDate:yyyy-MM-dd}')
                                   )";

            var overlapCheck = await dapper.SearchByQuery<PeriodCloseModel>(SQLOverlap);
            if (overlapCheck != null && overlapCheck.Any())
            {
                return (false, null!, "Period overlaps with an existing open period.");
            }

            // Check for duplicate period name (same organization, period name, and module type)
            string SQLDuplicate = $@"SELECT * FROM PeriodClose 
                                    WHERE OrganizationId = {obj.OrganizationId}
                                    AND UPPER(PeriodName) = '{obj.PeriodName!.ToUpper().Replace("'", "''")}'
                                    AND ModuleType = '{moduleTypeEscaped}'
                                    AND IsSoftDeleted = 0";

            var duplicateCheck = await dapper.SearchByQuery<PeriodCloseModel>(SQLDuplicate);
            if (duplicateCheck != null && duplicateCheck.Any())
            {
                var existingPeriod = duplicateCheck.First();
                return (false, null!, $"Duplicate period name found. Period '{obj.PeriodName}' with Module Type '{moduleTypeValue}' already exists for this organization. Please use a different period name or module type.");
            }

            string notesValue = string.IsNullOrWhiteSpace(obj.Notes) ? "NULL" : $"'{obj.Notes.ToUpper().Replace("'", "''")}'";
            string createdByValue = (obj.CreatedBy > 0) ? obj.CreatedBy.ToString() : "NULL";
            string createdFromValue = string.IsNullOrWhiteSpace(obj.CreatedFrom) ? "NULL" : $"'{obj.CreatedFrom.ToUpper().Replace("'", "''")}'";

            string SQLInsert = $@"INSERT INTO PeriodClose 
                (
                    OrganizationId, PeriodName, ModuleType, StartDate, EndDate, Status, Notes,
                    CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                ) 
                VALUES 
                (
                    {obj.OrganizationId},
                    '{obj.PeriodName!.ToUpper().Replace("'", "''")}', 
                    '{moduleTypeEscaped}',
                    '{obj.StartDate:yyyy-MM-dd}',
                    '{obj.EndDate:yyyy-MM-dd}',
                    '{statusValueEscaped}',
                    {notesValue},
                    {createdByValue},
                    '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    {createdFromValue}, 
                    {obj.IsSoftDeleted}
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var res = await dapper.Insert(SQLInsert);
            if (!res.Item1)
            {
                string errorMsg = res.Item3 ?? "Failed to save period. Please check the data and try again.";
                return (false, null!, errorMsg);
            }

            int newId = res.Item2;
            var result = await Get(newId);
            return (true, result!.Item2!, "");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PeriodCloseService Post Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, PeriodCloseModel, string)> Put(PeriodCloseModel obj)
    {
        try
        {
            // Normalize ModuleType and Status (uppercase, trimmed, with defaults)
            string moduleTypeValue = string.IsNullOrWhiteSpace(obj.ModuleType) ? "ALL" : obj.ModuleType.ToUpper().Trim();
            string statusValue = string.IsNullOrWhiteSpace(obj.Status) ? "OPEN" : obj.Status.ToUpper().Trim();

            // Check for overlapping periods (excluding current period, same module type or ALL)
            string moduleTypeEscaped = moduleTypeValue.Replace("'", "''");
            string SQLOverlap = $@"SELECT * FROM PeriodClose 
                                   WHERE OrganizationId = {obj.OrganizationId}
                                   AND Id != {obj.Id}
                                   AND IsSoftDeleted = 0
                                   AND Status IN ('OPEN', 'OPEN_PENDING')
                                   AND (ModuleType = '{moduleTypeEscaped}' OR ModuleType = 'ALL' OR '{moduleTypeEscaped}' = 'ALL')
                                   AND (
                                       (StartDate <= '{obj.StartDate:yyyy-MM-dd}' AND EndDate >= '{obj.StartDate:yyyy-MM-dd}')
                                       OR (StartDate <= '{obj.EndDate:yyyy-MM-dd}' AND EndDate >= '{obj.EndDate:yyyy-MM-dd}')
                                       OR (StartDate >= '{obj.StartDate:yyyy-MM-dd}' AND EndDate <= '{obj.EndDate:yyyy-MM-dd}')
                                   )";

            var overlapCheck = await dapper.SearchByQuery<PeriodCloseModel>(SQLOverlap);
            if (overlapCheck != null && overlapCheck.Any())
            {
                return (false, null!, "Period overlaps with an existing open period.");
            }

            // Check for duplicate period name (excluding current period, same organization, period name, and module type)
            string SQLDuplicate = $@"SELECT * FROM PeriodClose 
                                    WHERE OrganizationId = {obj.OrganizationId}
                                    AND Id != {obj.Id}
                                    AND UPPER(PeriodName) = '{obj.PeriodName!.ToUpper().Replace("'", "''")}'
                                    AND ModuleType = '{moduleTypeEscaped}'
                                    AND IsSoftDeleted = 0";

            var duplicateCheck = await dapper.SearchByQuery<PeriodCloseModel>(SQLDuplicate);
            if (duplicateCheck != null && duplicateCheck.Any())
            {
                return (false, null!, $"Duplicate period name found. Period '{obj.PeriodName}' with Module Type '{moduleTypeValue}' already exists for this organization. Please use a different period name or module type.");
            }

            string notesValue = string.IsNullOrWhiteSpace(obj.Notes) ? "NULL" : $"'{obj.Notes.ToUpper().Replace("'", "''")}'";
            string updatedByValue = (obj.UpdatedBy > 0) ? obj.UpdatedBy.ToString() : "NULL";
            string updatedFromValue = string.IsNullOrWhiteSpace(obj.UpdatedFrom) ? "NULL" : $"'{obj.UpdatedFrom.ToUpper().Replace("'", "''")}'";
            string statusValueEscaped = statusValue.Replace("'", "''");

            string SQLUpdate = $@"UPDATE PeriodClose SET 
                    OrganizationId = {obj.OrganizationId}, 
                    PeriodName = '{obj.PeriodName!.ToUpper().Replace("'", "''")}',
                    ModuleType = '{moduleTypeEscaped}',
                    StartDate = '{obj.StartDate:yyyy-MM-dd}',
                    EndDate = '{obj.EndDate:yyyy-MM-dd}',
                    Status = '{statusValueEscaped}',
                    Notes = {notesValue},
                    UpdatedBy = {updatedByValue}, 
                    UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
                    UpdatedFrom = {updatedFromValue}, 
                    IsSoftDeleted = {obj.IsSoftDeleted} 
                WHERE Id = {obj.Id};";

            var updateResult = await dapper.Update(SQLUpdate);
            if (!updateResult.Item1)
            {
                string errorMsg = updateResult.Item2 ?? "Failed to update period. Please check the data and try again.";
                return (false, null!, errorMsg);
            }
            var result = await Get(obj.Id);
            return (true, result!.Item2!, "");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PeriodCloseService Put Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        try
        {
            return await dapper.Delete("PeriodClose", id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PeriodCloseService Delete Error");
            return (false, ex.Message);
        }
    }

    public async Task<(bool, string)> SoftDelete(PeriodCloseModel obj)
    {
        try
        {
            string SQL = $@"UPDATE PeriodClose SET IsSoftDeleted = 1, 
                    UpdatedBy = {obj.UpdatedBy}, 
                    UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
                    UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}' 
                WHERE Id = {obj.Id}";
            await dapper.Update(SQL);
            return (true, "");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PeriodCloseService SoftDelete Error");
            return (false, ex.Message);
        }
    }

    public async Task<(bool, PeriodCloseModel?)>? GetOpenPeriod(int organizationId, DateTime date, string moduleType = "ALL")
    {
        try
        {
            string moduleTypeEscaped = (moduleType ?? "ALL").ToUpper().Trim().Replace("'", "''");
            string dateStr = date.ToString("yyyy-MM-dd");
            
            // Use UPPER() for case-insensitive ModuleType comparison and proper date comparison
            // Compare dates as DATE types for reliable comparison
            string SQL = $@"SELECT pc.*, u.Username as ClosedByName
                            FROM PeriodClose as pc
                            LEFT JOIN Users as u on u.Id=pc.ClosedBy
                            WHERE pc.OrganizationId = {organizationId}
                            AND pc.IsSoftDeleted = 0
                            AND UPPER(LTRIM(RTRIM(ISNULL(pc.Status, '')))) IN ('OPEN', 'OPEN_PENDING')
                            AND (
                                UPPER(LTRIM(RTRIM(ISNULL(pc.ModuleType, '')))) = '{moduleTypeEscaped}' 
                                OR UPPER(LTRIM(RTRIM(ISNULL(pc.ModuleType, '')))) = 'ALL'
                            )
                            AND CAST('{dateStr}' AS DATE) BETWEEN CAST(pc.StartDate AS DATE) AND CAST(pc.EndDate AS DATE)";

            Log.Information("GetOpenPeriod Query: OrganizationId={OrganizationId}, Date={Date}, ModuleType={ModuleType}, SQL={SQL}", 
                organizationId, dateStr, moduleTypeEscaped, SQL);

            PeriodCloseModel result = (await dapper.SearchByQuery<PeriodCloseModel>(SQL))?.FirstOrDefault() ?? new PeriodCloseModel();
            
            if (result == null || result.Id == 0)
            {
                // Debug: Check what periods exist for this organization and date range
                string debugSQL = $@"SELECT pc.Id, pc.OrganizationId, pc.PeriodName, pc.ModuleType, pc.Status, pc.StartDate, pc.EndDate, pc.IsSoftDeleted,
                                    CONVERT(VARCHAR(10), pc.StartDate, 120) as StartDateStr,
                                    CONVERT(VARCHAR(10), pc.EndDate, 120) as EndDateStr
                                    FROM PeriodClose as pc
                                    WHERE pc.OrganizationId = {organizationId}
                                    AND pc.IsSoftDeleted = 0
                                    AND CAST('{dateStr}' AS DATE) BETWEEN CAST(pc.StartDate AS DATE) AND CAST(pc.EndDate AS DATE)";
                
                var debugResults = await dapper.SearchByQuery<PeriodCloseModel>(debugSQL);
                if (debugResults != null && debugResults.Any())
                {
                    foreach (var period in debugResults)
                    {
                        Log.Warning("GetOpenPeriod Debug: Found period matching date range - Id={Id}, ModuleType='{ModuleType}' (len={Len}), Status='{Status}' (len={StatusLen}), StartDate={StartDate}, EndDate={EndDate}", 
                            period.Id, 
                            period.ModuleType, 
                            period.ModuleType?.Length ?? 0,
                            period.Status,
                            period.Status?.Length ?? 0,
                            period.StartDate, 
                            period.EndDate);
                    }
                }
                else
                {
                    Log.Warning("GetOpenPeriod Debug: No periods found matching date range for OrganizationId={OrganizationId}, Date={Date}", organizationId, dateStr);
                }
                
                Log.Warning("GetOpenPeriod: No open period found. OrganizationId={OrganizationId}, Date={Date}, ModuleType={ModuleType}", 
                    organizationId, dateStr, moduleTypeEscaped);
                return (false, null);
            }
            else
            {
                Log.Information("GetOpenPeriod: Found period Id={PeriodId}, ModuleType={ModuleType}, Status={Status}, StartDate={StartDate}, EndDate={EndDate}", 
                    result.Id, result.ModuleType, result.Status, result.StartDate, result.EndDate);
                return (true, result);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetOpenPeriod Error: OrganizationId={OrganizationId}, Date={Date}, ModuleType={ModuleType}", 
                organizationId, date.ToString("yyyy-MM-dd"), moduleType);
            return (false, null);
        }
    }

    public async Task<(bool, string)> ClosePeriod(int id, int userId)
    {
        try
        {
            string SQL = $@"UPDATE PeriodClose SET 
                    Status = 'CLOSE',
                    ClosedDate = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                    ClosedBy = {userId}
                WHERE Id = {id}";
            await dapper.Update(SQL);
            return (true, "");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "PeriodCloseService ClosePeriod Error");
            return (false, ex.Message);
        }
    }
}


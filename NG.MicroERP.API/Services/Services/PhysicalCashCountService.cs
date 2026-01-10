using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;

public interface IPhysicalCashCountService
{
    Task<(bool, List<PhysicalCashCountModel>)>? Search(string Criteria = "");
    Task<(bool, PhysicalCashCountModel?)>? Get(int id);
    Task<(bool, PhysicalCashCountModel, string)> Post(PhysicalCashCountModel obj);
    Task<(bool, PhysicalCashCountModel, string)> Put(PhysicalCashCountModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PhysicalCashCountModel obj);
}

public class PhysicalCashCountService : IPhysicalCashCountService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PhysicalCashCountModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT pcc.*, l.Name as LocationName, 
                        u1.Fullname as CountedByName, u2.Fullname as VerifiedByName
                        FROM PhysicalCashCount as pcc
                        LEFT JOIN Locations as l on l.Id = pcc.LocationId
                        LEFT JOIN Users as u1 on u1.Id = pcc.CountedBy
                        LEFT JOIN Users as u2 on u2.Id = pcc.VerifiedBy
                        Where pcc.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by pcc.CountDate Desc, pcc.LocationId, pcc.Locker, pcc.Denomination Desc";

        List<PhysicalCashCountModel> result = (await dapper.SearchByQuery<PhysicalCashCountModel>(SQL)) ?? new List<PhysicalCashCountModel>();

        if (result == null || result.Count == 0)
            return (false, new List<PhysicalCashCountModel>());
        else
            return (true, result);
    }

    public async Task<(bool, PhysicalCashCountModel?)>? Get(int id)
    {
        string SQL = $@"SELECT pcc.*, l.Name as LocationName, 
                        u1.Fullname as CountedByName, u2.Fullname as VerifiedByName
                        FROM PhysicalCashCount as pcc
                        LEFT JOIN Locations as l on l.Id = pcc.LocationId
                        LEFT JOIN Users as u1 on u1.Id = pcc.CountedBy
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

            string SQLInsert = $@"INSERT INTO PhysicalCashCount 
            (
                OrganizationId, 
                LocationId,
                Locker,
                CountDate, 
                Denomination, 
                Quantity,
                Amount,
                Notes,
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
                {obj.LocationId},
                '{obj.Locker!.Replace("'", "''")}',
                '{obj.CountDate:yyyy-MM-dd}',
                {obj.Denomination},
                {obj.Quantity},
                {obj.Amount},
                '{obj.Notes?.Replace("'", "''") ?? ""}',
                {obj.CountedBy},
                {(obj.VerifiedBy > 0 ? obj.VerifiedBy.ToString() : "NULL")},
                {(obj.VerifiedOn.HasValue ? $"'{obj.VerifiedOn:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
                '{obj.Status ?? "PENDING"}',
                {obj.CreatedBy},
                '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                '{obj.CreatedFrom?.Replace("'", "''") ?? ""}', 
                0
            );";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                var result = await Get(res.Item2)!;
                return (true, result.Item2!, "");
            }
            else
            {
                return (false, null!, "Error inserting record.");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, PhysicalCashCountModel, string)> Put(PhysicalCashCountModel obj)
    {
        try
        {
            // Calculate amount
            obj.Amount = obj.Denomination * obj.Quantity;

            string SQLUpdate = $@"UPDATE PhysicalCashCount SET 
                OrganizationId = {obj.OrganizationId}, 
                LocationId = {obj.LocationId},
                Locker = '{obj.Locker!.Replace("'", "''")}',
                CountDate = '{obj.CountDate:yyyy-MM-dd}',
                Denomination = {obj.Denomination},
                Quantity = {obj.Quantity},
                Amount = {obj.Amount},
                Notes = '{obj.Notes?.Replace("'", "''") ?? ""}',
                CountedBy = {obj.CountedBy},
                VerifiedBy = {(obj.VerifiedBy > 0 ? obj.VerifiedBy.ToString() : "NULL")},
                VerifiedOn = {(obj.VerifiedOn.HasValue ? $"'{obj.VerifiedOn:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
                Status = '{obj.Status ?? "PENDING"}',
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
                return (false, null!, "Error updating record.");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("PhysicalCashCount", id);
    }

    public async Task<(bool, string)> SoftDelete(PhysicalCashCountModel obj)
    {
        string SQLUpdate = $@"UPDATE PhysicalCashCount SET 
            UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
            UpdatedBy = {obj.UpdatedBy},
            IsSoftDeleted = 1 
        WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

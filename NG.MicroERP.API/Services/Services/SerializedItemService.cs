using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;
using Serilog;

namespace NG.MicroERP.API.Services;

public interface ISerializedItemService
{
    Task<(bool, List<SerializedItemModel>)>? Search(string Criteria = "");
    Task<(bool, SerializedItemModel?)>? Get(int id);
    Task<(bool, SerializedItemModel, string)> Post(SerializedItemModel obj);
    Task<(bool, SerializedItemModel, string)> Put(SerializedItemModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(SerializedItemModel obj);
}

public class SerializedItemService : ISerializedItemService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<SerializedItemModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT si.*, i.Code as ItemCode, i.Name as ItemName, l.Name as LocationName, p.Name as PartyName, inv.Code as InvoiceCode
                        FROM SerializedItems as si
                        LEFT JOIN Items as i on i.Id=si.ItemId
                        LEFT JOIN Locations as l on l.Id=si.LocationId
                        LEFT JOIN Parties as p on p.Id=si.PartyId
                        LEFT JOIN Invoice as inv on inv.Id=si.InvoiceId
                        Where si.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by si.SerialNumber, si.ItemId";

        List<SerializedItemModel> result = (await dapper.SearchByQuery<SerializedItemModel>(SQL)) ?? new List<SerializedItemModel>();

        if (result == null || result.Count == 0)
            return (false, new List<SerializedItemModel>());
        else
            return (true, result);
    }

    public async Task<(bool, SerializedItemModel?)>? Get(int id)
    {
        string SQL = $@"SELECT si.*, i.Code as ItemCode, i.Name as ItemName, l.Name as LocationName, p.Name as PartyName, inv.Code as InvoiceCode
                       FROM SerializedItems as si
                       LEFT JOIN Items as i on i.Id=si.ItemId
                       LEFT JOIN Locations as l on l.Id=si.LocationId
                       LEFT JOIN Parties as p on p.Id=si.PartyId
                       LEFT JOIN Invoice as inv on inv.Id=si.InvoiceId
                       Where si.Id={id} AND si.IsSoftDeleted=0";

        SerializedItemModel result = (await dapper.SearchByQuery<SerializedItemModel>(SQL))?.FirstOrDefault() ?? new SerializedItemModel();
        
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, SerializedItemModel, string)> Post(SerializedItemModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM SerializedItems WHERE UPPER(SerialNumber) = '{obj.SerialNumber!.ToUpper()}' AND OrganizationId = {obj.OrganizationId};";
            
            string SQLInsert = $@"INSERT INTO SerializedItems 
			(
				OrganizationId, 
				SerialNumber,
				ItemId,
				LocationId,
				Status,
				PurchaseDate,
				PurchasePrice,
				SaleDate,
				SalePrice,
				PartyId,
				InvoiceId,
				BatchNumber,
				ExpiryDate,
				WarrantyExpiryDate,
				Notes,
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.SerialNumber!.ToUpper().Replace("'", "''")}', 
				{obj.ItemId},
				{(obj.LocationId > 0 ? obj.LocationId.ToString() : "NULL")},
				'{obj.Status?.ToUpper() ?? "AVAILABLE"}',
				{(obj.PurchaseDate.HasValue ? $"'{obj.PurchaseDate.Value:yyyy-MM-dd}'" : "NULL")},
				{obj.PurchasePrice},
				{(obj.SaleDate.HasValue ? $"'{obj.SaleDate.Value:yyyy-MM-dd}'" : "NULL")},
				{obj.SalePrice},
				{(obj.PartyId > 0 ? obj.PartyId.ToString() : "NULL")},
				{(obj.InvoiceId > 0 ? obj.InvoiceId.ToString() : "NULL")},
				'{obj.BatchNumber?.Replace("'", "''") ?? ""}',
				{(obj.ExpiryDate.HasValue ? $"'{obj.ExpiryDate.Value:yyyy-MM-dd}'" : "NULL")},
				{(obj.WarrantyExpiryDate.HasValue ? $"'{obj.WarrantyExpiryDate.Value:yyyy-MM-dd}'" : "NULL")},
				'{obj.Notes?.ToUpper().Replace("'", "''") ?? ""}',
				{obj.CreatedBy},
				'{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                obj.Id = res.Item2;
                return (true, obj, "OK");
            }
            else
                return (false, null!, res.Item3 ?? "Duplicate Serial Number Found.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SerializedItemService Post Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, SerializedItemModel, string)> Put(SerializedItemModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM SerializedItems 
                                    WHERE UPPER(SerialNumber) = '{obj.SerialNumber!.ToUpper()}' 
                                    AND OrganizationId = {obj.OrganizationId}
                                    AND Id != {obj.Id}
                                    AND IsSoftDeleted = 0";

            string SQLUpdate = $@"UPDATE SerializedItems SET 
					SerialNumber = '{obj.SerialNumber!.ToUpper().Replace("'", "''")}',
					ItemId = {obj.ItemId},
					LocationId = {(obj.LocationId > 0 ? obj.LocationId.ToString() : "NULL")},
					Status = '{obj.Status?.ToUpper() ?? "AVAILABLE"}',
					PurchaseDate = {(obj.PurchaseDate.HasValue ? $"'{obj.PurchaseDate.Value:yyyy-MM-dd}'" : "NULL")},
					PurchasePrice = {obj.PurchasePrice},
					SaleDate = {(obj.SaleDate.HasValue ? $"'{obj.SaleDate.Value:yyyy-MM-dd}'" : "NULL")},
					SalePrice = {obj.SalePrice},
					PartyId = {(obj.PartyId > 0 ? obj.PartyId.ToString() : "NULL")},
					InvoiceId = {(obj.InvoiceId > 0 ? obj.InvoiceId.ToString() : "NULL")},
					BatchNumber = '{obj.BatchNumber?.Replace("'", "''") ?? ""}',
					ExpiryDate = {(obj.ExpiryDate.HasValue ? $"'{obj.ExpiryDate.Value:yyyy-MM-dd}'" : "NULL")},
					WarrantyExpiryDate = {(obj.WarrantyExpiryDate.HasValue ? $"'{obj.WarrantyExpiryDate.Value:yyyy-MM-dd}'" : "NULL")},
					Notes = '{obj.Notes?.ToUpper().Replace("'", "''") ?? ""}',
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                return (true, obj, "OK");
            }
            else
                return (false, null!, res.Item2 ?? "Duplicate Serial Number Found.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SerializedItemService Put Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("SerializedItems", id);
    }

    public async Task<(bool, string)> SoftDelete(SerializedItemModel obj)
    {
        string SQLUpdate = $@"UPDATE SerializedItems SET 
					UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
					UpdatedBy = {obj.UpdatedBy},
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}


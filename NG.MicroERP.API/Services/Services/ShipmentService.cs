using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;
using Serilog;

namespace NG.MicroERP.API.Services;

public interface IShipmentService
{
    Task<(bool, List<ShipmentModel>)>? Search(string Criteria = "");
    Task<(bool, ShipmentModel?)>? Get(int id);
    Task<(bool, ShipmentModel, string)> Post(ShipmentModel obj);
    Task<(bool, ShipmentModel, string)> Put(ShipmentModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ShipmentModel obj);
}

public class ShipmentService : IShipmentService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ShipmentModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT s.*, l.Name as LocationName, p.Name as PartyName, c.Name as CourierName
                        FROM Shipments as s
                        LEFT JOIN Locations as l on l.Id=s.LocationId
                        LEFT JOIN Parties as p on p.Id=s.PartyId
                        LEFT JOIN Parties as c on c.Id=s.CourierId
                        Where s.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by s.ShipmentDate Desc, s.ShipmentNo";

        List<ShipmentModel> result = (await dapper.SearchByQuery<ShipmentModel>(SQL)) ?? new List<ShipmentModel>();

        if (result == null || result.Count == 0)
            return (false, new List<ShipmentModel>());
        else
            return (true, result);
    }

    public async Task<(bool, ShipmentModel?)>? Get(int id)
    {
        // Get Header
        string SQLHeader = $@"SELECT s.*, l.Name as LocationName, p.Name as PartyName, c.Name as CourierName
                             FROM Shipments as s
                             LEFT JOIN Locations as l on l.Id=s.LocationId
                             LEFT JOIN Parties as p on p.Id=s.PartyId
                             LEFT JOIN Parties as c on c.Id=s.CourierId
                             Where s.Id={id} AND s.IsSoftDeleted=0";

        ShipmentModel header = (await dapper.SearchByQuery<ShipmentModel>(SQLHeader))?.FirstOrDefault() ?? new ShipmentModel();
        
        if (header == null || header.Id == 0)
            return (false, null);

        // Get Details
        string SQLDetails = $@"SELECT sd.*, i.Code as ItemCode, i.Name as ItemName
                               FROM ShipmentDetails as sd
                               LEFT JOIN Items as i on i.Id=sd.ItemId
                               Where sd.ShipmentId={id} AND sd.IsSoftDeleted=0
                               Order by sd.SeqNo, sd.Id";

        List<ShipmentDetailModel> details = (await dapper.SearchByQuery<ShipmentDetailModel>(SQLDetails)) ?? new List<ShipmentDetailModel>();
        header.Details = new ObservableCollection<ShipmentDetailModel>(details);

        return (true, header);
    }

    public async Task<(bool, ShipmentModel, string)> Post(ShipmentModel obj)
    {
        try
        {
            string shipmentNo = dapper.GetCode("SH", "Shipments", "ShipmentNo")!;
            string SQLDuplicate = $@"SELECT * FROM Shipments WHERE UPPER(ShipmentNo) = '{shipmentNo.ToUpper()}' AND OrganizationId = {obj.OrganizationId};";
            
            // Insert Header
            string SQLInsertHeader = $@"INSERT INTO Shipments 
			(
				OrganizationId, 
				ShipmentNo,
				ShipmentType,
				LocationId,
				PartyId,
				ReferenceNo,
				ReferenceType,
				ReferenceId,
				ShipmentDate,
				ExpectedDate,
				Status,
				TotalItems,
				TotalQuantity,
				TotalValue,
				CourierId,
				TrackingNumber,
				Notes,
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{shipmentNo.ToUpper()}', 
				'{obj.ShipmentType?.ToUpper() ?? "INCOMING"}',
				{(obj.LocationId > 0 ? obj.LocationId.ToString() : "NULL")},
				{(obj.PartyId > 0 ? obj.PartyId.ToString() : "NULL")},
				'{(obj.ReferenceNo != null ? obj.ReferenceNo.ToUpper().Replace("'", "''") : "")}',
				'{(obj.ReferenceType != null ? obj.ReferenceType.ToUpper().Replace("'", "''") : "")}',
				{obj.ReferenceId},
				{(obj.ShipmentDate.HasValue ? $"'{obj.ShipmentDate.Value:yyyy-MM-dd}'" : "NULL")},
				{(obj.ExpectedDate.HasValue ? $"'{obj.ExpectedDate.Value:yyyy-MM-dd}'" : "NULL")},
				'{obj.Status?.ToUpper() ?? "PENDING"}',
				{obj.TotalItems},
				{obj.TotalQuantity},
				{obj.TotalValue},
				{(obj.CourierId > 0 ? obj.CourierId.ToString() : "NULL")},
				'{(obj.TrackingNumber != null ? obj.TrackingNumber.Replace("'", "''") : "")}',
				'{(obj.Notes != null ? obj.Notes.ToUpper().Replace("'", "''") : "")}',
				{obj.CreatedBy},
				'{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
				'{(obj.CreatedFrom != null ? obj.CreatedFrom.ToUpper() : "")}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsertHeader, SQLDuplicate);
            if (res.Item1 == true)
            {
                int headerId = res.Item2;
                obj.Id = headerId;
                obj.ShipmentNo = shipmentNo;

                // Insert Details
                int seqNo = 1;
                foreach (var detail in obj.Details ?? new ObservableCollection<ShipmentDetailModel>())
                {
                    string SQLInsertDetail = $@"INSERT INTO ShipmentDetails 
                    (
                        ShipmentId, ItemId, StockCondition, Quantity, ReceivedQuantity, UnitPrice, TotalPrice,
                        BatchNumber, ExpiryDate, Description, SeqNo,
                        CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                    ) 
                    VALUES 
                    (
                        {headerId}, {detail.ItemId}, '{detail.StockCondition?.ToUpper() ?? "NEW"}',
                        {detail.Quantity}, {detail.ReceivedQuantity}, {detail.UnitPrice}, {detail.TotalPrice},
                        '{(detail.BatchNumber != null ? detail.BatchNumber.Replace("'", "''") : "")}',
                        {(detail.ExpiryDate.HasValue ? $"'{detail.ExpiryDate.Value:yyyy-MM-dd}'" : "NULL")},
                        '{(detail.Description != null ? detail.Description.ToUpper().Replace("'", "''") : "")}', {seqNo++},
                        {obj.CreatedBy}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                        '{(obj.CreatedFrom != null ? obj.CreatedFrom.ToUpper() : "")}', {detail.IsSoftDeleted}
                    );";
                    await dapper.Insert(SQLInsertDetail);
                }

                return (true, obj, "OK");
            }
            else
                return (false, null!, res.Item3 ?? "Duplicate Shipment No Found.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ShipmentService Post Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, ShipmentModel, string)> Put(ShipmentModel obj)
    {
        try
        {
            // Update Header
            string SQLUpdateHeader = $@"UPDATE Shipments SET 
					ShipmentType = '{obj.ShipmentType?.ToUpper() ?? "INCOMING"}',
					LocationId = {(obj.LocationId > 0 ? obj.LocationId.ToString() : "NULL")},
					PartyId = {(obj.PartyId > 0 ? obj.PartyId.ToString() : "NULL")},
					ReferenceNo = '{(obj.ReferenceNo != null ? obj.ReferenceNo.ToUpper().Replace("'", "''") : "")}',
					ReferenceType = '{(obj.ReferenceType != null ? obj.ReferenceType.ToUpper().Replace("'", "''") : "")}',
					ReferenceId = {obj.ReferenceId},
					ShipmentDate = {(obj.ShipmentDate.HasValue ? $"'{obj.ShipmentDate.Value:yyyy-MM-dd}'" : "NULL")},
					ExpectedDate = {(obj.ExpectedDate.HasValue ? $"'{obj.ExpectedDate.Value:yyyy-MM-dd}'" : "NULL")},
					ReceivedDate = {(obj.ReceivedDate.HasValue ? $"'{obj.ReceivedDate.Value:yyyy-MM-dd}'" : "NULL")},
					Status = '{obj.Status?.ToUpper() ?? "PENDING"}',
					TotalItems = {obj.TotalItems},
					TotalQuantity = {obj.TotalQuantity},
					TotalValue = {obj.TotalValue},
					CourierId = {(obj.CourierId > 0 ? obj.CourierId.ToString() : "NULL")},
					TrackingNumber = '{(obj.TrackingNumber != null ? obj.TrackingNumber.Replace("'", "''") : "")}',
					Notes = '{(obj.Notes != null ? obj.Notes.ToUpper().Replace("'", "''") : "")}',
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
					UpdatedFrom = '{(obj.UpdatedFrom != null ? obj.UpdatedFrom.ToUpper() : "")}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdateHeader);
            if (res.Item1 == true)
            {
                // Delete existing details
                string SQLDeleteDetails = $@"UPDATE ShipmentDetails SET IsSoftDeleted = 1 WHERE ShipmentId = {obj.Id};";
                await dapper.Update(SQLDeleteDetails);

                // Insert new details
                int seqNo = 1;
                foreach (var detail in obj.Details ?? new ObservableCollection<ShipmentDetailModel>())
                {
                    string SQLInsertDetail = $@"INSERT INTO ShipmentDetails 
                    (
                        ShipmentId, ItemId, StockCondition, Quantity, ReceivedQuantity, UnitPrice, TotalPrice,
                        BatchNumber, ExpiryDate, Description, SeqNo,
                        CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                    ) 
                    VALUES 
                    (
                        {obj.Id}, {detail.ItemId}, '{detail.StockCondition?.ToUpper() ?? "NEW"}',
                        {detail.Quantity}, {detail.ReceivedQuantity}, {detail.UnitPrice}, {detail.TotalPrice},
                        '{detail.BatchNumber?.Replace("'", "''") ?? ""}',
                        {(detail.ExpiryDate.HasValue ? $"'{detail.ExpiryDate.Value:yyyy-MM-dd}'" : "NULL")},
                        '{detail.Description?.ToUpper().Replace("'", "''") ?? ""}', {seqNo++},
                        {obj.CreatedBy}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                        '{obj.CreatedFrom!.ToUpper()}', {detail.IsSoftDeleted}
                    );";
                    await dapper.Insert(SQLInsertDetail);
                }

                return (true, obj, "OK");
            }
            else
                return (false, null!, res.Item2);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ShipmentService Put Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Shipments", id);
    }

    public async Task<(bool, string)> SoftDelete(ShipmentModel obj)
    {
        string SQLUpdate = $@"UPDATE Shipments SET 
					UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
					UpdatedBy = {obj.UpdatedBy},
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}


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

public interface IStockMovementService
{
    Task<(bool, List<StockMovementModel>)>? Search(string Criteria = "");
    Task<(bool, StockMovementModel?)>? Get(int id);
    Task<(bool, StockMovementModel, string)> Post(StockMovementModel obj);
    Task<(bool, StockMovementModel, string)> Put(StockMovementModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(StockMovementModel obj);
}

public class StockMovementService : IStockMovementService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<StockMovementModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT sm.*, ISNULL(sm.DocumentType, '') as DocumentType, fl.Name as FromLocationName, tl.Name as ToLocationName, u.Fullname as ApprovedByName
                        FROM StockMovements as sm
                        LEFT JOIN Locations as fl on fl.Id=sm.FromLocationId
                        LEFT JOIN Locations as tl on tl.Id=sm.ToLocationId
                        LEFT JOIN Users as u on u.Id=sm.ApprovedBy
                        Where sm.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by sm.MovementDate Desc, sm.MovementNo";

        List<StockMovementModel> result = (await dapper.SearchByQuery<StockMovementModel>(SQL)) ?? new List<StockMovementModel>();

        if (result == null || result.Count == 0)
            return (false, new List<StockMovementModel>());
        else
            return (true, result);
    }

    public async Task<(bool, StockMovementModel?)>? Get(int id)
    {
        // Get Header
        string SQLHeader = $@"SELECT sm.*, ISNULL(sm.DocumentType, '') as DocumentType, fl.Name as FromLocationName, tl.Name as ToLocationName, u.Fullname as ApprovedByName
                             FROM StockMovements as sm
                             LEFT JOIN Locations as fl on fl.Id=sm.FromLocationId
                             LEFT JOIN Locations as tl on tl.Id=sm.ToLocationId
                             LEFT JOIN Users as u on u.Id=sm.ApprovedBy
                             Where sm.Id={id} AND sm.IsSoftDeleted=0";

        StockMovementModel header = (await dapper.SearchByQuery<StockMovementModel>(SQLHeader))?.FirstOrDefault() ?? new StockMovementModel();
        
        if (header == null || header.Id == 0)
            return (false, null);

        // Get Details
        string SQLDetails = $@"SELECT smd.*, i.Code as ItemCode, i.Name as ItemName
                               FROM StockMovementDetails as smd
                               LEFT JOIN Items as i on i.Id=smd.ItemId
                               Where smd.MovementId={id} AND smd.IsSoftDeleted=0
                               Order by smd.SeqNo, smd.Id";

        List<StockMovementDetailModel> details = (await dapper.SearchByQuery<StockMovementDetailModel>(SQLDetails)) ?? new List<StockMovementDetailModel>();
        header.Details = new ObservableCollection<StockMovementDetailModel>(details);

        return (true, header);
    }

    public async Task<(bool, StockMovementModel, string)> Post(StockMovementModel obj)
    {
        try
        {
            // Auto-determine DocumentType from MovementType if not provided
            if (string.IsNullOrWhiteSpace(obj.DocumentType))
            {
                obj.DocumentType = obj.MovementType?.ToUpper() switch
                {
                    "TRANSFER" => "STN",  // Stock Transfer Note
                    "ADJUSTMENT" => "ADJ", // Adjustment
                    "RETURN" => "RTN",     // Return
                    "DAMAGE" => "DMG",     // Damage
                    "LOSS" => "LSS",       // Loss
                    _ => "MOV"             // Default Movement
                };
            }

            // Determine document prefix based on DocumentType
            string docPrefix = obj.DocumentType?.ToUpper() switch
            {
                "GRN" => "GRN",  // Goods Receipt Note
                "SIR" => "SIR",  // Stock Issue Requisition
                "STN" => "STN",  // Stock Transfer Note
                "ADJ" => "ADJ",  // Adjustment
                "RTN" => "RTN",  // Return
                "DMG" => "DMG",  // Damage
                "LSS" => "LSS",  // Loss
                _ => "MOV"       // Default Movement
            };

            // Generate document number based on document type
            string movementNo = dapper.GetCode(docPrefix, "StockMovements", "MovementNo")!;
            string SQLDuplicate = $@"SELECT * FROM StockMovements WHERE UPPER(MovementNo) = '{movementNo.ToUpper()}' AND OrganizationId = {obj.OrganizationId};";
            
            // Insert Header
            string SQLInsertHeader = $@"INSERT INTO StockMovements 
			(
				OrganizationId, 
				MovementNo,
				DocumentType,
				MovementType,
				FromLocationId,
				ToLocationId,
				MovementDate,
				ReferenceNo,
				ReferenceType,
				ReferenceId,
				Status,
				Reason,
				ApprovedBy,
				ApprovedDate,
				Notes,
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{movementNo.ToUpper()}', 
				'{obj.DocumentType?.ToUpper() ?? docPrefix}',
				'{obj.MovementType?.ToUpper() ?? "TRANSFER"}',
				{(obj.FromLocationId > 0 ? obj.FromLocationId.ToString() : "NULL")},
				{(obj.ToLocationId > 0 ? obj.ToLocationId.ToString() : "NULL")},
				'{obj.MovementDate:yyyy-MM-dd}',
				'{obj.ReferenceNo?.ToUpper().Replace("'", "''") ?? ""}',
				'{obj.ReferenceType?.ToUpper().Replace("'", "''") ?? ""}',
				{obj.ReferenceId},
				'{obj.Status?.ToUpper() ?? "PENDING"}',
				'{obj.Reason?.ToUpper().Replace("'", "''") ?? ""}',
				{(obj.ApprovedBy > 0 ? obj.ApprovedBy.ToString() : "NULL")},
				{(obj.ApprovedDate.HasValue ? $"'{obj.ApprovedDate.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
				'{obj.Notes?.ToUpper().Replace("'", "''") ?? ""}',
				{obj.CreatedBy},
				'{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsertHeader, SQLDuplicate);
            if (res.Item1 == true)
            {
                int headerId = res.Item2;
                obj.Id = headerId;
                obj.MovementNo = movementNo;

                // Insert Details
                int seqNo = 1;
                foreach (var detail in obj.Details ?? new ObservableCollection<StockMovementDetailModel>())
                {
                    string SQLInsertDetail = $@"INSERT INTO StockMovementDetails 
                    (
                        MovementId, ItemId, StockCondition, Quantity, UnitCost, TotalCost,
                        BatchNumber, ExpiryDate, Description, SeqNo,
                        CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                    ) 
                    VALUES 
                    (
                        {headerId}, {detail.ItemId}, '{detail.StockCondition?.ToUpper() ?? "NEW"}',
                        {detail.Quantity}, {detail.UnitCost}, {detail.TotalCost},
                        '{detail.BatchNumber?.Replace("'", "''") ?? ""}',
                        {(detail.ExpiryDate.HasValue ? $"'{detail.ExpiryDate.Value:yyyy-MM-dd}'" : "NULL")},
                        '{detail.Description?.ToUpper().Replace("'", "''") ?? ""}', {seqNo++},
                        {obj.CreatedBy}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                        '{obj.CreatedFrom!.ToUpper()}', {detail.IsSoftDeleted}
                    );";
                    await dapper.Insert(SQLInsertDetail);
                }

                // Update inventory if movement is completed
                if (obj.Status?.ToUpper() == "COMPLETED")
                {
                    // Check stock availability before completing
                    if (obj.FromLocationId > 0 && obj.Details != null)
                    {
                        var itemsToCheck = obj.Details.Select(d => (
                            d.ItemId,
                            d.Quantity,
                            d.StockCondition ?? "NEW"
                        )).ToList();

                        var stockCheck = await Helper.InventoryHelper.CheckStockAvailability(dapper, obj.OrganizationId, obj.FromLocationId, itemsToCheck);
                        if (!stockCheck.Item1)
                        {
                            // Rollback by soft deleting the movement
                            await dapper.ExecuteQuery($"UPDATE StockMovements SET IsSoftDeleted = 1 WHERE Id = {headerId}");
                            return (false, null!, stockCheck.Item2);
                        }
                    }

                    await Helper.InventoryHelper.UpdateInventoryFromStockMovement(dapper, headerId, obj.OrganizationId, obj.FromLocationId, obj.ToLocationId);
                }

                return (true, obj, "OK");
            }
            else
                return (false, null!, res.Item3 ?? "Duplicate Movement No Found.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "StockMovementService Post Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, StockMovementModel, string)> Put(StockMovementModel obj)
    {
        try
        {
            // Validate period for stock movement
            if (obj.MovementDate.HasValue)
            {
                var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.OrganizationId, obj.MovementDate.Value, "STOCK");
                if (!periodCheck.Item1)
                {
                    return (false, null!, periodCheck.Item2);
                }
            }

            // Get old status before update
            var existing = await dapper.SearchByQuery<StockMovementModel>($"SELECT * FROM StockMovements WHERE Id = {obj.Id}");
            string oldStatus = existing?.FirstOrDefault()?.Status ?? "";

            // Auto-determine DocumentType from MovementType if not provided
            if (string.IsNullOrWhiteSpace(obj.DocumentType))
            {
                obj.DocumentType = obj.MovementType?.ToUpper() switch
                {
                    "TRANSFER" => "STN",  // Stock Transfer Note
                    "ADJUSTMENT" => "ADJ", // Adjustment
                    "RETURN" => "RTN",     // Return
                    "DAMAGE" => "DMG",     // Damage
                    "LOSS" => "LSS",       // Loss
                    _ => "MOV"             // Default Movement
                };
            }

            // Update Header
            string SQLUpdateHeader = $@"UPDATE StockMovements SET 
					DocumentType = '{obj.DocumentType?.ToUpper() ?? "MOV"}',
					MovementType = '{obj.MovementType?.ToUpper() ?? "TRANSFER"}',
					FromLocationId = {(obj.FromLocationId > 0 ? obj.FromLocationId.ToString() : "NULL")},
					ToLocationId = {(obj.ToLocationId > 0 ? obj.ToLocationId.ToString() : "NULL")},
					MovementDate = '{obj.MovementDate:yyyy-MM-dd}',
					ReferenceNo = '{obj.ReferenceNo?.ToUpper().Replace("'", "''") ?? ""}',
					ReferenceType = '{obj.ReferenceType?.ToUpper().Replace("'", "''") ?? ""}',
					ReferenceId = {obj.ReferenceId},
					Status = '{obj.Status?.ToUpper() ?? "PENDING"}',
					Reason = '{obj.Reason?.ToUpper().Replace("'", "''") ?? ""}',
					ApprovedBy = {(obj.ApprovedBy > 0 ? obj.ApprovedBy.ToString() : "NULL")},
					ApprovedDate = {(obj.ApprovedDate.HasValue ? $"'{obj.ApprovedDate.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")},
					Notes = '{obj.Notes?.ToUpper().Replace("'", "''") ?? ""}',
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdateHeader);
            if (res.Item1 == true)
            {
                // Delete existing details
                string SQLDeleteDetails = $@"UPDATE StockMovementDetails SET IsSoftDeleted = 1 WHERE MovementId = {obj.Id};";
                await dapper.Update(SQLDeleteDetails);

                // Insert new details
                int seqNo = 1;
                foreach (var detail in obj.Details ?? new ObservableCollection<StockMovementDetailModel>())
                {
                    string SQLInsertDetail = $@"INSERT INTO StockMovementDetails 
                    (
                        MovementId, ItemId, StockCondition, Quantity, UnitCost, TotalCost,
                        BatchNumber, ExpiryDate, Description, SeqNo,
                        CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                    ) 
                    VALUES 
                    (
                        {obj.Id}, {detail.ItemId}, '{detail.StockCondition?.ToUpper() ?? "NEW"}',
                        {detail.Quantity}, {detail.UnitCost}, {detail.TotalCost},
                        '{detail.BatchNumber?.Replace("'", "''") ?? ""}',
                        {(detail.ExpiryDate.HasValue ? $"'{detail.ExpiryDate.Value:yyyy-MM-dd}'" : "NULL")},
                        '{detail.Description?.ToUpper().Replace("'", "''") ?? ""}', {seqNo++},
                        {obj.CreatedBy}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                        '{obj.CreatedFrom!.ToUpper()}', {detail.IsSoftDeleted}
                    );";
                    await dapper.Insert(SQLInsertDetail);
                }

                // If status changed to COMPLETED, check stock and update inventory
                if (oldStatus?.ToUpper() != "COMPLETED" && obj.Status?.ToUpper() == "COMPLETED")
                {
                    // Check stock availability before completing
                    if (obj.FromLocationId > 0 && obj.Details != null)
                    {
                        var itemsToCheck = obj.Details.Select(d => (
                            d.ItemId,
                            d.Quantity,
                            d.StockCondition ?? "NEW"
                        )).ToList();

                        var stockCheck = await Helper.InventoryHelper.CheckStockAvailability(dapper, obj.OrganizationId, obj.FromLocationId, itemsToCheck);
                        if (!stockCheck.Item1)
                        {
                            return (false, null!, stockCheck.Item2);
                        }
                    }

                    await Helper.InventoryHelper.UpdateInventoryFromStockMovement(dapper, obj.Id, obj.OrganizationId, obj.FromLocationId, obj.ToLocationId);
                }

                return (true, obj, "OK");
            }
            else
                return (false, null!, res.Item2);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "StockMovementService Put Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("StockMovements", id);
    }

    public async Task<(bool, string)> SoftDelete(StockMovementModel obj)
    {
        string SQLUpdate = $@"UPDATE StockMovements SET 
					UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
					UpdatedBy = {obj.UpdatedBy},
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}


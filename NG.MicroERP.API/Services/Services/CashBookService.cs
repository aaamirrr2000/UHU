using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;


public interface ICashBookService
{
    Task<(bool, List<CashBookModel>)>? Search(string Criteria = "");
    Task<(bool, CashBookModel?)>? Get(int id);
    Task<(bool, CashBookReportModel?)>? GetCashBookReport(int id);
    Task<(bool, CashBookModel, string)> Post(CashBookModel obj);
    Task<(bool, CashBookModel, string)> Put(CashBookModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(CashBookModel obj);
}


public class CashBookService : ICashBookService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<CashBookModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT 'CASH BOOK' as Source, a.*, b.Name as LocationName FROM Cashbook as a
                        LEFT JOIN Locations as b on b.id=a.LocationId
                        Where a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<CashBookModel> result = (await dapper.SearchByQuery<CashBookModel>(SQL)) ?? new List<CashBookModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, CashBookModel?)>? Get(int id)
    {
        CashBookModel result = (await dapper.SearchByID<CashBookModel>("Cashbook", id)) ?? new CashBookModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, CashBookReportModel?)>? GetCashBookReport(int id)
    {
        CashBookReportModel result = (await dapper.SearchByID<CashBookReportModel>("CashBookReport", id)) ?? new CashBookReportModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, CashBookModel, string)> Post(CashBookModel obj)
    {
        try
        {
            // Validate period for cashbook creation
            if (obj.TranDate.HasValue)
            {
                var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.OrganizationId, obj.TranDate.Value, "CASHBOOK");
                if (!periodCheck.Item1)
                {
                    return (false, null!, periodCheck.Item2);
                }
            }

            string Code = dapper.GetCode("CBP", "Cashbook", "SeqNo")!;
            string SQLDuplicate = $@"SELECT * FROM Cashbook WHERE UPPER(SeqNo) = '{obj.SeqNo!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Cashbook 
			(
				OrganizationId, 
				SeqNo,
                FileAttachment,
				LocationId, 
				PartyId, 
				TranDate, 
				Description, 
				Amount, 
				AccountId, 
				TranType, 
				PaymentMethod, 
				RefNo, 
				TranRef, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{Code!}', 
                '{obj.FileAttachment}',
				{obj.LocationId},
				{obj.PartyId},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.Description!.ToUpper()}', 
				{obj.Amount},
				{obj.AccountId},
				'{obj.TranType!.ToUpper()}', 
				'{obj.PaymentMethod!.ToUpper()}', 
				'{obj.RefNo!.ToUpper()}', 
				'{obj.TranRef!.ToUpper()}', 
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<CashBookModel> Output = new List<CashBookModel>();
                var result = await Search($"a.id={res.Item2}")!;
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

    public async Task<(bool, CashBookModel, string)> Put(CashBookModel obj)
    {
        try
        {
            // Check if cashbook is posted to GL - prevent updates
            var existingCashBook = await dapper.SearchByQuery<CashBookModel>($"SELECT * FROM Cashbook WHERE Id = {obj.Id}");
            if (existingCashBook != null && existingCashBook.Any() && existingCashBook.First().IsPostedToGL == 1)
            {
                return (false, null!, "Cannot update cashbook entry that is posted to General Ledger.");
            }

            // Validate period for cashbook update
            if (obj.TranDate.HasValue)
            {
                var periodCheck = await new GeneralLedgerService().ValidatePeriod(obj.OrganizationId, obj.TranDate.Value, "CASHBOOK");
                if (!periodCheck.Item1)
                {
                    return (false, null!, periodCheck.Item2);
                }
            }

            string SQLDuplicate = $@"SELECT * FROM Cashbook WHERE UPPER(SeqNo) = '{obj.SeqNo!.ToUpper()}' and ID != {obj.Id};";
            string SQLUpdate = $@"UPDATE Cashbook SET 
					OrganizationId = {obj.OrganizationId}, 
					SeqNo = '{obj.SeqNo!.ToUpper()}', 
                    FileAttachment = '{obj.FileAttachment}', 
					LocationId = {obj.LocationId}, 
					PartyId = {obj.PartyId}, 
					TranDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					Description = '{obj.Description!.ToUpper()}', 
					Amount = {obj.Amount}, 
					AccountId = {obj.AccountId}, 
					TranType = '{obj.TranType!.ToUpper()}', 
					PaymentMethod = '{obj.PaymentMethod!.ToUpper()}', 
					RefNo = '{obj.RefNo!.ToUpper()}', 
					TranRef = '{obj.TranRef!.ToUpper()}',  
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<CashBookModel> Output = new List<CashBookModel>();
                var result = await Search($"a.id={obj.Id}")!;
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
        // Check if cashbook is posted to GL - prevent deletion
        var cashBook = await dapper.SearchByQuery<CashBookModel>($"SELECT * FROM Cashbook WHERE Id = {id}");
        if (cashBook != null && cashBook.Any() && cashBook.First().IsPostedToGL == 1)
        {
            return (false, "Cannot delete cashbook entry that is posted to General Ledger.");
        }
        return await dapper.Delete("Cashbook", id);
    }

    public async Task<(bool, string)> SoftDelete(CashBookModel obj)
    {
        // Check if cashbook is posted to GL - prevent deletion
        var cashBook = await dapper.SearchByQuery<CashBookModel>($"SELECT * FROM Cashbook WHERE Id = {obj.Id}");
        if (cashBook != null && cashBook.Any() && cashBook.First().IsPostedToGL == 1)
        {
            return (false, "Cannot delete cashbook entry that is posted to General Ledger.");
        }
        string SQLUpdate = $@"UPDATE Cashbook SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

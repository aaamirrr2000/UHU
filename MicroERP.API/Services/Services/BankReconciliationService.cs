using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroERP.API.Helper;
using MicroERP.Shared.Models;
using Serilog;

namespace MicroERP.API.Services;

public interface IBankReconciliationService
{
    Task<(bool, List<BankReconciliationModel>)>? Search(string Criteria = "");
    Task<(bool, BankReconciliationModel?)>? Get(int id);
    Task<(bool, BankReconciliationModel, string)> Post(BankReconciliationModel obj);
    Task<(bool, BankReconciliationModel, string)> Put(BankReconciliationModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(BankReconciliationModel obj);
    Task<(bool, List<BankReconciliationDetailModel>)>? GetBookTransactions(int bankAccountId, DateTime statementDate);
}

public class BankReconciliationService : IBankReconciliationService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<BankReconciliationModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT br.*, coa.Name as BankAccountName, coa.Code as BankAccountCode
                        FROM BankReconciliation as br
                        LEFT JOIN ChartOfAccounts as coa on coa.Id=br.BankAccountId
                        Where br.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by br.StatementDate Desc, br.ReconciliationNo Desc";

        List<BankReconciliationModel> result = (await dapper.SearchByQuery<BankReconciliationModel>(SQL)) ?? new List<BankReconciliationModel>();

        if (result == null || result.Count == 0)
            return (false, new List<BankReconciliationModel>());
        else
            return (true, result);
    }

    public async Task<(bool, BankReconciliationModel?)>? Get(int id)
    {
        // Get Header
        string SQLHeader = $@"SELECT br.*, coa.Name as BankAccountName, coa.Code as BankAccountCode
                             FROM BankReconciliation as br
                             LEFT JOIN ChartOfAccounts as coa on coa.Id=br.BankAccountId
                             Where br.Id={id} AND br.IsSoftDeleted=0";

        BankReconciliationModel header = (await dapper.SearchByQuery<BankReconciliationModel>(SQLHeader))?.FirstOrDefault() ?? new BankReconciliationModel();
        
        if (header == null || header.Id == 0)
            return (false, null);

        // Get Details
        string SQLDetails = $@"SELECT brd.*
                               FROM BankReconciliationDetails as brd
                               Where brd.ReconciliationId={id} AND brd.IsSoftDeleted=0
                               Order by brd.TransactionDate, brd.Id";

        List<BankReconciliationDetailModel> details = (await dapper.SearchByQuery<BankReconciliationDetailModel>(SQLDetails)) ?? new List<BankReconciliationDetailModel>();
        header.Details = details;

        return (true, header);
    }

    public async Task<(bool, List<BankReconciliationDetailModel>)>? GetBookTransactions(int bankAccountId, DateTime statementDate)
    {
        try
        {
            // Get transactions from CashBook where PaymentMethod matches the bank account
            string cashBookSQL = $@"SELECT 
                                    cb.Id as MatchedTransactionId,
                                    'CASHBOOK' as MatchedTransactionType,
                                    cb.TranDate as TransactionDate,
                                    cb.SeqNo as ReferenceNo,
                                    cb.Description,
                                    CASE 
                                        WHEN cb.TranType = 'RECEIPT' THEN cb.Amount
                                        WHEN cb.TranType = 'PAYMENT' THEN -cb.Amount
                                        WHEN cb.TranType = 'BANK DEPOSIT' THEN -cb.Amount
                                        ELSE 0
                                    END as Amount,
                                    cb.TranType as TransactionType,
                                    'BOOK' as Source
                                    FROM Cashbook cb
                                    WHERE cb.AccountId = {bankAccountId}
                                    AND cb.TranDate <= '{statementDate:yyyy-MM-dd}'
                                    AND cb.IsSoftDeleted = 0
                                    AND cb.IsPostedToGL = 1";

            // Get transactions from General Ledger for the bank account
            string glSQL = $@"SELECT 
                             gld.Id as MatchedTransactionId,
                             'GENERALLEDGER' as MatchedTransactionType,
                             glh.EntryDate as TransactionDate,
                             glh.EntryNo as ReferenceNo,
                             gld.Description,
                             (gld.DebitAmount - gld.CreditAmount) as Amount,
                             glh.ReferenceType as TransactionType,
                             'BOOK' as Source
                             FROM GeneralLedgerDetail gld
                             INNER JOIN GeneralLedgerHeader glh ON glh.Id = gld.HeaderId
                             WHERE gld.AccountId = {bankAccountId}
                             AND glh.EntryDate <= '{statementDate:yyyy-MM-dd}'
                             AND glh.IsSoftDeleted = 0
                             AND glh.IsPosted = 1";

            var cashBookTransactions = await dapper.SearchByQuery<BankReconciliationDetailModel>(cashBookSQL) ?? new List<BankReconciliationDetailModel>();
            var glTransactions = await dapper.SearchByQuery<BankReconciliationDetailModel>(glSQL) ?? new List<BankReconciliationDetailModel>();

            var allTransactions = cashBookTransactions.Concat(glTransactions).OrderBy(t => t.TransactionDate).ToList();

            return (true, allTransactions);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting book transactions for bank reconciliation");
            return (false, new List<BankReconciliationDetailModel>());
        }
    }

    public async Task<(bool, BankReconciliationModel, string)> Post(BankReconciliationModel obj)
    {
        try
        {
            string reconciliationNo = dapper.GetCode("BR", "BankReconciliation", "ReconciliationNo")!;
            string SQLDuplicate = $@"SELECT * FROM BankReconciliation WHERE UPPER(ReconciliationNo) = '{reconciliationNo.ToUpper()}' AND OrganizationId = {obj.OrganizationId} AND IsSoftDeleted = 0;";
            
            // Calculate difference
            obj.Difference = obj.StatementBalance - obj.BookBalance;

            // Insert Header
            string SQLInsertHeader = $@"INSERT INTO BankReconciliation 
			(
				OrganizationId, 
				ReconciliationNo,
				BankAccountId,
				StatementDate,
				OpeningBalance,
				StatementBalance,
				BookBalance,
				Difference,
				Status,
				Notes,
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{reconciliationNo.ToUpper()}', 
				{obj.BankAccountId},
				'{obj.StatementDate:yyyy-MM-dd}',
				{obj.OpeningBalance},
				{obj.StatementBalance},
				{obj.BookBalance},
				{obj.Difference},
				'{obj.Status?.ToUpper() ?? "OPEN"}',
				'{obj.Notes?.Replace("'", "''") ?? ""}',
				{obj.CreatedBy},
				'{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var res = await dapper.Insert(SQLInsertHeader, SQLDuplicate);
            if (res.Item1 == true)
            {
                int headerId = res.Item2;
                obj.Id = headerId;
                obj.ReconciliationNo = reconciliationNo;

                // Insert Details
                if (obj.Details != null && obj.Details.Any())
                {
                    foreach (var detail in obj.Details)
                    {
                        string SQLInsertDetail = $@"INSERT INTO BankReconciliationDetails 
                        (
                            ReconciliationId, TransactionType, TransactionDate, ReferenceNo, Description,
                            Amount, IsMatched, MatchedTransactionId, MatchedTransactionType, Source,
                            CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                        ) 
                        VALUES 
                        (
                            {headerId}, 
                            '{detail.TransactionType?.ToUpper() ?? "DEPOSIT"}',
                            '{detail.TransactionDate:yyyy-MM-dd}',
                            '{detail.ReferenceNo?.Replace("'", "''") ?? ""}',
                            '{detail.Description?.Replace("'", "''") ?? ""}',
                            {detail.Amount},
                            {detail.IsMatched},
                            {(detail.MatchedTransactionId > 0 ? detail.MatchedTransactionId.ToString() : "NULL")},
                            '{detail.MatchedTransactionType?.ToUpper() ?? ""}',
                            '{detail.Source?.ToUpper() ?? "STATEMENT"}',
                            {obj.CreatedBy}, 
                            '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                            '{obj.CreatedFrom!.ToUpper()}', 
                            {detail.IsSoftDeleted}
                        );";
                        await dapper.Insert(SQLInsertDetail);
                    }
                }

                return (true, obj, "OK");
            }
            else
                return (false, null!, res.Item3 ?? "Failed to save bank reconciliation.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BankReconciliationService Post Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, BankReconciliationModel, string)> Put(BankReconciliationModel obj)
    {
        try
        {
            // Calculate difference
            obj.Difference = obj.StatementBalance - obj.BookBalance;

            // Update Header
            string SQLUpdateHeader = $@"UPDATE BankReconciliation SET 
					BankAccountId = {obj.BankAccountId},
					StatementDate = '{obj.StatementDate:yyyy-MM-dd}',
					OpeningBalance = {obj.OpeningBalance},
					StatementBalance = {obj.StatementBalance},
					BookBalance = {obj.BookBalance},
					Difference = {obj.Difference},
					Status = '{obj.Status?.ToUpper() ?? "OPEN"}',
					Notes = '{obj.Notes?.Replace("'", "''") ?? ""}',
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdateHeader);
            if (res.Item1 == true)
            {
                // Delete existing details
                string SQLDeleteDetails = $@"UPDATE BankReconciliationDetails SET IsSoftDeleted = 1 WHERE ReconciliationId = {obj.Id};";
                await dapper.Update(SQLDeleteDetails);

                // Insert new details
                if (obj.Details != null && obj.Details.Any())
                {
                    foreach (var detail in obj.Details)
                    {
                        string SQLInsertDetail = $@"INSERT INTO BankReconciliationDetails 
                        (
                            ReconciliationId, TransactionType, TransactionDate, ReferenceNo, Description,
                            Amount, IsMatched, MatchedTransactionId, MatchedTransactionType, Source,
                            CreatedBy, CreatedOn, CreatedFrom, IsSoftDeleted
                        ) 
                        VALUES 
                        (
                            {obj.Id}, 
                            '{detail.TransactionType?.ToUpper() ?? "DEPOSIT"}',
                            '{detail.TransactionDate:yyyy-MM-dd}',
                            '{detail.ReferenceNo?.Replace("'", "''") ?? ""}',
                            '{detail.Description?.Replace("'", "''") ?? ""}',
                            {detail.Amount},
                            {detail.IsMatched},
                            {(detail.MatchedTransactionId > 0 ? detail.MatchedTransactionId.ToString() : "NULL")},
                            '{detail.MatchedTransactionType?.ToUpper() ?? ""}',
                            '{detail.Source?.ToUpper() ?? "STATEMENT"}',
                            {obj.CreatedBy}, 
                            '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                            '{obj.CreatedFrom!.ToUpper()}', 
                            {detail.IsSoftDeleted}
                        );";
                        await dapper.Insert(SQLInsertDetail);
                    }
                }

                return (true, obj, "OK");
            }
            else
                return (false, null!, res.Item2 ?? "Failed to update bank reconciliation.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BankReconciliationService Put Error");
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("BankReconciliation", id);
    }

    public async Task<(bool, string)> SoftDelete(BankReconciliationModel obj)
    {
        string SQLUpdate = $@"UPDATE BankReconciliation SET 
					UpdatedOn = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
					UpdatedBy = {obj.UpdatedBy},
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}


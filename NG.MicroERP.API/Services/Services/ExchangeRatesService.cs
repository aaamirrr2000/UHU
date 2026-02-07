using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;

public interface IExchangeRatesService
{
    Task<(bool, List<ExchangeRatesModel>)>? Search(string Criteria = "");
    Task<(bool, ExchangeRatesModel?)>? Get(int id);
    Task<(bool, ExchangeRatesModel, string)> Post(ExchangeRatesModel obj);
    Task<(bool, ExchangeRatesModel, string)> Put(ExchangeRatesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ExchangeRatesModel obj);
}

public class ExchangeRatesService : IExchangeRatesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ExchangeRatesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT 
            er.*,
            bc.Code AS BaseCurrencyCode,
            bc.Name AS BaseCurrencyName,
            tc.Code AS TargetCurrencyCode,
            tc.Name AS TargetCurrencyName
            FROM ExchangeRates er
            LEFT JOIN Currencies bc ON er.BaseCurrencyId = bc.Id
            LEFT JOIN Currencies tc ON er.TargetCurrencyId = tc.Id
            WHERE er.IsSoftDeleted = 0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by er.Id Desc";

        List<ExchangeRatesModel> result = (await dapper.SearchByQuery<ExchangeRatesModel>(SQL)) ?? new List<ExchangeRatesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, ExchangeRatesModel?)>? Get(int id)
    {
        string SQL = $@"SELECT 
            er.*,
            bc.Code AS BaseCurrencyCode,
            bc.Name AS BaseCurrencyName,
            tc.Code AS TargetCurrencyCode,
            tc.Name AS TargetCurrencyName
            FROM ExchangeRates er
            LEFT JOIN Currencies bc ON er.BaseCurrencyId = bc.Id
            LEFT JOIN Currencies tc ON er.TargetCurrencyId = tc.Id
            WHERE er.Id = {id} AND er.IsSoftDeleted = 0";

        List<ExchangeRatesModel> result = (await dapper.SearchByQuery<ExchangeRatesModel>(SQL)) ?? new List<ExchangeRatesModel>();
        
        if (result == null || result.Count == 0)
            return (false, null);
        else
            return (true, result.FirstOrDefault());
    }

    public async Task<(bool, ExchangeRatesModel, string)> Post(ExchangeRatesModel obj)
    {
        try
        {
            // Check for overlapping rates
            string endDateStr = obj.EndDate.HasValue ? obj.EndDate.Value.ToString("yyyy-MM-dd") : DateTime.MaxValue.ToString("yyyy-MM-dd");
            string SQLDuplicate = $@"SELECT * FROM ExchangeRates 
                WHERE BaseCurrencyId = {obj.BaseCurrencyId} 
                AND TargetCurrencyId = {obj.TargetCurrencyId}
                AND IsSoftDeleted = 0
                AND (
                    (StartDate <= '{obj.StartDate:yyyy-MM-dd}' AND (EndDate IS NULL OR EndDate >= '{obj.StartDate:yyyy-MM-dd}'))
                    OR (StartDate <= '{endDateStr}' AND (EndDate IS NULL OR EndDate >= '{endDateStr}'))
                    OR ('{obj.StartDate:yyyy-MM-dd}' <= StartDate AND ('{endDateStr}' >= StartDate))
                )";

            var duplicateCheck = await dapper.SearchByQuery<ExchangeRatesModel>(SQLDuplicate);
            if (duplicateCheck != null && duplicateCheck.Any())
            {
                return (false, null!, "Overlapping exchange rate period found for this currency pair.");
            }

            string endDateValue = obj.EndDate.HasValue ? $"'{obj.EndDate.Value:yyyy-MM-dd}'" : "NULL";

            string SQLInsert = $@"INSERT INTO ExchangeRates 
            (
                BaseCurrencyId, 
                TargetCurrencyId, 
                Rate, 
                StartDate, 
                EndDate, 
                Source, 
                Remarks, 
                CreatedBy, 
                CreatedOn, 
                CreatedFrom, 
                IsSoftDeleted
            ) 
            VALUES 
            (
                {obj.BaseCurrencyId}, 
                {obj.TargetCurrencyId}, 
                {obj.Rate}, 
                '{obj.StartDate:yyyy-MM-dd}', 
                {endDateValue}, 
                '{obj.Source?.Replace("'", "''") ?? ""}', 
                '{obj.Remarks?.Replace("'", "''") ?? ""}', 
                {obj.CreatedBy}, 
                '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                '{obj.CreatedFrom?.Replace("'", "''") ?? ""}', 
                {obj.IsSoftDeleted}
            );";

            var res = await dapper.Insert(SQLInsert, "");
            if (res.Item1 == true)
            {
                List<ExchangeRatesModel> Output = new List<ExchangeRatesModel>();
                var result = await Get(res.Item2)!;
                if (result.Item1 && result.Item2 != null)
                {
                    return (true, result.Item2, "");
                }
                else
                {
                    return (false, null!, "Record created but could not be retrieved.");
                }
            }
            else
            {
                return (false, null!, res.Item3 ?? "Failed to insert record.");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, ExchangeRatesModel, string)> Put(ExchangeRatesModel obj)
    {
        try
        {
            // Check for overlapping rates (excluding current record)
            string endDateStr = obj.EndDate.HasValue ? obj.EndDate.Value.ToString("yyyy-MM-dd") : DateTime.MaxValue.ToString("yyyy-MM-dd");
            string SQLDuplicate = $@"SELECT * FROM ExchangeRates 
                WHERE BaseCurrencyId = {obj.BaseCurrencyId} 
                AND TargetCurrencyId = {obj.TargetCurrencyId}
                AND Id != {obj.Id}
                AND IsSoftDeleted = 0
                AND (
                    (StartDate <= '{obj.StartDate:yyyy-MM-dd}' AND (EndDate IS NULL OR EndDate >= '{obj.StartDate:yyyy-MM-dd}'))
                    OR (StartDate <= '{endDateStr}' AND (EndDate IS NULL OR EndDate >= '{endDateStr}'))
                    OR ('{obj.StartDate:yyyy-MM-dd}' <= StartDate AND ('{endDateStr}' >= StartDate))
                )";

            var duplicateCheck = await dapper.SearchByQuery<ExchangeRatesModel>(SQLDuplicate);
            if (duplicateCheck != null && duplicateCheck.Any())
            {
                return (false, null!, "Overlapping exchange rate period found for this currency pair.");
            }

            string endDateValue = obj.EndDate.HasValue ? $"'{obj.EndDate.Value:yyyy-MM-dd}'" : "NULL";
            string updatedOnValue = obj.UpdatedOn.HasValue ? $"'{obj.UpdatedOn.Value:yyyy-MM-dd HH:mm:ss}'" : $"'{DateTime.Now:yyyy-MM-dd HH:mm:ss}'";

            string SQLUpdate = $@"UPDATE ExchangeRates SET 
                BaseCurrencyId = {obj.BaseCurrencyId}, 
                TargetCurrencyId = {obj.TargetCurrencyId}, 
                Rate = {obj.Rate}, 
                StartDate = '{obj.StartDate:yyyy-MM-dd}', 
                EndDate = {endDateValue}, 
                Source = '{obj.Source?.Replace("'", "''") ?? ""}', 
                Remarks = '{obj.Remarks?.Replace("'", "''") ?? ""}', 
                UpdatedBy = {obj.UpdatedBy}, 
                UpdatedOn = {updatedOnValue}, 
                UpdatedFrom = '{obj.UpdatedFrom?.Replace("'", "''") ?? ""}', 
                IsSoftDeleted = {obj.IsSoftDeleted} 
            WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, "");
            if (res.Item1 == true)
            {
                var result = await Get(obj.Id)!;
                if (result.Item1 && result.Item2 != null)
                {
                    return (true, result.Item2, "");
                }
                else
                {
                    return (false, null!, "Record updated but could not be retrieved.");
                }
            }
            else
            {
                return (false, null!, res.Item2 ?? "Failed to update record.");
            }
        }
        catch (Exception ex)
        {
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("ExchangeRates", id);
    }

    public async Task<(bool, string)> SoftDelete(ExchangeRatesModel obj)
    {
        string SQLUpdate = $@"UPDATE ExchangeRates SET 
            UpdatedOn = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 
            UpdatedBy = {obj.UpdatedBy},
            IsSoftDeleted = 1 
        WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

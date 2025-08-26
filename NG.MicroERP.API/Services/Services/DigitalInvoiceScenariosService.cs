using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services.Services;


public interface IDigitalInvoiceScenariosService
{
    Task<(bool, List<DigitalInvoiceScenariosModel>)>? Search(string Criteria = "");
    Task<(bool, DigitalInvoiceScenariosModel?)>? Get(int id);
    Task<(bool, DigitalInvoiceScenariosModel, string)> Post(DigitalInvoiceScenariosModel obj);
    Task<(bool, string)> Put(DigitalInvoiceScenariosModel obj);
    Task<(bool, string)> Delete(int id);
}


public class DigitalInvoiceScenariosService : IDigitalInvoiceScenariosService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<DigitalInvoiceScenariosModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM DigitalInvoiceScenarios";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " Where " + Criteria;

        SQL += " Order by Id Desc";

        List<DigitalInvoiceScenariosModel> result = (await dapper.SearchByQuery<DigitalInvoiceScenariosModel>(SQL)) ?? new List<DigitalInvoiceScenariosModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, DigitalInvoiceScenariosModel?)>? Get(int id)
    {
        DigitalInvoiceScenariosModel result = (await dapper.SearchByID<DigitalInvoiceScenariosModel>("DigitalInvoiceScenarios", id)) ?? new DigitalInvoiceScenariosModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, DigitalInvoiceScenariosModel, string)> Post(DigitalInvoiceScenariosModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO DigitalInvoiceScenarios 
			(
				ScenarioID, 
				SaleType, 
				BuyerType, 
				TaxContext
			) 
			VALUES 
			(
				'{obj.ScenarioID!.ToUpper()}', 
				'{obj.SaleType!.ToUpper()}', 
				'{obj.BuyerType!.ToUpper()}', 
				'{obj.TaxContext!.ToUpper()}',
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<DigitalInvoiceScenariosModel> Output = new List<DigitalInvoiceScenariosModel>();
                var result = await Search($"id={res.Item2}")!;
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
            return (true, null!, ex.Message);
        }
    }

    public async Task<(bool, string)> Put(DigitalInvoiceScenariosModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE DigitalInvoiceScenarios SET 
					ScenarioID = '{obj.ScenarioID!.ToUpper()}', 
					SaleType = '{obj.SaleType!.ToUpper()}', 
					BuyerType = '{obj.BuyerType!.ToUpper()}', 
					TaxContext = '{obj.TaxContext!.ToUpper()}' 
				WHERE Id = {obj.Id};";

            return await dapper.Update(SQLUpdate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("DigitalInvoiceScenarios", id);
    }
}



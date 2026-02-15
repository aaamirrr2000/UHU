using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;


public interface ITaxCalculationService
{
    Task<(bool, List<ItemPartyTaxCalculationModel>)>? Search(string Criteria = "");
}


public class TaxCalculationService : ITaxCalculationService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<ItemPartyTaxCalculationModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM vw_ItemPartyTaxCalculation";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " Where " + Criteria;

        List<ItemPartyTaxCalculationModel> result = (await dapper.SearchByQuery<ItemPartyTaxCalculationModel>(SQL)) ?? new List<ItemPartyTaxCalculationModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

}




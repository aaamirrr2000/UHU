using NG.MicroERP.API.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.API.Services;

public interface IDineinOrderStatusService
{
    Task<(bool, List<DineinOrderStatusModel>)>? Search(string Criteria = "");
}


public class DineinOrderStatusService : IDineinOrderStatusService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<DineinOrderStatusModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM DineInOrderStatus";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " Where " + Criteria;

        SQL += " Order by Id Desc";

        List<DineinOrderStatusModel> result = (await dapper.SearchByQuery<DineinOrderStatusModel>(SQL)) ?? new List<DineinOrderStatusModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }
}
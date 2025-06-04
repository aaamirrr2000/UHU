using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;

public interface IChartOfAccountsService
{
    Task<(bool, List<ChartOfAccountsModel>)>? Search(string Criteria = "");
    Task<(bool, ChartOfAccountsModel?)>? Get(int id);
    Task<(bool, ChartOfAccountsModel, string)> Post(ChartOfAccountsModel obj);
    Task<(bool, string)> Put(ChartOfAccountsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(ChartOfAccountsModel obj);
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;


public interface IStockReceivingService
{
    Task<(bool, List<StockReceivingModel>)>? Search(string Criteria = "");
    Task<(bool, StockReceivingModel?)>? Get(int id);
    Task<(bool, StockReceivingModel, string)> Post(StockReceivingModel obj);
    Task<(bool, string)> Put(StockReceivingModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(StockReceivingModel obj);
}
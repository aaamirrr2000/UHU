using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;


public interface ILeavesService
{
    Task<(bool, List<LeavesModel>)>? Search(string Criteria = "");
    Task<(bool, LeavesModel?)>? Get(int id);
    Task<(bool, LeavesModel, string)> Post(LeavesModel obj);
    Task<(bool, string)> Put(LeavesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(LeavesModel obj);
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;

public interface IMyMenuService
{
    Task<(bool, List<MyMenuModel>)>? Search(string Criteria = "");
    Task<(bool, MyMenuModel?)>? Get(int id);
    Task<(bool, MyMenuModel, string)> Post(MyMenuModel obj);
    Task<(bool, string)> Put(MyMenuModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(MyMenuModel obj);
}

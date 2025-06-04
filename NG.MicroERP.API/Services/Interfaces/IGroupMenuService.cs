using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.API.Services;

public interface IGroupMenuService
{
    Task<(bool, List<GroupMenuModel>)>? Search(string Criteria = "");
    Task<(bool, GroupMenuModel?)>? Get(int id);
    Task<(bool, List<GroupMenuModel>)>? SearchGroupMenu(string Criteria = "");
}

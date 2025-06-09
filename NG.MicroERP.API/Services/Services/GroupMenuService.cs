using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

namespace NG.MicroERP.API.Services;


public interface IGroupMenuService
{
    Task<(bool, List<GroupMenuModel>)>? Search(string Criteria = "");
    Task<(bool, GroupMenuModel?)>? Get(int id);
    Task<(bool, List<GroupMenuModel>)>? SearchGroupMenu(string Criteria = "");
}

public class GroupMenuService : IGroupMenuService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<GroupMenuModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM ViewGroupMenu";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " Where " + Criteria;

        SQL += " Order by MenuId";

        List<GroupMenuModel> result = (await dapper.SearchByQuery<GroupMenuModel>(SQL)) ?? new List<GroupMenuModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, GroupMenuModel?)>? Get(int id)
    {
        GroupMenuModel result = (await dapper.SearchByID<GroupMenuModel>("GroupMenu", id)) ?? new GroupMenuModel();
        if (result == null || result.GroupId == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, List<GroupMenuModel>)>? SearchGroupMenu(string Criteria = "")
    {
        string SQL = "Select * from ViewGroupMenu ";

        if (Debugger.IsAttached == true)
        {
            SQL += "where Live>=0";
        }
        else
        {
            SQL += "where Live=1";
        }

        if (Criteria.Length > 0)
        {
            SQL += " and " + Criteria;
        }

        SQL += " Order by SeqNo, ParentId, MenuId";

        List<GroupMenuModel> result = (await dapper.SearchByQuery<GroupMenuModel>(SQL)) ?? [];
        return (true, result);
    }

}

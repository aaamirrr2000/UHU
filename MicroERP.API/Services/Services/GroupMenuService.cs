using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroERP.Shared.Helper;
using MicroERP.Shared.Models;
using MicroERP.API.Services;
using MicroERP.API.Helper;

namespace MicroERP.API.Services;


public interface IGroupMenuService
{
    Task<(bool, List<GroupMenuModel>)>? Search(string Criteria = "");
    Task<(bool, GroupMenuModel?)>? Get(int id);
    Task<(bool, List<GroupMenuModel>)>? SearchGroupMenu(string Criteria = "");
    Task<(bool, string)> AccessLevel(int userId, string pageName = "");
}

public class GroupMenuService : IGroupMenuService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<GroupMenuModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM vw_GroupMenu";

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
        GroupMenuModel result = (await dapper.SearchByID<GroupMenuModel>("vw_GroupMenu", id)) ?? new GroupMenuModel();
        if (result == null || result.GroupId == 0)
            return (false, null);
        else
            return (true, result);
    }

    public async Task<(bool, List<GroupMenuModel>)>? SearchGroupMenu(string Criteria = "")
    {
        string SQL = "Select * from vw_GroupMenu ";

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

    public async Task<(bool, string)> AccessLevel(int userId, string pageName = "")
    {
        // 1. Get User Group
        string userSql = $"SELECT * FROM Users WHERE Id = {userId}";
        var user = (await dapper.SearchByQuery<UsersModel>(userSql))?.FirstOrDefault();

        if (user == null)
            return (false, "NO ACCESS");

        if (user.GroupId == 1)
            return (true, "FULL ACCESS");

        // 2. Get Access Level
        string accessSql = $"SELECT * FROM vw_GroupMenu WHERE GroupId = {user.GroupId} AND lower(PageName) = '{pageName.ToLower()}'";
        var accessLevel = (await dapper.SearchByQuery<GroupMenuModel>(accessSql))?.FirstOrDefault();

        if (accessLevel == null)
            return (false, "NO ACCESS");
        else
            return (true, accessLevel.My_Privilege ?? "NO ACCESS");
    }

}


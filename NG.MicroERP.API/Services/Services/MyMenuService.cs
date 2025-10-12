using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Models;
using NG.MicroERP.API.Services;
using NG.MicroERP.API.Helper;

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


public class MyMenuService : IMyMenuService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<MyMenuModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM Menu Where IsSoftDeleted=0 and IsActive=1";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<MyMenuModel> result = (await dapper.SearchByQuery<MyMenuModel>(SQL)) ?? new List<MyMenuModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, MyMenuModel?)>? Get(int id)
    {
        MyMenuModel result = (await dapper.SearchByID<MyMenuModel>("Menu", id)) ?? new MyMenuModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, MyMenuModel, string)> Post(MyMenuModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO Menu 
			(
				MenuCaption, 
				AdditionalInfo, 
				Tooltip, 
				PageName, 
				ParentId, 
				Icon, 
				SeqNo, 
				Live, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				'{obj.MenuCaption!.ToUpper()}', 
				'{obj.AdditionalInfo!.ToUpper()}', 
				'{obj.Tooltip!.ToUpper()}', 
				'{obj.PageName!.ToUpper()}', 
				{obj.ParentId},
				'{obj.Icon!.ToUpper()}', 
				{obj.SeqNo},
				{obj.Live},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<MyMenuModel> Output = new List<MyMenuModel>();
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

    public async Task<(bool, string)> Put(MyMenuModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE Menu SET 
					MenuCaption = '{obj.MenuCaption!.ToUpper()}', 
					AdditionalInfo = '{obj.AdditionalInfo!.ToUpper()}', 
					Tooltip = '{obj.Tooltip!.ToUpper()}', 
					PageName = '{obj.PageName!.ToUpper()}', 
					ParentId = {obj.ParentId}, 
					Icon = '{obj.Icon!.ToUpper()}', 
					SeqNo = {obj.SeqNo}, 
					Live = {obj.Live}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
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
        return await dapper.Delete("Menu", id);
    }

    public async Task<(bool, string)> SoftDelete(MyMenuModel obj)
    {
        string SQLUpdate = $@"UPDATE Menu SET 
					UpdatedOn = '{DateTime.UtcNow}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

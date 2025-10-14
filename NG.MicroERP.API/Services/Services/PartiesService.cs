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


public interface IPartiesService
{
    Task<(bool, List<PartiesModel>)>? Search(string Criteria = "");
    Task<(bool, PartiesModel?)>? Get(int id);
    Task<(bool, PartiesModel, string)> Post(PartiesModel obj);
    Task<(bool, string)> Put(PartiesModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(PartiesModel obj);
}


public class PartiesService : IPartiesService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<PartiesModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT a.*, b.Name as ParentName, c.AreaName as City, d.Name as Account FROM Parties as a
                        left join Parties as b on b.id=a.ParentId
                        left join Areas as c on c.id=a.CityId
                        left join ChartOfAccounts as d on d.id=a.AccountId
                        Where a.IsSoftDeleted=0";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " and " + Criteria;

        SQL += " Order by Id Desc";

        List<PartiesModel> result = (await dapper.SearchByQuery<PartiesModel>(SQL)) ?? new List<PartiesModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, PartiesModel?)>? Get(int id)
    {
        PartiesModel result = (await dapper.SearchByID<PartiesModel>("Parties", id)) ?? new PartiesModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, PartiesModel, string)> Post(PartiesModel obj)
    {

        try
        {

            string Code = dapper.GetCode("", "Parties", "Code")!;
            //string SQLDuplicate = $@"SELECT * FROM Parties WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Parties 
			(
				OrganizationId, 
				Code, 
				Pic, 
				Name, 
				PartyType, 
				PartyTypeCode, 
				ParentId, 
				Address, 
				CityId,
                AccountId,
				Latitude, 
				Longitude, 
				Radius, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted 
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{Code}', 
				'{obj.Pic!}', 
				'{obj.Name!.ToUpper()}', 
				'{obj.PartyType!.ToUpper()}', 
				'{obj.PartyTypeCode!.ToUpper()}', 
				{obj.ParentId},
				'{obj.Address!.ToUpper()}', 
				{obj.CityId},
                {obj.AccountId},
				'{obj.Latitude!.ToUpper()}', 
				'{obj.Longitude!.ToUpper()}', 
				{obj.Radius},
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<PartiesModel> Output = new List<PartiesModel>();
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

    public async Task<(bool, string)> Put(PartiesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Parties WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE Parties SET 
					OrganizationId = {obj.OrganizationId}, 
					Code = '{obj.Code!.ToUpper()}', 
					Pic = '{obj.Pic!.ToUpper()}', 
					Name = '{obj.Name!.ToUpper()}', 
					PartyType = '{obj.PartyType!.ToUpper()}', 
					PartyTypeCode = '{obj.PartyTypeCode!.ToUpper()}', 
					ParentId = {obj.ParentId}, 
					Address = '{obj.Address!.ToUpper()}', 
					CityId = {obj.CityId}, 
                    AccountId = {obj.AccountId}, 
					Latitude = '{obj.Latitude!.ToUpper()}', 
					Longitude = '{obj.Longitude!.ToUpper()}', 
					Radius = {obj.Radius}, 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted}
				WHERE Id = {obj.Id};";

            return await dapper.Update(SQLUpdate, SQLDuplicate);
        }
        catch (Exception ex)
        {
            return (true, ex.Message);
        }
    }

    public async Task<(bool, string)> Delete(int id)
    {
        return await dapper.Delete("Parties", id);
    }

    public async Task<(bool, string)> SoftDelete(PartiesModel obj)
    {
        string SQLUpdate = $@"UPDATE Parties SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}

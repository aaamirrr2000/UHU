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
    Task<(bool, PartiesModel, string)> Put(PartiesModel obj);
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

        SQL += " Order by a.Id Desc";

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
            string SQLDuplicate = $@"SELECT * FROM Parties WHERE UPPER(code) = '{obj.Code!.ToUpper()}';";
            string SQLInsert = $@"INSERT INTO Parties 
			(
				OrganizationId, 
				Code, 
				Name, 
				PartyType, 
				ParentId, 
				CustomerRating, 
				CustomerClass, 
				CustomerSince, 
				SalesPersonId, 
				CreditLimit, 
				PaymentTermsId, 
				AccountId, 
				NTN,
				STN, 
				Address, 
				CityId, 
				Latitude, 
				Longitude, 
				Radius, 
				ContactPerson, 
				ContactDesignation, 
				ContactEmail, 
				Pic, 
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{Code}', 
				'{obj.Name!.ToUpper()}', 
				'{obj.PartyType!.ToUpper()}', 
				{obj.ParentId},
				{obj.CustomerRating},
				'{obj.CustomerClass!.ToUpper()}', 
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				{obj.SalesPersonId},
				{obj.CreditLimit},
				{obj.PaymentTermsId},
				{obj.AccountId},
				'{obj.NTN!.ToUpper()}',
				'{obj.STN!.ToUpper()}',
				'{obj.Address!.ToUpper()}', 
				{obj.CityId},
				'{obj.Latitude!.ToUpper()}', 
				'{obj.Longitude!.ToUpper()}', 
				{obj.Radius},
				'{obj.ContactPerson!.ToUpper()}', 
				'{obj.ContactDesignation!.ToUpper()}', 
				'{obj.ContactEmail!.ToUpper()}', 
				'{obj.Pic!.ToUpper()}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}'
			);";

            var res = await dapper.Insert(SQLInsert, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<PartiesModel> Output = new List<PartiesModel>();
                var result = await Search($"a.id={res.Item2}")!;
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
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, PartiesModel, string)> Put(PartiesModel obj)
    {
        try
        {
            string SQLDuplicate = $@"SELECT * FROM Parties WHERE UPPER(code) = '{obj.Code!.ToUpper()}' and Id != {obj.Id};";
            string SQLUpdate = $@"UPDATE Parties SET 
					OrganizationId = {obj.OrganizationId}, 
					Code = '{obj.Code!.ToUpper()}', 
					Name = '{obj.Name!.ToUpper()}', 
					PartyType = '{obj.PartyType!.ToUpper()}', 
					ParentId = {obj.ParentId}, 
					CustomerRating = {obj.CustomerRating}, 
					CustomerClass = '{obj.CustomerClass!.ToUpper()}', 
					CustomerSince = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					SalesPersonId = {obj.SalesPersonId}, 
					CreditLimit = {obj.CreditLimit}, 
					PaymentTermsId = {obj.PaymentTermsId}, 
					AccountId = {obj.AccountId}, 
					NTN = '{obj.NTN!.ToUpper()}', 
					Address = '{obj.Address!.ToUpper()}', 
					CityId = {obj.CityId}, 
					Latitude = '{obj.Latitude!.ToUpper()}', 
					Longitude = '{obj.Longitude!.ToUpper()}', 
					Radius = {obj.Radius}, 
					ContactPerson = '{obj.ContactPerson!.ToUpper()}', 
					ContactDesignation = '{obj.ContactDesignation!.ToUpper()}', 
					ContactEmail = '{obj.ContactEmail!.ToUpper()}', 
					Pic = '{obj.Pic!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}'
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate, SQLDuplicate);
            if (res.Item1 == true)
            {
                List<PartiesModel> Output = new List<PartiesModel>();
                var result = await Search($"a.id={res.Item2}")!;
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
            return (false, null!, ex.Message);
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
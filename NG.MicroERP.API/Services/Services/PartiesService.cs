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
            string SQLDuplicate = $@"SELECT * FROM Parties WHERE UPPER(code) = '{obj.Code!.ToUpper()}' AND IsSoftDeleted = 0;";
            // Convert 0 to NULL for nullable foreign key fields to avoid FK constraint violations
            string parentIdValue = obj.ParentId > 0 ? obj.ParentId.ToString() : "NULL";
            string salesPersonIdValue = obj.SalesPersonId > 0 ? obj.SalesPersonId.ToString() : "NULL";
            string paymentTermsIdValue = obj.PaymentTermsId > 0 ? obj.PaymentTermsId.ToString() : "NULL";
            string accountIdValue = obj.AccountId > 0 ? obj.AccountId.ToString() : "NULL";
            string cityIdValue = obj.CityId > 0 ? obj.CityId.ToString() : "NULL";
            
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
				'{obj.Name?.ToUpper().Replace("'", "''") ?? ""}', 
				'{obj.PartyType?.ToUpper().Replace("'", "''") ?? ""}', 
				{parentIdValue},
				{obj.CustomerRating},
				'{obj.CustomerClass?.ToUpper().Replace("'", "''") ?? ""}', 
				'{(obj.CustomerSince?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}',
				{salesPersonIdValue},
				{obj.CreditLimit},
				{paymentTermsIdValue},
				{accountIdValue},
				'{obj.NTN?.ToUpper().Replace("'", "''") ?? ""}',
				'{obj.STN?.ToUpper().Replace("'", "''") ?? ""}',
				'{obj.Address?.ToUpper().Replace("'", "''") ?? ""}', 
				{cityIdValue},
				'{obj.Latitude?.ToUpper().Replace("'", "''") ?? ""}', 
				'{obj.Longitude?.ToUpper().Replace("'", "''") ?? ""}', 
				{obj.Radius},
				'{obj.ContactPerson?.ToUpper().Replace("'", "''") ?? ""}', 
				'{obj.ContactDesignation?.ToUpper().Replace("'", "''") ?? ""}', 
				'{obj.ContactEmail?.ToUpper().Replace("'", "''") ?? ""}', 
				'{obj.Pic?.Replace("'", "''") ?? ""}', 
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom?.ToUpper().Replace("'", "''") ?? ""}'
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
            // Convert 0 to NULL for nullable foreign key fields to avoid FK constraint violations
            string parentIdValue = obj.ParentId > 0 ? obj.ParentId.ToString() : "NULL";
            string salesPersonIdValue = obj.SalesPersonId > 0 ? obj.SalesPersonId.ToString() : "NULL";
            string paymentTermsIdValue = obj.PaymentTermsId > 0 ? obj.PaymentTermsId.ToString() : "NULL";
            string accountIdValue = obj.AccountId > 0 ? obj.AccountId.ToString() : "NULL";
            string cityIdValue = obj.CityId > 0 ? obj.CityId.ToString() : "NULL";
            
            string SQLDuplicate = $@"SELECT * FROM Parties WHERE UPPER(code) = '{obj.Code!.ToUpper()}' AND Id != {obj.Id} AND IsSoftDeleted = 0;";
            string SQLUpdate = $@"UPDATE Parties SET 
					OrganizationId = {obj.OrganizationId}, 
					Code = '{obj.Code?.ToUpper().Replace("'", "''") ?? ""}', 
					Name = '{obj.Name?.ToUpper().Replace("'", "''") ?? ""}', 
					PartyType = '{obj.PartyType?.ToUpper().Replace("'", "''") ?? ""}', 
					ParentId = {parentIdValue}, 
					CustomerRating = {obj.CustomerRating}, 
					CustomerClass = '{obj.CustomerClass?.ToUpper().Replace("'", "''") ?? ""}', 
					CustomerSince = '{(obj.CustomerSince?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}', 
					SalesPersonId = {salesPersonIdValue}, 
					CreditLimit = {obj.CreditLimit}, 
					PaymentTermsId = {paymentTermsIdValue}, 
					AccountId = {accountIdValue}, 
					NTN = '{obj.NTN?.ToUpper().Replace("'", "''") ?? ""}', 
					STN = '{obj.STN?.ToUpper().Replace("'", "''") ?? ""}', 
					Address = '{obj.Address?.ToUpper().Replace("'", "''") ?? ""}', 
					CityId = {cityIdValue}, 
					Latitude = '{obj.Latitude?.ToUpper().Replace("'", "''") ?? ""}', 
					Longitude = '{obj.Longitude?.ToUpper().Replace("'", "''") ?? ""}', 
					Radius = {obj.Radius}, 
					ContactPerson = '{obj.ContactPerson?.ToUpper().Replace("'", "''") ?? ""}', 
					ContactDesignation = '{obj.ContactDesignation?.ToUpper().Replace("'", "''") ?? ""}', 
					ContactEmail = '{obj.ContactEmail?.ToUpper().Replace("'", "''") ?? ""}', 
					Pic = '{obj.Pic?.Replace("'", "''") ?? ""}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom?.ToUpper().Replace("'", "''") ?? ""}'
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
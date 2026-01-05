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

public interface ITypeCodeService
{
    Task<(bool, List<TypeCodeModel>)>? Search(string Criteria = "");
    Task<(bool, TypeCodeModel?)>? Get(int id);
    Task<(bool, TypeCodeModel, string)> Post(TypeCodeModel obj);
    Task<(bool, TypeCodeModel, string)> Put(TypeCodeModel obj);
    Task<(bool, string)> Delete(int id);
}

public class TypeCodeService : ITypeCodeService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<TypeCodeModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT * FROM TypeCode";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " Where " + Criteria;

        //SQL += " Order by Id Desc";

        List<TypeCodeModel> result = (await dapper.SearchByQuery<TypeCodeModel>(SQL)) ?? new List<TypeCodeModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, TypeCodeModel?)>? Get(int id)
    {
        TypeCodeModel result = (await dapper.SearchByID<TypeCodeModel>("TypeCode", id)) ?? new TypeCodeModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, TypeCodeModel, string)> Post(TypeCodeModel obj)
    {
        try
        {
            string SQLInsert = $@"INSERT INTO TypeCode 
			(
				OrganizationId, 
				ListName, 
				ListValue, 
				ParentId, 
				SeqNo
			) 
			VALUES 
			(
				{obj.OrganizationId},
				'{obj.ListName!.ToUpper()}', 
				'{obj.ListValue!.ToUpper()}', 
				{obj.ParentId},
				{obj.SeqNo}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<TypeCodeModel> Output = new List<TypeCodeModel>();
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
            return (false, null!, ex.Message);
        }
    }

    public async Task<(bool, TypeCodeModel, string)> Put(TypeCodeModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE TypeCode SET 
					OrganizationId = {obj.OrganizationId}, 
					ListName = '{obj.ListName!.ToUpper()}', 
					ListValue = '{obj.ListValue!.ToUpper()}', 
					ParentId = {obj.ParentId}, 
					SeqNo = {obj.SeqNo} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<TypeCodeModel> Output = new List<TypeCodeModel>();
                var result = await Search($"id={obj.Id}")!;
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
        return await dapper.Delete("TypeCode", id);
    }

}
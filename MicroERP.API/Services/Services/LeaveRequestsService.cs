using MicroERP.API.Helper;
using MicroERP.Shared.Models;

namespace MicroERP.API.Services.Services;


public interface ILeaveRequestsService
{
    Task<(bool, List<LeaveRequestsModel>)>? Search(string Criteria = "");
    Task<(bool, LeaveRequestsModel?)>? Get(int id);
    Task<(bool, LeaveRequestsModel, string)> Post(LeaveRequestsModel obj);
    Task<(bool, LeaveRequestsModel, string)> Put(LeaveRequestsModel obj);
    Task<(bool, string)> Delete(int id);
    Task<(bool, string)> SoftDelete(LeaveRequestsModel obj);
}


public class LeaveRequestsService : ILeaveRequestsService
{
    DapperFunctions dapper = new DapperFunctions();

    public async Task<(bool, List<LeaveRequestsModel>)>? Search(string Criteria = "")
    {
        string SQL = $@"SELECT
                        a.Id,
                        a.OrganizationId,
                        a.EmpId,
                        COALESCE(c.Fullname, 'Employee Not Found') as Fullname,
                        a.LeaveTypeId,
                        COALESCE(b.LeaveName, 'Leave Type Not Found') as LeaveName,
                        a.StartDate,
                        a.EndDate,
                        a.Reason,
                        a.ContactAddress,
                        a.ContactNumber,
                        a.Status,
                        a.AppliedDate,
                        COALESCE(d.Fullname, 'Not Approved') as ApprovedByName,
                        a.ApprovedDate,
                        a.Remarks
                    FROM LeaveRequests as a
                    LEFT JOIN leavetypes as b on b.Id = a.LeaveTypeId
                    LEFT JOIN employees as c on c.Id = a.EmpId
                    LEFT JOIN employees as d on d.Id = a.ApprovedBy";

        if (!string.IsNullOrWhiteSpace(Criteria))
            SQL += " Where " + Criteria;

        SQL += " Order by a.Id Desc";

        List<LeaveRequestsModel> result = (await dapper.SearchByQuery<LeaveRequestsModel>(SQL)) ?? new List<LeaveRequestsModel>();

        if (result == null || result.Count == 0)
            return (false, null!);
        else
            return (true, result);
    }

    public async Task<(bool, LeaveRequestsModel?)>? Get(int id)
    {
        LeaveRequestsModel result = (await dapper.SearchByID<LeaveRequestsModel>("LeaveRequests", id)) ?? new LeaveRequestsModel();
        if (result == null || result.Id == 0)
            return (false, null);
        else
            return (true, result);
    }


    public async Task<(bool, LeaveRequestsModel, string)> Post(LeaveRequestsModel obj)
    {

        try
        {
            string SQLInsert = $@"INSERT INTO LeaveRequests 
			(
				OrganizationId, 
				EmpId, 
				LeaveTypeId, 
				StartDate, 
				EndDate, 
				Reason, 
				ContactAddress, 
				ContactNumber, 
				Status, 
				AppliedDate, 
				Remarks,
                ApprovedBy,
                ApprovedDate,
				IsActive, 
				CreatedBy, 
				CreatedOn, 
				CreatedFrom, 
				IsSoftDeleted
			) 
			VALUES 
			(
				{obj.OrganizationId},
				{obj.EmpId},
				{obj.LeaveTypeId},
				'{(obj.StartDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}',
				'{(obj.EndDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}',
				'{obj.Reason!.ToUpper()}', 
				'{obj.ContactAddress!.ToUpper()}', 
				'{obj.ContactNumber!.ToUpper()}', 
				'{obj.Status!.ToUpper()}', 
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{(obj.Remarks != null ? obj.Remarks.ToUpper() : "")}',
				{obj.ApprovedBy},
				'{(obj.ApprovedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}',
				{obj.IsActive},
				{obj.CreatedBy},
				'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
				'{obj.CreatedFrom!.ToUpper()}', 
				{obj.IsSoftDeleted}
			);";

            var res = await dapper.Insert(SQLInsert);
            if (res.Item1 == true)
            {
                List<LeaveRequestsModel> Output = new List<LeaveRequestsModel>();
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

    public async Task<(bool, LeaveRequestsModel, string)> Put(LeaveRequestsModel obj)
    {
        try
        {
            string SQLUpdate = $@"UPDATE LeaveRequests SET 
					OrganizationId = {obj.OrganizationId}, 
					EmpId = {obj.EmpId}, 
					LeaveTypeId = {obj.LeaveTypeId}, 
					StartDate = '{(obj.StartDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}', 
					EndDate = '{(obj.EndDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}', 
					Reason = '{obj.Reason!.ToUpper()}', 
					ContactAddress = '{obj.ContactAddress!.ToUpper()}', 
					ContactNumber = '{obj.ContactNumber!.ToUpper()}', 
					Status = '{obj.Status!.ToUpper()}', 
					AppliedDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					Remarks = '{obj.Remarks!.ToUpper()}', 
					IsActive = {obj.IsActive}, 
					UpdatedBy = {obj.UpdatedBy}, 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedFrom = '{obj.UpdatedFrom!.ToUpper()}', 
					IsSoftDeleted = {obj.IsSoftDeleted} 
				WHERE Id = {obj.Id};";

            var res = await dapper.Update(SQLUpdate);
            if (res.Item1 == true)
            {
                List<LeaveRequestsModel> Output = new List<LeaveRequestsModel>();
                var result = await Search($"a.id={obj.Id}")!;
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
        return await dapper.Delete("LeaveRequests", id);
    }

    public async Task<(bool, string)> SoftDelete(LeaveRequestsModel obj)
    {
        string SQLUpdate = $@"UPDATE LeaveRequests SET 
					UpdatedOn = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', 
					UpdatedBy = '{obj.UpdatedBy!}',
					IsSoftDeleted = 1 
				WHERE Id = {obj.Id};";

        return await dapper.Update(SQLUpdate);
    }
}



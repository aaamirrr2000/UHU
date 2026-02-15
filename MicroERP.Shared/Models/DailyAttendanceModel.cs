using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class DailyAttendanceModel
{
    public int EmpId { get; set; }
    public string? Fullname { get; set; }

    public int DepartmentId { get; set; }
    public string? Department { get; set; }

    public int? ParentDepartmentId { get; set; }      
    public string? ParentDepartmentName { get; set; }   

    public int DesignationId { get; set; }
    public string? Designation { get; set; }

    public string? ShiftName { get; set; }
    public int? FlexiTime { get; set; }

    public DateTime QueryDate { get; set; }

    public DateTime? InDate { get; set; }
    public string? ScheduledIn { get; set; }
    public string? InTime { get; set; }
    public string? InDiff { get; set; }
    public string? ArrivalStatus { get; set; }

    public DateTime? OutDate { get; set; }
    public string? ScheduledOut { get; set; }
    public string? OutTime { get; set; }
    public string? OutDiff { get; set; }
    public string? LeavingStatus { get; set; }

    public int EventCount { get; set; }
    public string? Status { get; set; }

    public DateTime? LeaveStartDate { get; set; }
    public DateTime? LeaveEndDate { get; set; }
    public string? LeaveName { get; set; }
    public string? HolidayDescription { get; set; }
    public string? DayCategory { get; set; }
}
public class AttendanceSummaryModel
{
    public int TotalRecords { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int OnLeaveCount { get; set; }
    public int InCount { get; set; }
    public int OutCount { get; set; }
}


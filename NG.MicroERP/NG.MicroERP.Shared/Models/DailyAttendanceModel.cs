using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class DailyAttendanceModel
{
    // Employee Information
    public int EmpId { get; set; }
    public string Fullname { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;

    // Arrival Section
    public DateTime? InDate { get; set; }
    public string ScheduledIn { get; set; } = string.Empty; 
    public string InTime { get; set; } = string.Empty;    
    public string InDiff { get; set; } = string.Empty;     
    public string ArrivalStatus { get; set; } = string.Empty;

    // Leaving Section
    public DateTime? OutDate { get; set; }
    public string ScheduledOut { get; set; } = string.Empty; 
    public string OutTime { get; set; } = string.Empty;    
    public string OutDiff { get; set; } = string.Empty;    
    public string LeavingStatus { get; set; } = string.Empty;

    // Additional Fields from Updated Query
    public int EventCount { get; set; }
    public string DayType { get; set; } = string.Empty;


    // Optional: You might want these computed properties for easier access
    public bool HasArrival => EventCount >= 1;
    public bool HasLeaving => EventCount >= 2;
    public bool IsCompleteDay => EventCount >= 2;
    public bool IsAbsent => EventCount == 0;
    public bool IsIncompleteDay => EventCount == 1;
}


DELIMITER $$

CREATE DEFINER=`DevOps`@`%` PROCEDURE `GetAttendanceReport`(
    IN start_date DATE,
    IN end_date DATE
)
BEGIN
    -- Generate date range for the specified period
    WITH RECURSIVE DateRange AS (
        SELECT start_date AS QueryDate
        UNION ALL
        SELECT DATE_ADD(QueryDate, INTERVAL 1 DAY)
        FROM DateRange
        WHERE QueryDate < end_date
    ),

    -- Aggregate daily events (first & last swipes)
    DailyEvents AS (
        SELECT 
            b.EmpId,
            a.departmentId,
            c.DepartmentName,
            a.designationId,
            d.DesignationName,
            DATE(a.event_time) AS EventDate,
            MIN(a.event_time) AS FirstEvent,
            MAX(a.event_time) AS LastEvent,
            COUNT(a.event_time) AS EventCount,
            CASE 
                WHEN COUNT(a.event_time) = 1 THEN 'ARRIVAL_ONLY'
                ELSE 'COMPLETE_DAY'
            END AS DayType
        FROM acsevent AS a
        INNER JOIN employees AS b ON b.EmpId = a.employee_no
        INNER JOIN departments AS c ON c.Id = a.departmentId
        INNER JOIN designations AS d ON d.Id = a.designationId
        WHERE DATE(a.event_time) BETWEEN start_date AND end_date
        GROUP BY b.EmpId, DATE(a.event_time)
    ),

    -- List of all days in range with weekday info
    DateInfo AS (
        SELECT 
            dr.QueryDate,
            DAYOFWEEK(dr.QueryDate) AS DayOfWeek
        FROM DateRange dr
    ),

    -- Combine employee info, dates, and attendance including approved leaves
    EmployeeBase AS (
        SELECT
            e.Id AS EmpId,
            e.Fullname,
            e.OrganizationId,
            d.Id AS DepartmentId,
            d.DepartmentName AS Department,
            pd.Id AS ParentDepartmentId,
            pd.DepartmentName AS ParentDepartmentName,
            dd.Id AS DesignationId,
            dd.DesignationName AS Designation,
            s.ShiftName,
            s.FlexiTime,
            s.StartTime,
            s.EndTime,
            di.QueryDate,
            di.DayOfWeek,
            b.FirstEvent,
            b.LastEvent,
            b.EventCount,
            b.DayType,
            l.Id AS LeaveId,
            l.StartDate AS LeaveStartDate,
            l.EndDate AS LeaveEndDate,
            ll.LeaveName,
            h.Id AS HolidayId,
            h.Description AS HolidayDescription,

            CASE 
                WHEN di.DayOfWeek IN (1, 7) THEN 'WEEKEND'
                WHEN h.Id IS NOT NULL THEN 'HOLIDAY'
                WHEN l.Id IS NOT NULL THEN 'LEAVE'
                ELSE 'WORKING_DAY'
            END AS DayCategory

        FROM employees e
        CROSS JOIN DateInfo di
        LEFT JOIN DailyEvents b 
            ON b.EmpId = e.Id AND b.EventDate = di.QueryDate
        LEFT JOIN Shifts s 
            ON s.Id = e.ShiftId
        LEFT JOIN Departments d 
            ON d.Id = e.DepartmentId
        LEFT JOIN Departments pd
            ON d.ParentId = pd.Id
        LEFT JOIN Designations dd 
            ON dd.Id = e.DesignationId
        LEFT JOIN LeaveRequests l 
            ON l.EmpId = e.Id 
           AND di.QueryDate BETWEEN l.StartDate AND l.EndDate
           AND l.Status = 'APPROVED'
        LEFT JOIN LeaveTypes ll 
            ON ll.Id = l.LeaveTypeId
        LEFT JOIN HolidayCalendar h 
            ON h.HolidayDate = di.QueryDate 
           AND h.IsActive = 1 
           AND h.IsSoftDeleted = 0
           AND (h.OrganizationId = e.OrganizationId OR h.OrganizationId = 0)
    ),

    -- Calculate attendance timing differences and status
    AttendanceCalculations AS (
        SELECT
            *, 
            CASE
                WHEN DayCategory NOT IN ('WORKING_DAY') THEN NULL
                WHEN FirstEvent IS NULL THEN NULL
                WHEN TIME(FirstEvent) <= ADDTIME(StartTime, SEC_TO_TIME(FlexiTime * 60)) THEN '00:00'
                ELSE TIME_FORMAT(TIMEDIFF(TIME(FirstEvent), ADDTIME(StartTime, SEC_TO_TIME(FlexiTime * 60))), '%H:%i')
            END AS InDiff,

            CASE
                WHEN DayCategory = 'WEEKEND' THEN 'WEEKEND'
                WHEN DayCategory = 'HOLIDAY' THEN 'HOLIDAY'
                WHEN LeaveId IS NOT NULL THEN 'ON LEAVE'
                WHEN FirstEvent IS NULL THEN 'ABSENT'
                WHEN TIME(FirstEvent) > ADDTIME(StartTime, SEC_TO_TIME(FlexiTime * 60)) THEN 'LATE'
                ELSE 'ON TIME'
            END AS ArrivalStatus,

            CASE
                WHEN DayCategory NOT IN ('WORKING_DAY') THEN NULL
                WHEN LastEvent IS NULL OR EventCount <= 1 THEN NULL
                WHEN TIME(LastEvent) >= SUBTIME(EndTime, SEC_TO_TIME(FlexiTime * 60)) THEN '00:00'
                ELSE TIME_FORMAT(TIMEDIFF(SUBTIME(EndTime, SEC_TO_TIME(FlexiTime * 60)), TIME(LastEvent)), '%H:%i')
            END AS OutDiff,

            CASE
                WHEN DayCategory = 'WEEKEND' THEN 'WEEKEND'
                WHEN DayCategory = 'HOLIDAY' THEN 'HOLIDAY'
                WHEN LeaveId IS NOT NULL THEN 'ON LEAVE'
                WHEN FirstEvent IS NULL THEN 'ABSENT'
                WHEN EventCount = 1 THEN 'INCOMPLETE'
                WHEN TIME(LastEvent) < SUBTIME(EndTime, SEC_TO_TIME(FlexiTime * 60)) THEN 'BEFORE TIME'
                ELSE 'ON TIME'
            END AS LeavingStatus,

            CASE
                WHEN DayCategory = 'WEEKEND' AND DayOfWeek = 1 THEN 'SUNDAY'
                WHEN DayCategory = 'WEEKEND' AND DayOfWeek = 7 THEN 'SATURDAY'
                WHEN DayCategory = 'HOLIDAY' THEN CONCAT('HOLIDAY - ', HolidayDescription)
                WHEN LeaveId IS NOT NULL THEN CONCAT('ON ', LeaveName, ' LEAVE')
                WHEN DayCategory = 'WORKING_DAY' AND DayType IS NULL THEN 'ABSENT'
                ELSE DayType
            END AS Status
        FROM EmployeeBase
    )

    -- Final Output
    SELECT
        EmpId,
        Fullname,
        DepartmentId,
        Department,
        ParentDepartmentId,
        ParentDepartmentName,
        DesignationId,
        Designation,
        ShiftName,
        FlexiTime,
        QueryDate,

        COALESCE(DATE(FirstEvent), QueryDate) AS InDate,
        DATE_FORMAT(StartTime, '%h:%i %p') AS ScheduledIn,
        DATE_FORMAT(FirstEvent, '%h:%i %p') AS InTime,
        InDiff,
        ArrivalStatus,

        COALESCE(DATE(LastEvent), QueryDate) AS OutDate,
        DATE_FORMAT(EndTime, '%h:%i %p') AS ScheduledOut,
        DATE_FORMAT(LastEvent, '%h:%i %p') AS OutTime,
        OutDiff,
        LeavingStatus,

        COALESCE(EventCount, 0) AS EventCount,
        Status,
        LeaveStartDate,
        LeaveEndDate,
        LeaveName,
        HolidayDescription,
        DayCategory
    FROM AttendanceCalculations
    ORDER BY ParentDepartmentId, DepartmentId, EmpId, QueryDate;

END$$

DELIMITER ;

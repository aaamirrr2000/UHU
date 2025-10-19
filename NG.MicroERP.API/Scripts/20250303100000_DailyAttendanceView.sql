CREATE VIEW DailyAttendance AS
WITH DailyEvents AS (
    SELECT 
        b.EmpId,
        b.Fullname,
        b.Department,
        b.Division,
        b.InTime AS ScheduledIn,
        b.OutTime AS ScheduledOut,
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
    WHERE DATE(a.event_time) = '2025-10-22'
    GROUP BY b.EmpId, b.Fullname, b.Department, b.Division, b.InTime, b.OutTime, DATE(a.event_time)
)
SELECT
    e.Id AS EmpId,
    e.Fullname,
    e.Department,
    e.Division,
    -- Arrival Section
    COALESCE(DATE(b.FirstEvent), '2025-10-22') AS InDate,
    DATE_FORMAT(e.InTime, '%h:%i %p') AS ScheduledIn,
    DATE_FORMAT(b.FirstEvent, '%h:%i %p') AS InTime,
    
    CASE
        WHEN b.FirstEvent IS NULL THEN NULL
        WHEN TIME(b.FirstEvent) <= ADDTIME(TIME(e.InTime), '01:00:00') THEN '00:00'
        ELSE TIME_FORMAT(TIMEDIFF(TIME(b.FirstEvent), ADDTIME(TIME(e.InTime), '01:00:00')), '%H:%i')
    END AS InDiff,
    
    CASE
        WHEN b.FirstEvent IS NULL THEN 'ABSENT'
        WHEN b.EventCount = 1 AND TIME(b.FirstEvent) > ADDTIME(TIME(e.InTime), '01:00:00') THEN 'LATE'
        WHEN b.EventCount = 1 THEN 'ON TIME'
        WHEN TIME(b.FirstEvent) > ADDTIME(TIME(e.InTime), '01:00:00') THEN 'LATE'
        ELSE 'ON TIME'
    END AS ArrivalStatus,

    -- Leaving Section
    COALESCE(DATE(b.LastEvent), '2025-10-22') AS OutDate,
    DATE_FORMAT(e.OutTime, '%h:%i %p') AS ScheduledOut,
    CASE
        WHEN b.LastEvent IS NULL OR b.EventCount <= 1 THEN NULL
        ELSE DATE_FORMAT(b.LastEvent, '%h:%i %p')
    END AS OutTime,
    
    CASE
        WHEN b.LastEvent IS NULL OR b.EventCount <= 1 THEN NULL
        WHEN TIME(b.LastEvent) >= SUBTIME(TIME(e.OutTime), '01:00:00') THEN '00:00'
        ELSE TIME_FORMAT(TIMEDIFF(SUBTIME(TIME(e.OutTime), '01:00:00'), TIME(b.LastEvent)), '%H:%i')
    END AS OutDiff,
    
    CASE
        WHEN b.FirstEvent IS NULL THEN 'ABSENT'
        WHEN b.EventCount = 1 THEN 'INCOMPLETE'
        WHEN TIME(b.LastEvent) < SUBTIME(TIME(e.OutTime), '01:00:00') THEN 'BEFORE TIME'
        ELSE 'ON TIME'
    END AS LeavingStatus,

    COALESCE(b.EventCount, 0) AS EventCount,
    COALESCE(b.DayType, 'ABSENT') AS DayType

FROM employees e
LEFT JOIN DailyEvents b ON b.EmpId = e.Id
ORDER BY EmpId;

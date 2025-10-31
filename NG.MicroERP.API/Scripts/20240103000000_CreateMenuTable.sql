CREATE TABLE Menu
(
    Id                  INT             PRIMARY KEY AUTO_INCREMENT,
    Guid                CHAR(36)        NOT NULL DEFAULT (UUID()),
    MenuCaption         VARCHAR(100)    NULL DEFAULT NULL,
    AdditionalInfo      VARCHAR(100)    NULL DEFAULT NULL,
    Tooltip             VARCHAR(255)    NULL DEFAULT NULL,
    PageName            VARCHAR(100)    NULL DEFAULT NULL,
    ParentId            INT             NULL DEFAULT NULL,
    Icon                VARCHAR(50)     NULL DEFAULT NULL,
    SeqNo               INT             NULL DEFAULT NULL,
    Live                TINYINT         NULL DEFAULT NULL,
    CreatedBy           INT             NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    UpdatedBy           INT             NULL,
    UpdatedOn           DATETIME        NULL DEFAULT NULL,
    UpdatedFrom         VARCHAR(255)    NULL DEFAULT NULL,
    IsSoftDeleted       TINYINT         NOT NULL DEFAULT 0,
    RowVersion          TIMESTAMP       NULL
);

INSERT INTO Menu
    (Id, MenuCaption, AdditionalInfo, Tooltip, PageName, ParentId,
     Icon, SeqNo, Live,
     CreatedBy, CreatedFrom,
     UpdatedBy, UpdatedFrom,
     IsSoftDeleted)
VALUES
-- 📊 Attendance & Leave
(1, 'Attendance', NULL, 'Monitor and manage staff attendance', NULL, 0, 'fas fa-user-check', 5000, 1, 1, NULL, 1, NULL, 0),
(2, 'My Leave Request', NULL, 'Submit your leave requests', 'LeaveRequestsDashboard', 1, 'fas fa-paper-plane', 5010, 0, 1, NULL, 1, NULL, 0),
(3, 'Leave Requests', NULL, 'Manage leave requests of other employees', 'LeaveRequestsDashboard', 1, 'fas fa-envelope-open-text', 5020, 1, 1, NULL, 1, NULL, 0),

-- 🛡️ Security Setup
(100, 'Security Setup', NULL, 'Configure user security and permissions', NULL, 0, 'fas fa-shield-alt', 8000, 1, 1, NULL, 1, NULL, 0),
(101, 'Users', NULL, 'Add, edit, and deactivate system users', 'UsersDashboard', 100, 'fas fa-user', 8010, 1, 1, NULL, 1, NULL, 0),
(102, 'Groups', NULL, 'Create and manage user groups and roles', 'GroupsDashboard', 100, 'fas fa-users-cog', 8020, 1, 1, NULL, 1, NULL, 0),
(103, 'Permissions', NULL, 'Assign menu permissions to users or groups', 'PermissionsDashboard', 100, 'fas fa-lock', 8030, 1, 1, NULL, 1, NULL, 0),
(104, 'Scanner Devices', NULL, 'Configure Scanner Devices', 'ScannerDevicesDashboard', 100, 'fas fa-fingerprint', 8040, 1, 1, NULL, 1, NULL, 0),

-- ⚙️ General Setup
(200, 'General Setup', NULL, 'Configure core system settings and master data', NULL, 0, 'fas fa-cogs', 9000, 1, 1, NULL, 1, NULL, 0),
(205, 'Organization', NULL, 'Maintain organisation profile and settings', 'OrganizationsDashboard', 200, 'fas fa-building', 9010, 1, 1, NULL, 1, NULL, 0),
(210, 'Locations', NULL, 'Maintain branch or warehouse locations', 'LocationsDashboard', 200, 'fas fa-map-marker-alt', 9020, 1, 1, NULL, 1, NULL, 0),
(215, 'Employee', NULL, 'Maintain employee records and HR details', 'EmployeesDashboard', 200, 'fas fa-id-card', 9030, 1, 1, NULL, 1, NULL, 0),
(216, 'Departments', NULL, 'Manage departments and sub-departments', 'DepartmentsDashboard', 200, 'fas fa-sitemap', 9040, 1, 1, NULL, 1, NULL, 0),
(217, 'Designations', NULL, 'Manage employee designations and reporting lines', 'DesignationsDashboard', 200, 'fas fa-user-tag', 9050, 1, 1, NULL, 1, NULL, 0),
(218, 'Shifts Mgt', NULL, 'Maintain employee shift schedules', 'ShiftsDashboard', 200, 'fas fa-clock', 9060, 1, 1, NULL, 1, NULL, 0),
(220, 'Areas', NULL, 'Maintain cities, regions, provinces, and countries', 'AreasDashboard', 200, 'fas fa-globe-asia', 9070, 1, 1, NULL, 1, NULL, 0),
(225, 'Leave Types', NULL, 'Define leave types and rules', 'LeaveTypesDashboard', 200, 'fas fa-umbrella-beach', 9080, 1, 1, NULL, 1, NULL, 0),
(230, 'Holiday Calendar', NULL, 'Define public holidays and off days', 'HolidayCalendarDashboard', 200, 'fas fa-calendar-alt', 9090, 1, 1, NULL, 1, NULL, 0);


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
     Icon,      SeqNo, Live,
     CreatedBy, CreatedFrom,
     UpdatedBy, UpdatedFrom,
     IsSoftDeleted)
VALUES



(1 , 'Reports'             , NULL , 'Generate operational and financial reports'
                                 , NULL                        , 0
                                 , 'fas fa-chart-line'         , 5000, 0
                                 , 1 , NULL
                                 , 1 , NULL
                                 , 0),

(2 , 'Attendance'             , NULL , 'Generate operational and financial reports'
                                 , NULL                        , 0
                                 , 'fas fa-chart-line'         , 5000, 2
                                 , 1 , NULL
                                 , 1 , NULL
                                 , 0),

(3 , 'Security Setup'        , NULL , 'Configure user security and permissions'
                              , NULL                        , 0
                              , 'fas fa-shield-alt'         , 8000, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0),

(4, 'Users'           , NULL , 'Add, edit, and deactivate system users'
                              , 'UsersDashboard'            , 3
                              , 'fas fa-user'               , 8010, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0),

(5, 'Groups'          , NULL , 'Create and manage user groups and roles'
                              , 'GroupsDashboard'           , 3
                              , 'fas fa-users-cog'          , 8020, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0),

(6, 'Permissions'     , NULL , 'Assign menu permissions to users or groups'
                              , 'PermissionsDashboard'      , 3
                              , 'fas fa-lock'               , 8030, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0),

/*── SETUP / MASTER DATA ────────────────────────────────*/
(7, 'General Setup'           , NULL , 'Configure core system settings and master data'
                              , NULL                        , 0
                              , 'fas fa-cogs'               , 9000, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0),

(8, 'Organization'    , NULL , 'Maintain organisation profile and settings'
                              , 'OrganizationsDashboard'     , 7
                              , 'fas fa-building'           , 9010, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0),

(9, 'Locations'       , NULL , 'Maintain branch or warehouse locations'
                              , 'LocationsDashboard'        , 7
                              , 'fas fa-map-marker-alt'     , 9020, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0),

(10, 'Employee'        , NULL , 'Maintain employee records and HR details'
                              , 'EmployeesDashboard'        , 7
                              , 'fas fa-id-badge'           , 9030, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0),


(11, 'Areas'    , NULL , 'Maintain Cities, Regions, Province and Countries'
                              , 'AreasDashboard'     , 7
                              , 'fas fa-building'           , 9010, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0),

(12, 'Leave Types'    , NULL , 'Define Leave Types'
                              , 'LeaveTypesDashboard'     , 7
                              , 'fas fa-building'           , 9010, 1
                              , 1 , NULL
                              , 1 , NULL
                              , 0);
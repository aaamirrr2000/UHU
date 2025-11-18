CREATE TABLE HolidayCalendar
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId      INT NULL DEFAULT 1,
    HolidayDate         DATE            NOT NULL,
    Description         VARCHAR(100),
    IsRecurring         BIT             DEFAULT 0,
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedBy           INT             NULL,
    CreatedOn           DATETIME        NOT NULL DEFAULT GETDATE(),
    CreatedFrom         VARCHAR(255)    NULL,
    UpdatedBy           INT             NULL,
    UpdatedOn           DATETIME        NOT NULL DEFAULT GETDATE(),
    UpdatedFrom         VARCHAR(255)    NULL,
    IsSoftDeleted       BIT             NULL DEFAULT 0,

    CONSTRAINT FK_HolidayCalendar_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_HolidayCalendar_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_HolidayCalendar_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

INSERT INTO HolidayCalendar (OrganizationId, HolidayDate, Description, IsRecurring, IsActive)
VALUES
(1, '2025-02-05', 'Kashmir Solidarity Day', 0, 1),
(1, '2025-03-23', 'Pakistan Day', 1, 1),
(1, '2025-05-01', 'Labour Day', 1, 1),
(1, '2025-08-14', 'Independence Day', 1, 1),
(1, '2025-12-25', 'Quaid-e-Azam Day / Christmas', 1, 1),

-- Religious (approximate, adjust per lunar calendar)
(1, '2025-03-01', 'Eid Milad-un-Nabi (PBUH)', 1, 1),
(1, '2025-03-31', '1st Ramadan (Start of Fasting)', 0, 1),
(1, '2025-04-29', 'Eid-ul-Fitr (1st Day)', 0, 1),
(1, '2025-04-30', 'Eid-ul-Fitr (2nd Day)', 0, 1),
(1, '2025-05-01', 'Eid-ul-Fitr (3rd Day)', 0, 1),
(1, '2025-06-06', 'Youm-e-Ali', 0, 1),
(1, '2025-06-08', 'Shab-e-Miraj', 0, 1),
(1, '2025-06-28', 'Eid-ul-Adha (1st Day)', 0, 1),
(1, '2025-06-29', 'Eid-ul-Adha (2nd Day)', 0, 1),
(1, '2025-06-30', 'Eid-ul-Adha (3rd Day)', 0, 1),
(1, '2025-07-05', 'Hajj Day / Youm-e-Arafat', 0, 1),
(1, '2025-07-28', 'Ashura (9th Muharram)', 0, 1),
(1, '2025-07-29', 'Ashura (10th Muharram)', 0, 1),

-- Regional / Optional
(1, '2025-11-09', 'Iqbal Day', 1, 1);

CREATE VIEW GroupMenu AS
WITH GroupMenuCTE AS
(
    SELECT
        g.Id AS GroupId,
        g.Name AS GroupName,
        g.IsActive,
        m.Id AS MenuId,
        m.MenuCaption AS MenuCaption,
        m.Tooltip AS Tooltip,
        m.AdditionalInfo AS AdditionalInfo,
        m.ParentId AS ParentId,
        m.PageName AS PageName,
        m.Icon AS Icon,
        m.SeqNo AS SeqNo,
        m.Live AS Live,
        g.OrganizationId
    FROM Groups AS g
    CROSS JOIN Menu AS m
)
SELECT
    cte.*,
    ISNULL(p.Privilege, '') AS My_Privilege
FROM GroupMenuCTE AS cte
LEFT JOIN Permissions AS p
    ON p.GroupId = cte.GroupId
    AND p.MenuId = cte.MenuId;

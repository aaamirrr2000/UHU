CREATE VIEW GroupMenu AS 
WITH GroupMenuCTE AS (
    SELECT
        a.Id AS GroupId,
        a.Name AS GroupName,
        a.IsActive,
        b.Id AS MenuId,
        b.MenuCaption AS MenuCaption,
        b.Tooltip AS Tooltip,
        b.AdditionalInfo AS AdditionalInfo,
        b.ParentId AS ParentId,
        b.PageName AS PageName,
        b.Icon AS Icon,
        b.SeqNo AS SeqNo,
        b.Live AS Live,
        a.OrganizationId
    FROM Groups AS a
    CROSS JOIN Menu AS b
)
SELECT
    a.*,
    CASE
        WHEN b.Privilege IS NULL THEN ''
        ELSE b.Privilege
    END AS My_Privilege
FROM GroupMenuCTE AS a
LEFT JOIN Permissions AS b ON b.GroupId = a.GroupId AND b.MenuId = a.MenuId;
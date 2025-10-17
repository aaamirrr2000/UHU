CREATE VIEW GroupMenu AS 

WITH
GroupMenu as
(
	SELECT
		a.ID AS GroupId,
		a.name AS GroupName,
		a.IsActive,
		b.ID AS MenuId,
		b.menucaption as MenuCaption,
		b.tooltip as Tooltip,
		b.additionalinfo as AdditionalInfo,
		b.parentid as ParentId,
		b.pagename as PageName,
		b.icon as Icon,
		b.seqno as SeqNo,
		b.live as Live,
    a.OrganizationId
	FROM groups AS a, menu AS b
)
select
	a.*,
	 CASE
     WHEN
         b.privilege IS NULL THEN ''
     ELSE
         b.privilege
 END AS My_Privilege
from GroupMenu as a
LEFT JOIN permissions as b on b.groupid=a.groupid and b.menuid=a.menuid;
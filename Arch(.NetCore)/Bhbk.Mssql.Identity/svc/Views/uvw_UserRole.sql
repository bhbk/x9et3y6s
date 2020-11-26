
CREATE VIEW [svc].[uvw_UserRole]
AS
SELECT
	UserId
	,RoleId
	,IsDeletable
	,CreatedUtc

FROM
	[dbo].[tbl_UserRole]
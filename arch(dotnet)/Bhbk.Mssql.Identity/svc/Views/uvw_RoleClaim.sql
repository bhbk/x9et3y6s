
CREATE VIEW [svc].[uvw_RoleClaim]
AS
SELECT
	RoleId
	,ClaimId
	,IsDeletable
	,CreatedUtc

FROM
	[dbo].[tbl_RoleClaim]
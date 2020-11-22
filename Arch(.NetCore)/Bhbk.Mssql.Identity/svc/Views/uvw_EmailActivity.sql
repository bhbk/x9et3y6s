




CREATE VIEW [svc].[uvw_EmailActivity]
AS
SELECT	
	Id
	,EmailId
	,SendgridId
	,SendgridStatus
	,StatusAtUtc

FROM
	[dbo].[tbl_EmailActivity]
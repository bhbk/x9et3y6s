


CREATE VIEW [svc].[uvw_AuthActivity]
AS
SELECT        
	Id
	,AudienceId
	,UserId
	,LoginType
	,LoginOutcome
	,LocalEndpoint
	,RemoteEndpoint
	,CreatedUtc

FROM
	[dbo].[tbl_AuthActivity]
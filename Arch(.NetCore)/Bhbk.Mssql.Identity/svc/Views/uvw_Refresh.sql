
CREATE VIEW [svc].[uvw_Refresh]
AS
SELECT
	Id
	,IssuerId
	,AudienceId
	,UserId
	,RefreshValue
	,RefreshType
	,ValidFromUtc
	,ValidToUtc
	,IssuedUtc

FROM
	[dbo].[tbl_Refresh]
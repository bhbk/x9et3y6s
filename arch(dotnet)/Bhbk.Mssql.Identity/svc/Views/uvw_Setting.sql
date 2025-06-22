
CREATE VIEW [svc].[uvw_Setting]
AS
SELECT
	Id
	,IssuerId
	,AudienceId
	,UserId
	,ConfigKey
	,ConfigValue
	,IsDeletable
	,CreatedUtc

FROM
	[dbo].[tbl_Setting]
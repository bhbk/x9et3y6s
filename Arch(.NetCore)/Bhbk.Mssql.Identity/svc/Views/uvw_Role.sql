

CREATE VIEW [svc].[uvw_Role]
AS
SELECT
	Id
	,AudienceId
	,Name
	,Description
	,IsEnabled
	,IsDeletable
	,CreatedUtc

FROM
	[dbo].[tbl_Role]
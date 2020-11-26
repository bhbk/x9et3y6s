
CREATE VIEW [svc].[uvw_Issuer]
AS
SELECT
	Id
	,Name
	,Description
	,IssuerKey
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Issuer]

CREATE VIEW [svc].[uvw_TextQueue]
AS
SELECT
	Id
	,FromPhoneNumber
	,ToPhoneNumber
	,Body
	,IsCancelled
	,CreatedUtc
	,SendAtUtc
	,DeliveredUtc

FROM
	[dbo].[tbl_TextQueue]
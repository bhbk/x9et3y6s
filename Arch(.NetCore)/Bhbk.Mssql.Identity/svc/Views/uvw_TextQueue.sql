


CREATE VIEW [svc].[uvw_TextQueue]
AS
SELECT
	Id
	,FromId
	,FromPhoneNumber
	,ToId
	,ToPhoneNumber
	,Body
	,CreatedUtc
	,SendAtUtc
	,DeliveredUtc

FROM
	[dbo].[tbl_TextQueue]
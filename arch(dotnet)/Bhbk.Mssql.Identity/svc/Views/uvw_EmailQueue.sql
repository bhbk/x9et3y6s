
CREATE VIEW [svc].[uvw_EmailQueue]
AS
SELECT	
	Id
	,FromEmail
	,FromDisplay
	,ToEmail
	,ToDisplay
	,Subject
	,Body
	,IsCancelled
	,CreatedUtc
	,SendAtUtc
	,DeliveredUtc

FROM
	[dbo].[tbl_EmailQueue]
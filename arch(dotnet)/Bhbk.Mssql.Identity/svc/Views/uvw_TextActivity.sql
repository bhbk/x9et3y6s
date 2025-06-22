
CREATE VIEW [svc].[uvw_TextActivity]
AS
SELECT	
	Id
	,TextId
	,TwilioSid
	,TwilioStatus
	,TwilioMessage
	,StatusAtUtc

FROM
	[dbo].[tbl_TextActivity]
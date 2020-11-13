﻿


CREATE VIEW [svc].[uvw_EmailQueue]
AS
SELECT	
	Id
	,ActorId
	,FromId
	,FromEmail
	,FromDisplay
	,ToId
	,ToEmail
	,ToDisplay
	,Subject
	,Body
	,CreatedUtc
	,SendAtUtc
	,DeliveredUtc

FROM
	[dbo].[tbl_EmailQueue]
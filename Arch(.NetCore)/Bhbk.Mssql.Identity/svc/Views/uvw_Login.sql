﻿

CREATE VIEW [svc].[uvw_Login]
AS
SELECT
	Id
	,Name
	,Description
	,LoginKey
	,IsEnabled
	,IsDeletable
	,CreatedUtc

FROM
	[dbo].[tbl_Login]
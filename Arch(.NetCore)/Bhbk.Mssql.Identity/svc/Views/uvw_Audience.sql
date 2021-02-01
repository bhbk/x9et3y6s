

CREATE VIEW [svc].[uvw_Audience]
AS
SELECT
	Id
	,IssuerId
	,Name
	,Description
	,ConcurrencyStamp
	,PasswordHashPBKDF2
	,PasswordHashSHA256
	,SecurityStamp
	,IsLockedOut
	,IsDeletable
	,LockoutEndUtc
	,CreatedUtc

FROM
	[dbo].[tbl_Audience]


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
	,AccessFailedCount
	,AccessSuccessCount
	,LockoutEndUtc
	,LastLoginSuccessUtc
	,LastLoginFailureUtc
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Audience]

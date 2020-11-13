
CREATE VIEW [svc].[uvw_Audience]
AS
SELECT
	Id
	,IssuerId
	,ActorId
	,Name
	,Description
	,ConcurrencyStamp
	,PasswordHashPBKDF2
	,PasswordHashSHA256
	,SecurityStamp
	,IsLockedOut
	,IsEnabled
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

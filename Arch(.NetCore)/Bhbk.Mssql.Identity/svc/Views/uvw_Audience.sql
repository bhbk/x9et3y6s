
CREATE VIEW [svc].[uvw_Audience]
AS
SELECT        Id, IssuerId, ActorId, Name, Description, ConcurrencyStamp, PasswordHashPBKDF2, PasswordHashSHA256, SecurityStamp, Enabled, Created, LockoutEnabled, LockoutEnd, LastLoginSuccess, LastLoginFailure, 
                         AccessFailedCount, AccessSuccessCount, LastUpdated, Immutable
FROM            dbo.tbl_Audience

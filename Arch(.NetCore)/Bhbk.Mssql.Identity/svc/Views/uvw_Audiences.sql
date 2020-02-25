
CREATE VIEW [svc].[uvw_Audiences]
AS
SELECT        Id, IssuerId, ActorId, Name, Description, ConcurrencyStamp, PasswordHash, SecurityStamp, AudienceType, Enabled, Created, LockoutEnabled, LockoutEnd, LastLoginSuccess, LastLoginFailure, AccessFailedCount, 
                         AccessSuccessCount, LastUpdated, Immutable
FROM            dbo.tbl_Audiences
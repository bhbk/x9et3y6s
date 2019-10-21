CREATE VIEW svc.uvw_Users
AS
SELECT        Id, ActorId, Email, EmailConfirmed, FirstName, LastName, PhoneNumber, PhoneNumberConfirmed, Created, LastUpdated, LockoutEnabled, LockoutEnd, LastLoginSuccess, LastLoginFailure, AccessFailedCount, 
                         AccessSuccessCount, ConcurrencyStamp, PasswordHash, PasswordConfirmed, SecurityStamp, TwoFactorEnabled, HumanBeing, Immutable
FROM            dbo.tbl_Users
GO

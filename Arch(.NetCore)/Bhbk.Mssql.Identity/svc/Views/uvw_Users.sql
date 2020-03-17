
CREATE VIEW svc.uvw_Users
AS
SELECT        Id, ActorId, UserName, EmailAddress, EmailConfirmed, FirstName, LastName, PhoneNumber, PhoneNumberConfirmed, Created, LastUpdated, LockoutEnabled, LockoutEnd, LastLoginSuccess, LastLoginFailure, 
                         AccessFailedCount, AccessSuccessCount, ConcurrencyStamp, PasswordHashPBKDF2, PasswordHashSHA256, PasswordConfirmed, SecurityStamp, TwoFactorEnabled, HumanBeing, Immutable
FROM            dbo.tbl_Users

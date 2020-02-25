
CREATE VIEW [svc].[uvw_Logins]
AS
SELECT        Id, ActorId, Name, Description, LoginKey, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_Logins
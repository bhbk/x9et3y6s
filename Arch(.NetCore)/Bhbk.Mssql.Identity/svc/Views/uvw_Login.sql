
CREATE VIEW [svc].[uvw_Login]
AS
SELECT        Id, ActorId, Name, Description, LoginKey, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_Login
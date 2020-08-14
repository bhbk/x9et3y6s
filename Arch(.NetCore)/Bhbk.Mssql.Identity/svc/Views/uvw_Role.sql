
CREATE VIEW [svc].[uvw_Role]
AS
SELECT        Id, AudienceId, ActorId, Name, Description, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_Role
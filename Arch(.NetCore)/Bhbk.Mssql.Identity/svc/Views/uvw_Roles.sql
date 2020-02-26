
CREATE VIEW [svc].[uvw_Roles]
AS
SELECT        Id, AudienceId, ActorId, Name, Description, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_Roles
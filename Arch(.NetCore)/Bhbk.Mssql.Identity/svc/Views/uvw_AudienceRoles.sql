
CREATE VIEW [svc].[uvw_AudienceRoles]
AS
SELECT        AudienceId, RoleId, ActorId, Created, Immutable
FROM            dbo.tbl_AudienceRoles
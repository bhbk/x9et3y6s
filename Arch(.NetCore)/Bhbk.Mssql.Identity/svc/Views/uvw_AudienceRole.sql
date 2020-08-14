
CREATE VIEW [svc].[uvw_AudienceRole]
AS
SELECT        AudienceId, RoleId, ActorId, Created, Immutable
FROM            dbo.tbl_AudienceRole
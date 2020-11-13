
CREATE VIEW [svc].[uvw_AudienceRole]
AS
SELECT        AudienceId, RoleId, ActorId, IsDeletable, CreatedUtc
FROM            [dbo].[tbl_AudienceRole]
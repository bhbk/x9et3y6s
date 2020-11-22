
CREATE VIEW [svc].[uvw_AudienceRole]
AS
SELECT        AudienceId, RoleId, IsDeletable, CreatedUtc
FROM            [dbo].[tbl_AudienceRole]
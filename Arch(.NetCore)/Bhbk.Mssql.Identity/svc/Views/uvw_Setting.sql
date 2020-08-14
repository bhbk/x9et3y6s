
CREATE VIEW [svc].[uvw_Setting]
AS
SELECT        Id, IssuerId, AudienceId, UserId, ConfigKey, ConfigValue, Created, Immutable
FROM            dbo.tbl_Setting
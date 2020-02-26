
CREATE VIEW [svc].[uvw_Settings]
AS
SELECT        Id, IssuerId, AudienceId, UserId, ConfigKey, ConfigValue, Created, Immutable
FROM            dbo.tbl_Settings
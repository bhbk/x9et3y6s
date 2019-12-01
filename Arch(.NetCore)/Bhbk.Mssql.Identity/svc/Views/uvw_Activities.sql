
CREATE VIEW [svc].[uvw_Activities]
AS
SELECT        Id, UserId, AudienceId, ActivityType, TableName, KeyValues, OriginalValues, CurrentValues, Created, Immutable
FROM            dbo.tbl_Activities
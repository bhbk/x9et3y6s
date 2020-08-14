
CREATE VIEW [svc].[uvw_Activity]
AS
SELECT        Id, AudienceId, UserId, ActivityType, TableName, KeyValues, OriginalValues, CurrentValues, Created, Immutable
FROM            dbo.tbl_Activity
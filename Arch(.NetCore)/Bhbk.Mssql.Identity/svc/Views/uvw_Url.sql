
CREATE VIEW [svc].[uvw_Url]
AS
SELECT        Id, AudienceId, ActorId, UrlHost, UrlPath, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_Url
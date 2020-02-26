
CREATE VIEW [svc].[uvw_Urls]
AS
SELECT        Id, AudienceId, ActorId, UrlHost, UrlPath, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_Urls

CREATE VIEW [svc].[uvw_Url]
AS
SELECT        Id, AudienceId, ActorId, UrlHost, UrlPath, IsEnabled, IsDeletable, CreatedUtc, LastUpdatedUtc
FROM            [dbo].[tbl_Url]
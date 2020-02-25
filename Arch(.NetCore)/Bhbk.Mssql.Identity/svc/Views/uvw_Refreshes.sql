
CREATE VIEW [svc].[uvw_Refreshes]
AS
SELECT        Id, IssuerId, AudienceId, UserId, RefreshValue, RefreshType, ValidFromUtc, ValidToUtc, IssuedUtc
FROM            dbo.tbl_Refreshes
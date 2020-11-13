
CREATE VIEW [svc].[uvw_Issuer]
AS
SELECT        Id, ActorId, Name, Description, IssuerKey, IsEnabled, IsDeletable, CreatedUtc, LastUpdatedUtc
FROM            [dbo].[tbl_Issuer]
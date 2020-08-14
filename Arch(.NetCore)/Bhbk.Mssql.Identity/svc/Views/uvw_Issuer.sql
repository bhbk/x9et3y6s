
CREATE VIEW [svc].[uvw_Issuer]
AS
SELECT        Id, ActorId, Name, Description, IssuerKey, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_Issuer
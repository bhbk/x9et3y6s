﻿
CREATE VIEW [svc].[uvw_Issuers]
AS
SELECT        Id, ActorId, Name, Description, IssuerKey, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_Issuers
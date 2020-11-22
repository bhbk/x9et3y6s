
CREATE VIEW [svc].[uvw_UserClaim]
AS
SELECT        UserId, ClaimId, IsDeletable, CreatedUtc
FROM            [dbo].[tbl_UserClaim]
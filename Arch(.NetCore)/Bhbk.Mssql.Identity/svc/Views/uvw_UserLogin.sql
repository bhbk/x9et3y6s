
CREATE VIEW [svc].[uvw_UserLogin]
AS
SELECT        UserId, LoginId, IsDeletable, CreatedUtc
FROM            [dbo].[tbl_UserLogin]


CREATE PROCEDURE [svc].[usp_Login_Delete]
    @LoginID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Logins] WHERE [svc].[uvw_Logins].Id = @LoginID

DELETE [dbo].[tbl_Logins]
WHERE Id = @LoginID

END
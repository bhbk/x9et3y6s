

CREATE PROCEDURE [svc].[usp_Login_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Logins] WHERE [svc].[uvw_Logins].Id = @ID

DELETE [dbo].[tbl_Logins]
WHERE Id = @ID

END
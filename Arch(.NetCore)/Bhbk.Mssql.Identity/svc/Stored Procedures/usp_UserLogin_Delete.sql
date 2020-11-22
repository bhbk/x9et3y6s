



CREATE   PROCEDURE [svc].[usp_UserLogin_Delete]
    @UserID uniqueidentifier
	,@LoginID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_UserLogin] 
			WHERE [svc].[uvw_UserLogin].UserId = @UserID AND [svc].[uvw_UserLogin].LoginId = @LoginID 

        DELETE [dbo].[tbl_UserLogin]
	        WHERE UserId = @UserID AND LoginId = @LoginID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
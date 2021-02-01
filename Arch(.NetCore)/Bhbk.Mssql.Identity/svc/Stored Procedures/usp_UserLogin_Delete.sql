
CREATE   PROCEDURE [svc].[usp_UserLogin_Delete]
    @UserId uniqueidentifier
	,@LoginId uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserLogin] 
			WHERE UserId = @UserId AND LoginId = @LoginId 

        DELETE [dbo].[tbl_UserLogin]
	        WHERE UserId = @UserId AND LoginId = @LoginId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
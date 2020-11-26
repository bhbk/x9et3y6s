
CREATE PROCEDURE [svc].[usp_User_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_User] 
            WHERE Id = @ID

        DELETE [dbo].[tbl_Activity]
            WHERE UserId = @ID

        DELETE [dbo].[tbl_Refresh]
            WHERE UserId = @ID

        DELETE [dbo].[tbl_Setting]
            WHERE UserId = @ID

        DELETE [dbo].[tbl_State]
            WHERE UserId = @ID

        DELETE [dbo].[tbl_User]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
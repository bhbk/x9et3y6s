

CREATE PROCEDURE [svc].[usp_User_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_User] 
            WHERE Id = @Id

        DELETE [dbo].[tbl_AuthActivity]
            WHERE UserId = @Id

        DELETE [dbo].[tbl_Refresh]
            WHERE UserId = @Id

        DELETE [dbo].[tbl_Setting]
            WHERE UserId = @Id

        DELETE [dbo].[tbl_State]
            WHERE UserId = @Id

        DELETE [dbo].[tbl_User]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE PROCEDURE [svc].[usp_Audience_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Audience]
            WHERE Id = @Id

        DELETE [dbo].[tbl_AuthActivity]
            WHERE AudienceId = @Id

        DELETE [dbo].[tbl_Refresh]
            WHERE AudienceId = @Id

        DELETE [dbo].[tbl_Setting]
            WHERE AudienceId = @Id

        DELETE [dbo].[tbl_State]
            WHERE AudienceId = @Id

        DELETE [dbo].[tbl_Role]
            WHERE AudienceId = @Id

        DELETE [dbo].[tbl_Audience]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
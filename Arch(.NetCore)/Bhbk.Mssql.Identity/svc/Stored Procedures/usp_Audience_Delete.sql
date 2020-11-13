
CREATE PROCEDURE [svc].[usp_Audience_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_Audience] WHERE [svc].[uvw_Audience].Id = @ID

        DELETE [dbo].[tbl_Activity]
        WHERE AudienceId = @ID

        DELETE [dbo].[tbl_Refresh]
        WHERE AudienceId = @ID

        DELETE [dbo].[tbl_Setting]
        WHERE AudienceId = @ID

        DELETE [dbo].[tbl_State]
        WHERE AudienceId = @ID

        DELETE [dbo].[tbl_Role]
        WHERE AudienceId = @ID

        DELETE [dbo].[tbl_Audience]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
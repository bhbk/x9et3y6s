

CREATE PROCEDURE [svc].[usp_MOTD_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_MOTD] WHERE [svc].[uvw_MOTD].Id = @ID

        DELETE [dbo].[tbl_MOTD]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
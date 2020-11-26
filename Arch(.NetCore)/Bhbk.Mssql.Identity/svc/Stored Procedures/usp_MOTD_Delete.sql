
CREATE PROCEDURE [svc].[usp_MOTD_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_MOTD]
            WHERE Id = @ID

        DELETE [dbo].[tbl_MOTD]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
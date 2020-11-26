
CREATE PROCEDURE [svc].[usp_TextQueue_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_TextQueue]
            WHERE Id = @ID

        DELETE [dbo].[tbl_TextQueue]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
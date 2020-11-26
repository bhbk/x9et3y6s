
CREATE PROCEDURE [svc].[usp_TextActivity_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_TextActivity]
            WHERE Id = @ID

        DELETE [dbo].[tbl_TextActivity]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
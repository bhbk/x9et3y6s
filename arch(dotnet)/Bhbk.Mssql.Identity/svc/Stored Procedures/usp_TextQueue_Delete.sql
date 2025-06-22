
CREATE PROCEDURE [svc].[usp_TextQueue_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_TextQueue]
            WHERE Id = @Id

        DELETE [dbo].[tbl_TextQueue]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
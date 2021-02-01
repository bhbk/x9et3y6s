
CREATE PROCEDURE [svc].[usp_TextActivity_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_TextActivity]
            WHERE Id = @Id

        DELETE [dbo].[tbl_TextActivity]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
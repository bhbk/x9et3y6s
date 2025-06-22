
CREATE PROCEDURE [svc].[usp_EmailActivity_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_EmailActivity]
            WHERE Id = @Id

        DELETE [dbo].[tbl_EmailActivity]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
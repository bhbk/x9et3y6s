
CREATE PROCEDURE [svc].[usp_EmailActivity_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_EmailActivity]
            WHERE Id = @ID

        DELETE [dbo].[tbl_EmailActivity]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
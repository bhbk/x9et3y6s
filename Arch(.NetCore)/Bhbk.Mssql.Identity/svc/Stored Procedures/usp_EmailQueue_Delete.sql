
CREATE PROCEDURE [svc].[usp_EmailQueue_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_EmailQueue]
            WHERE Id = @ID

        DELETE [dbo].[tbl_EmailQueue]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
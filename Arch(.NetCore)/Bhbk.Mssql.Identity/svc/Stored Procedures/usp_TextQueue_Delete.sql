

CREATE PROCEDURE [svc].[usp_TextQueue_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_TextQueue] WHERE [svc].[uvw_TextQueue].Id = @ID

        DELETE [dbo].[tbl_TextQueue]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
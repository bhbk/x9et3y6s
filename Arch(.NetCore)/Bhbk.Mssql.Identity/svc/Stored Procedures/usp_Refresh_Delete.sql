
CREATE PROCEDURE [svc].[usp_Refresh_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Refresh]
            WHERE Id = @ID

        DELETE [dbo].[tbl_Refresh]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
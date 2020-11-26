
CREATE PROCEDURE [svc].[usp_Url_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Url]
            WHERE Id = @ID

        DELETE [dbo].[tbl_Url]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
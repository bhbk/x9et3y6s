
CREATE PROCEDURE [svc].[usp_Setting_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Setting]
            WHERE Id = @ID

        DELETE [dbo].[tbl_Setting]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
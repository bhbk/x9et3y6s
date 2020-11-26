
CREATE PROCEDURE [svc].[usp_State_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_State]
            WHERE Id = @ID

        DELETE [dbo].[tbl_State]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE PROCEDURE [svc].[usp_Activity_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_Activity] WHERE [svc].[uvw_Activity].Id = @ID

        DELETE [dbo].[tbl_Activity]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
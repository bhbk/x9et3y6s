
CREATE PROCEDURE [svc].[usp_Refresh_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Refresh]
            WHERE Id = @Id

        DELETE [dbo].[tbl_Refresh]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
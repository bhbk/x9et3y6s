
CREATE PROCEDURE [svc].[usp_EmailQueue_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_EmailQueue]
            WHERE Id = @Id

        DELETE [dbo].[tbl_EmailQueue]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
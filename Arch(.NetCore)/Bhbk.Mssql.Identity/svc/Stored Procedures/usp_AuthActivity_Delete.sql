

CREATE PROCEDURE [svc].[usp_AuthActivity_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_AuthActivity]
            WHERE Id = @Id

        DELETE [dbo].[tbl_AuthActivity]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
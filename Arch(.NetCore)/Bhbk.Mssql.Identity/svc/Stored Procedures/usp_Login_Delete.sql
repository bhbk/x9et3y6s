
CREATE PROCEDURE [svc].[usp_Login_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Login]
            WHERE Id = @Id

        DELETE [dbo].[tbl_Login]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
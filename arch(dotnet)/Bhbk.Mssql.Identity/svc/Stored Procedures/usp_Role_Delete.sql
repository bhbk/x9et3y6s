
CREATE PROCEDURE [svc].[usp_Role_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Role]
            WHERE Id = @Id

        DELETE [dbo].[tbl_Role]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
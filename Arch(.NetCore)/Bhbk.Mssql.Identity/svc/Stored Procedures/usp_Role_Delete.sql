
CREATE PROCEDURE [svc].[usp_Role_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_Role] WHERE [svc].[uvw_Role].Id = @ID

        DELETE [dbo].[tbl_Role]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
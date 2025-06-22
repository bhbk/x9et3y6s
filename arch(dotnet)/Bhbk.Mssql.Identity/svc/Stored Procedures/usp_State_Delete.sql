
CREATE PROCEDURE [svc].[usp_State_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_State]
            WHERE Id = @Id

        DELETE [dbo].[tbl_State]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
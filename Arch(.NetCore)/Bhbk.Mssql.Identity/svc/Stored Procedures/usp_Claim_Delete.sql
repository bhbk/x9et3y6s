
CREATE PROCEDURE [svc].[usp_Claim_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Claim]
            WHERE Id = @ID

        DELETE [dbo].[tbl_Claim]
            WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
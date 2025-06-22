
CREATE PROCEDURE [svc].[usp_Claim_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Claim]
            WHERE Id = @Id

        DELETE [dbo].[tbl_Claim]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
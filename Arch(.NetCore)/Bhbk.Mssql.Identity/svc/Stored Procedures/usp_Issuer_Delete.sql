
CREATE PROCEDURE [svc].[usp_Issuer_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		SELECT * FROM [dbo].[tbl_Issuer]
			WHERE Id = @ID

		DECLARE @AUDIENCEID uniqueidentifier = 
			(SELECT TOP(1) Id FROM [dbo].[tbl_Audience] 
				WHERE IssuerId = @ID)

        DELETE [dbo].[tbl_Activity]
	        WHERE AudienceId = @AUDIENCEID

		DELETE [dbo].[tbl_Claim]
			WHERE IssuerId = @ID

		DELETE [dbo].[tbl_Refresh]
			WHERE IssuerId = @ID

		DELETE [dbo].[tbl_Setting]
			WHERE IssuerId = @ID

		DELETE [dbo].[tbl_State]
			WHERE IssuerId = @ID

		DELETE [dbo].[tbl_Role]
			WHERE AudienceId = @AUDIENCEID

		DELETE [dbo].[tbl_Audience]
			WHERE IssuerId = @ID

		DELETE [dbo].[tbl_Issuer]
			WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
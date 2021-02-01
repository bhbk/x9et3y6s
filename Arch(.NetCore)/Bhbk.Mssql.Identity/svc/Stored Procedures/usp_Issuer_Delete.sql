
CREATE PROCEDURE [svc].[usp_Issuer_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		SELECT * FROM [dbo].[tbl_Issuer]
			WHERE Id = @Id

		DECLARE @AUDIENCEID uniqueidentifier = 
			(SELECT TOP(1) Id FROM [dbo].[tbl_Audience] 
				WHERE IssuerId = @Id)

        DELETE [dbo].[tbl_AuthActivity]
	        WHERE AudienceId = @AUDIENCEID

		DELETE [dbo].[tbl_Claim]
			WHERE IssuerId = @Id

		DELETE [dbo].[tbl_Refresh]
			WHERE IssuerId = @Id

		DELETE [dbo].[tbl_Setting]
			WHERE IssuerId = @Id

		DELETE [dbo].[tbl_State]
			WHERE IssuerId = @Id

		DELETE [dbo].[tbl_Role]
			WHERE AudienceId = @AUDIENCEID

		DELETE [dbo].[tbl_Audience]
			WHERE IssuerId = @Id

		DELETE [dbo].[tbl_Issuer]
			WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
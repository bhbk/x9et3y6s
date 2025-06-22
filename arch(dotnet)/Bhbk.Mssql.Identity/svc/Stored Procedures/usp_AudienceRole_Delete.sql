
CREATE   PROCEDURE [svc].[usp_AudienceRole_Delete]
    @AudienceId uniqueidentifier
	,@RoleId uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_AudienceRole] 
			WHERE AudienceId = @AudienceId AND RoleId = @RoleId 

        DELETE [dbo].[tbl_AudienceRole]
	        WHERE AudienceId = @AudienceId AND RoleId = @RoleId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
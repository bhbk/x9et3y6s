


CREATE   PROCEDURE [svc].[usp_AudienceRole_Delete]
    @AudienceID uniqueidentifier
	,@RoleID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_AudienceRole] 
			WHERE [svc].[uvw_AudienceRole].AudienceId = @AudienceID AND [svc].[uvw_AudienceRole].RoleId = @RoleID 

        DELETE [dbo].[tbl_AudienceRole]
	        WHERE AudienceId = @AudienceID AND RoleId = @RoleID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
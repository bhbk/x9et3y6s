


CREATE   PROCEDURE [svc].[usp_UserRole_Delete]
    @UserID uniqueidentifier
	,@RoleID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_UserRole] 
			WHERE [svc].[uvw_UserRole].UserId = @UserID AND [svc].[uvw_UserRole].RoleId = @RoleID 

        DELETE [dbo].[tbl_UserRole]
	        WHERE UserId = @UserID AND RoleId = @RoleID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE   PROCEDURE [svc].[usp_UserRole_Delete]
    @UserId uniqueidentifier
	,@RoleId uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserRole] 
			WHERE UserId = @UserId AND RoleId = @RoleId 

        DELETE [dbo].[tbl_UserRole]
	        WHERE UserId = @UserId AND RoleId = @RoleId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
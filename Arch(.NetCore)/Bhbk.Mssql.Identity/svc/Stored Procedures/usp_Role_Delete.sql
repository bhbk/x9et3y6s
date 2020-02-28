
CREATE PROCEDURE [svc].[usp_Role_Delete]
    @RoleID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Roles] WHERE [svc].[uvw_Roles].Id = @RoleID

DELETE [dbo].[tbl_Roles]
WHERE Id = @RoleID

END
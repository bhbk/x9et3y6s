
CREATE PROCEDURE [svc].[usp_Role_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Roles] WHERE [svc].[uvw_Roles].Id = @ID

DELETE [dbo].[tbl_Roles]
WHERE Id = @ID

END
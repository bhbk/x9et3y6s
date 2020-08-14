
CREATE PROCEDURE [svc].[usp_Role_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Role] WHERE [svc].[uvw_Role].Id = @ID

DELETE [dbo].[tbl_Role]
WHERE Id = @ID

END
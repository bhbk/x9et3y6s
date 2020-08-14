
CREATE PROCEDURE [svc].[usp_Login_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Login] WHERE [svc].[uvw_Login].Id = @ID

DELETE [dbo].[tbl_Login]
WHERE Id = @ID

END
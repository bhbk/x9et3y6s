
CREATE PROCEDURE [svc].[usp_User_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_User] WHERE [svc].[uvw_User].Id = @ID

DELETE [dbo].[tbl_Activity]
WHERE UserId = @ID

DELETE [dbo].[tbl_Refresh]
WHERE UserId = @ID

DELETE [dbo].[tbl_Setting]
WHERE UserId = @ID

DELETE [dbo].[tbl_State]
WHERE UserId = @ID

DELETE [dbo].[tbl_User]
WHERE Id = @ID

END
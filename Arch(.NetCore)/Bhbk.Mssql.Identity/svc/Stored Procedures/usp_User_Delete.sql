
CREATE PROCEDURE [svc].usp_User_Delete
    @UserID uniqueidentifier

AS

DELETE [dbo].[tbl_Users]
WHERE Id = @UserID
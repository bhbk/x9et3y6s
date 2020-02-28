


CREATE PROCEDURE [svc].[usp_User_Delete]
    @UserID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Users] WHERE [svc].[uvw_Users].Id = @UserID

DELETE [dbo].[tbl_Activities]
WHERE UserId = @UserID

DELETE [dbo].[tbl_Refreshes]
WHERE UserId = @UserID

DELETE [dbo].[tbl_Settings]
WHERE UserId = @UserID

DELETE [dbo].[tbl_States]
WHERE UserId = @UserID

DELETE [dbo].[tbl_Users]
WHERE Id = @UserID

END
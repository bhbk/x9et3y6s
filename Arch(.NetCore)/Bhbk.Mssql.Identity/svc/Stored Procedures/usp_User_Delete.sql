


CREATE PROCEDURE [svc].[usp_User_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Users] WHERE [svc].[uvw_Users].Id = @ID

DELETE [dbo].[tbl_Activities]
WHERE UserId = @ID

DELETE [dbo].[tbl_Refreshes]
WHERE UserId = @ID

DELETE [dbo].[tbl_Settings]
WHERE UserId = @ID

DELETE [dbo].[tbl_States]
WHERE UserId = @ID

DELETE [dbo].[tbl_Users]
WHERE Id = @ID

END

CREATE PROCEDURE [svc].[usp_Audience_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Audiences] WHERE [svc].[uvw_Audiences].Id = @ID

DELETE [dbo].[tbl_Activities]
WHERE AudienceId = @ID

DELETE [dbo].[tbl_Refreshes]
WHERE AudienceId = @ID

DELETE [dbo].[tbl_Settings]
WHERE AudienceId = @ID

DELETE [dbo].[tbl_States]
WHERE AudienceId = @ID

DELETE [dbo].[tbl_Roles]
WHERE AudienceId = @ID

DELETE [dbo].[tbl_Audiences]
WHERE Id = @ID

END
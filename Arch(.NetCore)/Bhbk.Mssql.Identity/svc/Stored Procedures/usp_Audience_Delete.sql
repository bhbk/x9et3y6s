





CREATE PROCEDURE [svc].[usp_Audience_Delete]
    @AudienceID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Audiences] WHERE [svc].[uvw_Audiences].Id = @AudienceID

DELETE [dbo].[tbl_Activities]
WHERE AudienceId = @AudienceID

DELETE [dbo].[tbl_Refreshes]
WHERE AudienceId = @AudienceID

DELETE [dbo].[tbl_Settings]
WHERE AudienceId = @AudienceID

DELETE [dbo].[tbl_States]
WHERE AudienceId = @AudienceID

DELETE [dbo].[tbl_Roles]
WHERE AudienceId = @AudienceID

DELETE [dbo].[tbl_Audiences]
WHERE Id = @AudienceID

END



CREATE PROCEDURE [svc].[usp_Audience_Delete]
    @AudienceID uniqueidentifier

AS

DELETE [dbo].[tbl_Audiences]
WHERE Id = @AudienceID
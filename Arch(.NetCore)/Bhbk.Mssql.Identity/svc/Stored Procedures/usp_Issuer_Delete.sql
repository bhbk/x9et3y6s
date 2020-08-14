
CREATE PROCEDURE [svc].[usp_Issuer_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Issuer] WHERE [svc].[uvw_Issuer].Id = @ID

DELETE [dbo].[tbl_Claim]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_Refresh]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_Setting]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_State]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_Audience]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_Issuer]
WHERE Id = @ID

END





CREATE PROCEDURE [svc].[usp_Issuer_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Issuers] WHERE [svc].[uvw_Issuers].Id = @ID

DELETE [dbo].[tbl_Claims]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_Refreshes]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_Settings]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_States]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_Audiences]
WHERE IssuerId = @ID

DELETE [dbo].[tbl_Issuers]
WHERE Id = @ID

END
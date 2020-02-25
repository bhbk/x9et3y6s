




CREATE PROCEDURE [svc].[usp_Issuer_Delete]
    @IssuerID uniqueidentifier

AS
BEGIN

DELETE [dbo].[tbl_Claims]
WHERE IssuerId = @IssuerID

DELETE [dbo].[tbl_Refreshes]
WHERE IssuerId = @IssuerID

DELETE [dbo].[tbl_Settings]
WHERE IssuerId = @IssuerID

DELETE [dbo].[tbl_States]
WHERE IssuerId = @IssuerID

DELETE [dbo].[tbl_Audiences]
WHERE IssuerId = @IssuerID

DELETE [dbo].[tbl_Issuers]
WHERE Id = @IssuerID

END
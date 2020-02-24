


CREATE PROCEDURE [svc].[usp_Issuer_Delete]
    @IssuerID uniqueidentifier

AS

DELETE [dbo].[tbl_Issuers]
WHERE Id = @IssuerID
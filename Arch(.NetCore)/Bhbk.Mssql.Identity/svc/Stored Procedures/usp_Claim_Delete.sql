

CREATE PROCEDURE [svc].[usp_Claim_Delete]
    @ClaimID uniqueidentifier

AS

DELETE [dbo].[tbl_Claims]
WHERE Id = @ClaimID
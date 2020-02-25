


CREATE PROCEDURE [svc].[usp_Claim_Delete]
    @ClaimID uniqueidentifier

AS
BEGIN

DELETE [dbo].[tbl_Claims]
WHERE Id = @ClaimID

END
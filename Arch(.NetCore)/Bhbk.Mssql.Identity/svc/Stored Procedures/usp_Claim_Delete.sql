


CREATE PROCEDURE [svc].[usp_Claim_Delete]
    @ClaimID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Claims] WHERE [svc].[uvw_Claims].Id = @ClaimID

DELETE [dbo].[tbl_Claims]
WHERE Id = @ClaimID

END
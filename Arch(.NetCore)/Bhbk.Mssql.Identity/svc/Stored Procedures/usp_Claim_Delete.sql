
CREATE PROCEDURE [svc].[usp_Claim_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Claim] WHERE [svc].[uvw_Claim].Id = @ID

DELETE [dbo].[tbl_Claim]
WHERE Id = @ID

END
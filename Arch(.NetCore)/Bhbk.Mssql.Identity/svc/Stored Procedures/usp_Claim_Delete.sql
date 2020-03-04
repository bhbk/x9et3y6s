


CREATE PROCEDURE [svc].[usp_Claim_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Claims] WHERE [svc].[uvw_Claims].Id = @ID

DELETE [dbo].[tbl_Claims]
WHERE Id = @ID

END




CREATE   PROCEDURE [svc].[usp_UserClaim_Delete]
    @UserID uniqueidentifier
	,@ClaimID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_UserClaim] 
			WHERE [svc].[uvw_UserClaim].UserId = @UserID AND [svc].[uvw_UserClaim].ClaimId = @ClaimID 

        DELETE [dbo].[tbl_UserClaim]
	        WHERE UserId = @UserID AND ClaimId = @ClaimID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
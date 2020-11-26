
CREATE   PROCEDURE [svc].[usp_UserClaim_Delete]
    @UserID uniqueidentifier
	,@ClaimID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserClaim] 
			WHERE UserId = @UserID AND ClaimId = @ClaimID 

        DELETE [dbo].[tbl_UserClaim]
	        WHERE UserId = @UserID AND ClaimId = @ClaimID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
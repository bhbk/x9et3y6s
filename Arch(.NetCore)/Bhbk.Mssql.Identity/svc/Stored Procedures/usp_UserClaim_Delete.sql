
CREATE   PROCEDURE [svc].[usp_UserClaim_Delete]
    @UserId uniqueidentifier
	,@ClaimId uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_UserClaim] 
			WHERE UserId = @UserId AND ClaimId = @ClaimId 

        DELETE [dbo].[tbl_UserClaim]
	        WHERE UserId = @UserId AND ClaimId = @ClaimId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
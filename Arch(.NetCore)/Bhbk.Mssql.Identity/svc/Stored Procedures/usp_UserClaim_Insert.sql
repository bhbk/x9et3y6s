
CREATE   PROCEDURE [svc].[usp_UserClaim_Insert]
     @UserId			UNIQUEIDENTIFIER 
    ,@ClaimId				UNIQUEIDENTIFIER
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_UserClaim]
	        (
             UserId         
	        ,ClaimId
            ,IsDeletable        
            ,CreatedUtc           
	        )
        VALUES
	        (
             @UserId          
	        ,@ClaimId
            ,@IsDeletable        
            ,@CREATEDUTC         
	        );

        SELECT * FROM [svc].[uvw_UserClaim] 
			WHERE [svc].[uvw_UserClaim].UserId = @UserID AND [svc].[uvw_UserClaim].ClaimId = @ClaimID 

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

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

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_UserClaim] 
			WHERE UserId = @UserID AND ClaimId = @ClaimID 

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE   PROCEDURE [svc].[usp_UserLogin_Insert]
     @UserId			UNIQUEIDENTIFIER 
    ,@LoginId				UNIQUEIDENTIFIER
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_UserLogin]
	        (
             UserId         
	        ,LoginId
            ,IsDeletable        
            ,CreatedUtc           
	        )
        VALUES
	        (
             @UserId          
	        ,@LoginId
            ,@IsDeletable        
            ,@CREATEDUTC         
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_UserLogin] 
			WHERE UserId = @UserId AND LoginId = @LoginId 

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END


CREATE   PROCEDURE [svc].[usp_UserLogin_Insert]
     @UserId			UNIQUEIDENTIFIER 
    ,@LoginId				UNIQUEIDENTIFIER
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

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

        SELECT * FROM [svc].[uvw_UserLogin] 
			WHERE [svc].[uvw_UserLogin].UserId = @UserID AND [svc].[uvw_UserLogin].LoginId = @LoginID 

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
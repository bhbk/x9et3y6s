
CREATE   PROCEDURE [svc].[usp_UserRole_Insert]
     @UserId			UNIQUEIDENTIFIER 
    ,@RoleId				UNIQUEIDENTIFIER
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_UserRole]
	        (
             UserId         
	        ,RoleId
            ,IsDeletable        
            ,CreatedUtc           
	        )
        VALUES
	        (
             @UserId          
	        ,@RoleId
            ,@IsDeletable        
            ,@CREATEDUTC         
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_UserRole] 
			WHERE UserId = @UserId AND RoleId = @RoleId 

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

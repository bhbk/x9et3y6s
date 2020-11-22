
CREATE   PROCEDURE [svc].[usp_UserRole_Insert]
     @UserId			UNIQUEIDENTIFIER 
    ,@RoleId				UNIQUEIDENTIFIER
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

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

        SELECT * FROM [svc].[uvw_UserRole] 
			WHERE [svc].[uvw_UserRole].UserId = @UserID AND [svc].[uvw_UserRole].RoleId = @RoleID 

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE PROCEDURE [svc].[usp_Role_Insert]
	 @AudienceId			UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @ROLEID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_Role]
	        (
             Id           
	        ,AudienceId
            ,ActorId    
            ,Name           
            ,Description       
            ,IsEnabled     
            ,IsDeletable        
            ,CreatedUtc           
	        )
        VALUES
	        (
             @ROLEID         
	        ,@AudienceId
            ,@ActorId    
            ,@Name           
            ,@Description       
            ,@IsEnabled     
            ,@IsDeletable        
            ,@CREATEDUTC         
	        );

        SELECT * FROM [svc].[uvw_Role] WHERE [svc].[uvw_Role].Id = @ROLEID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
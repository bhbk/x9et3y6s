
CREATE PROCEDURE [svc].[usp_Login_Insert]
    @ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@LoginKey				NVARCHAR (MAX)
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LOGINID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_Login]
	        (
             Id           
            ,ActorId    
            ,Name           
            ,Description
	        ,LoginKey
            ,IsEnabled     
            ,CreatedUtc           
            ,IsDeletable        
	        )
        VALUES
	        (
             @LOGINID         
            ,@ActorId    
            ,@Name           
            ,@Description       
	        ,@LoginKey
            ,@IsEnabled     
            ,@CREATEDUTC       
            ,@IsDeletable        
	        );

        SELECT * FROM [svc].[uvw_Login] WHERE [svc].[uvw_Login].Id = @LOGINID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
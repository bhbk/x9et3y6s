

CREATE PROCEDURE [svc].[usp_EmailQueue_Insert]
     @ActorId				UNIQUEIDENTIFIER
    ,@FromId				UNIQUEIDENTIFIER
    ,@FromEmail             NVARCHAR (MAX) 
    ,@FromDisplay           NVARCHAR (MAX) 
    ,@ToId					UNIQUEIDENTIFIER
    ,@ToEmail               NVARCHAR (MAX) 
    ,@ToDisplay             NVARCHAR (MAX) 
    ,@Subject               NVARCHAR (MAX) 
    ,@Body      			NVARCHAR (MAX)
    ,@SendAtUtc             DATETIMEOFFSET (7) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @EMAILID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_EmailQueue]
	        (
             Id           
            ,ActorId    
            ,FromId    
			,FromEmail
			,FromDisplay
			,ToId
			,ToEmail
			,ToDisplay
            ,Subject           
            ,Body 
			,CreatedUtc
            ,SendAtUtc           
	        )
        VALUES
	        (
             @EMAILID          
            ,@ActorId    
            ,@FromId    
            ,@FromEmail           
            ,@FromDisplay  
            ,@ToId 
            ,@ToEmail
			,@ToDisplay
			,@Subject
			,@Body
            ,@CREATEDUTC           
            ,@SendAtUtc        
	        );

        SELECT * FROM [svc].[uvw_EmailQueue] WHERE [svc].[uvw_EmailQueue].Id = @EMAILID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
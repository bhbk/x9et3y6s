

CREATE PROCEDURE [svc].[usp_TextQueue_Insert]
    @FromId		    		UNIQUEIDENTIFIER
    ,@FromPhoneNumber       NVARCHAR (MAX) 
    ,@ToId					UNIQUEIDENTIFIER
    ,@ToPhoneNumber         NVARCHAR (MAX) 
    ,@Body					NVARCHAR (MAX)
    ,@SendAtUtc             DATETIMEOFFSET (7) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @TEXTID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_TextQueue]
	        (
             Id           
            ,FromId    
			,FromPhoneNumber
			,ToId
			,ToPhoneNumber
            ,Body 
			,CreatedUtc
            ,SendAtUtc           
	        )
        VALUES
	        (
             @TEXTID          
            ,@FromId    
            ,@FromPhoneNumber       
            ,@ToId 
            ,@ToPhoneNumber
			,@Body
            ,@CREATEDUTC           
            ,@SendAtUtc        
	        );

        SELECT * FROM [svc].[uvw_TextQueue] WHERE [svc].[uvw_TextQueue].Id = @TEXTID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
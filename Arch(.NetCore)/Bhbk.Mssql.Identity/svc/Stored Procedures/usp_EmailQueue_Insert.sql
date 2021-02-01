
CREATE PROCEDURE [svc].[usp_EmailQueue_Insert]
    @FromEmail             NVARCHAR (320) 
    ,@FromDisplay           NVARCHAR (512) 
    ,@ToEmail               NVARCHAR (320) 
    ,@ToDisplay             NVARCHAR (512) 
    ,@Subject               NVARCHAR (1024) 
    ,@Body      			NVARCHAR (MAX)
    ,@SendAtUtc             DATETIMEOFFSET (7) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @EMAILID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_EmailQueue]
	        (
             Id           
			,FromEmail
			,FromDisplay
			,ToEmail
			,ToDisplay
            ,Subject           
            ,Body 
			,IsCancelled
			,CreatedUtc
            ,SendAtUtc           
	        )
        VALUES
	        (
             @EMAILID          
            ,@FromEmail           
            ,@FromDisplay  
            ,@ToEmail
			,@ToDisplay
			,@Subject
			,@Body
			,'FALSE'
            ,@CREATEDUTC           
            ,@SendAtUtc        
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_EmailQueue]
            WHERE Id = @EMAILID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

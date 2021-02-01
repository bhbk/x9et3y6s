
CREATE PROCEDURE [svc].[usp_TextActivity_Insert]
     @TextId				UNIQUEIDENTIFIER
    ,@TwilioSid	            NVARCHAR (50) 
    ,@TwilioStatus			NVARCHAR (10) 
    ,@TwilioMessage			NVARCHAR (500) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @ACTIVITYID UNIQUEIDENTIFIER = NEWID()
        DECLARE @STATUSATUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_TextActivity]
	        (
             Id           
            ,TextId    
            ,TwilioSid    
			,TwilioStatus
			,TwilioMessage
			,StatusAtUtc
	        )
        VALUES
	        (
             @ACTIVITYID          
            ,@TextId    
            ,@TwilioSid
            ,@TwilioStatus
			,@TwilioMessage
            ,@STATUSATUTC 
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_TextActivity] 
            WHERE Id = @ACTIVITYID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

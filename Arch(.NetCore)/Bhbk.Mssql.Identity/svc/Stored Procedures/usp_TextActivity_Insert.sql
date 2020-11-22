


CREATE PROCEDURE [svc].[usp_TextActivity_Insert]
     @TextId				UNIQUEIDENTIFIER
    ,@TwilioSid	            NVARCHAR (50) 
    ,@TwilioStatus			NVARCHAR (10) 
    ,@TwilioMessage			NVARCHAR (500) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

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

        SELECT * FROM [svc].[uvw_TextActivity] WHERE [svc].[uvw_TextActivity].Id = @ACTIVITYID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
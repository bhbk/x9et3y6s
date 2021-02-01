
CREATE PROCEDURE [svc].[usp_AuthActivity_Insert]
     @AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@LoginType             NVARCHAR (64) 
    ,@LoginOutcome          NVARCHAR (64) 
    ,@LocalEndpoint			NVARCHAR (128)
    ,@RemoteEndpoint		NVARCHAR (128) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @ACTIVITYID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_AuthActivity]
	        (
             Id           
            ,AudienceId    
            ,UserId    
            ,LoginType           
            ,LoginOutcome
            ,LocalEndpoint
            ,RemoteEndpoint   
            ,CreatedUtc    
	        )
        VALUES
	        (
             @ACTIVITYID         
            ,@AudienceId    
            ,@UserId    
            ,@LoginType
            ,@LoginOutcome
	        ,@LocalEndpoint
            ,@RemoteEndpoint 
            ,@CREATEDUTC        
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_AuthActivity]
            WHERE Id = @ACTIVITYID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END
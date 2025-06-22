
CREATE PROCEDURE [svc].[usp_Url_Insert]
	@AudienceId				UNIQUEIDENTIFIER
    ,@UrlHost				NVARCHAR (1024) 
    ,@UrlPath				NVARCHAR (1024) 
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @URLID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_Url]
	        (
             Id           
	        ,AudienceId
            ,UrlHost 
            ,UrlPath       
            ,IsEnabled     
			,IsDeletable
            ,CreatedUtc           
	        )
        VALUES
	        (
             @URLID         
	        ,@AudienceId
            ,@UrlHost
            ,@UrlPath       
            ,@IsEnabled     
            ,@IsDeletable        
            ,@CREATEDUTC         
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Url]
            WHERE Id = @URLID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

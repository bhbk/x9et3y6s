

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

        SELECT * FROM [svc].[uvw_Url] WHERE [svc].[uvw_Url].Id = @URLID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
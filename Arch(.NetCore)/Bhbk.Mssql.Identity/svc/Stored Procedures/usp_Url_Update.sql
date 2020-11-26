
CREATE PROCEDURE [svc].[usp_Url_Update]
     @Id					UNIQUEIDENTIFIER 
	,@AudienceId			UNIQUEIDENTIFIER
    ,@UrlHost				NVARCHAR (1024) 
    ,@UrlPath				NVARCHAR (1024) 
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Url]
        SET
             Id						= @Id
	        ,AudienceId				= @AudienceId
	        ,UrlHost				= @UrlHost
	        ,UrlPath				= @UrlPath
	        ,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Url] 
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
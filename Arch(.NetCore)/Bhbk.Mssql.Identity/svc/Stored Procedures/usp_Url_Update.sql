

CREATE PROCEDURE [svc].[usp_Url_Update]
     @Id					UNIQUEIDENTIFIER 
	,@AudienceId			UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@UrlHost				NVARCHAR (MAX) 
    ,@UrlPath				NVARCHAR (MAX) 
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
            ,ActorId				= @ActorId
	        ,UrlHost				= @UrlHost
	        ,UrlPath				= @UrlPath
	        ,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_Url] WHERE [svc].[uvw_Url].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
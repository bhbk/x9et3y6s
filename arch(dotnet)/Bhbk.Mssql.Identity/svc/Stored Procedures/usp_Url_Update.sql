
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

    	BEGIN TRANSACTION;

        UPDATE [dbo].[tbl_Url]
        SET
             Id						= @Id
	        ,AudienceId				= @AudienceId
	        ,UrlHost				= @UrlHost
	        ,UrlPath				= @UrlPath
	        ,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Url] 
            WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

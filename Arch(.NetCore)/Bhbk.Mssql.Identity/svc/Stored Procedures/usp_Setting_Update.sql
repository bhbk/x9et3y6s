
CREATE PROCEDURE [svc].[usp_Setting_Update]
     @Id					UNIQUEIDENTIFIER 
	,@IssuerId				UNIQUEIDENTIFIER
	,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@ConfigKey				NVARCHAR (128) 
    ,@ConfigValue			NVARCHAR (1024) 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        UPDATE [dbo].[tbl_Setting]
        SET
             Id						= @Id
	        ,IssuerId				= @IssuerId
	        ,AudienceId				= @AudienceId
            ,UserId					= @UserId
	        ,ConfigKey				= @ConfigKey
	        ,ConfigValue			= @ConfigValue
            ,IsDeletable			= @IsDeletable
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_Setting] WHERE [svc].[uvw_Setting].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE PROCEDURE [svc].[usp_Setting_Insert]
     @IssuerId				UNIQUEIDENTIFIER
	,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@ConfigKey				NVARCHAR (128) 
    ,@ConfigValue			NVARCHAR (1024) 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @SETTINGID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Setting]
			(
			 Id         
			,IssuerId
			,AudienceId    
			,UserId           
			,ConfigKey   
			,ConfigValue
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			 @SETTINGID          
			,@IssuerId
			,@AudienceId   
			,@UserId         
			,@ConfigKey
			,@ConfigValue
			,@IsDeletable
			,@CREATEDUTC
			);

		SELECT * FROM [svc].[uvw_Setting] WHERE [svc].[uvw_Setting].Id = @SETTINGID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
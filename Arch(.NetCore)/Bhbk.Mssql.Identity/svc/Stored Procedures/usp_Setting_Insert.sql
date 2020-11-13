

CREATE PROCEDURE [svc].[usp_Setting_Insert]
     @IssuerId				UNIQUEIDENTIFIER
	,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@ConfigKey				NVARCHAR (MAX) 
    ,@ConfigValue			NVARCHAR (MAX) 
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
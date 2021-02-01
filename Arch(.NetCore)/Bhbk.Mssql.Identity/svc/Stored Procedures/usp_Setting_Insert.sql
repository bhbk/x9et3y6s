
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

    	BEGIN TRANSACTION;

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

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_Setting]
			WHERE Id = @SETTINGID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

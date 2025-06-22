
CREATE PROCEDURE [svc].[usp_State_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@StateValue			NVARCHAR (2048) 
    ,@StateType				NVARCHAR (64)
    ,@StateDecision			BIT
    ,@StateConsume			BIT
    ,@IssuedUtc				DATETIMEOFFSET (7)
    ,@ValidFromUtc			DATETIMEOFFSET (7)
    ,@ValidToUtc			DATETIMEOFFSET (7)
    ,@LastPollingUtc		DATETIMEOFFSET (7)

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

		DECLARE @STATEID UNIQUEIDENTIFIER = NEWID()

		INSERT INTO [dbo].[tbl_State]
			(
			 Id         
			,IssuerId
			,AudienceId    
			,UserId           
			,StateValue   
			,StateType
			,StateDecision
			,StateConsume
			,IssuedUtc
			,ValidFromUtc
			,ValidToUtc
			,LastPollingUtc
			)
		VALUES
			(
			 @STATEID          
			,@IssuerId
			,@AudienceId   
			,@UserId         
			,@StateValue       
			,@StateType
			,@StateDecision
			,@StateConsume
			,@IssuedUtc
			,@ValidFromUtc
			,@ValidToUtc
			,@LastPollingUtc
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_State]
			WHERE Id = @STATEID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

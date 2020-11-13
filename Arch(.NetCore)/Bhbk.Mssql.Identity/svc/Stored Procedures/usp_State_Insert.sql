
CREATE PROCEDURE [svc].[usp_State_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@StateValue			NVARCHAR (512) 
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

		SELECT * FROM [svc].[uvw_State] WHERE [svc].[uvw_State].Id = @STATEID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
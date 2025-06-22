
CREATE PROCEDURE [svc].[usp_State_Update]
	 @Id					UNIQUEIDENTIFIER
    ,@IssuerId				UNIQUEIDENTIFIER
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

        UPDATE [dbo].[tbl_State]
        SET
             IssuerId				= @IssuerId
            ,AudienceId				= @AudienceId
            ,UserId					= @UserId
	        ,StateValue				= @StateValue
	        ,StateType				= @StateType
	        ,StateDecision			= @StateDecision
	        ,StateConsume			= @StateConsume
            ,IssuedUtc				= @IssuedUtc
            ,ValidFromUtc			= @ValidFromUtc
            ,ValidToUtc				= @ValidToUtc
            ,LastPollingUtc			= @LastPollingUtc
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_State] 
            WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END


CREATE PROCEDURE [svc].[usp_State_Update]
	 @Id					UNIQUEIDENTIFIER
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@StateValue			NVARCHAR (512) 
    ,@StateType				NVARCHAR (64)
    ,@StateDecision			BIT
    ,@StateConsume			BIT
    ,@IssuedUtc				DATETIME2 (7) 
    ,@ValidFromUtc			DATETIME2 (7)
    ,@ValidToUtc			DATETIME2 (7) 
    ,@LastPolling			DATETIME2 (7) 

AS
BEGIN

UPDATE [dbo].[tbl_States]
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
    ,LastPolling			= @LastPolling
WHERE Id = @Id

SELECT * FROM [svc].[uvw_States] WHERE [svc].[uvw_States].Id = @Id

END
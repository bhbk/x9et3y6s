



CREATE PROCEDURE [svc].[usp_Claim_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (MAX) 
    ,@Type					NVARCHAR (MAX)
    ,@Value					NVARCHAR (MAX) 
    ,@ValueType             NVARCHAR (64) 
    ,@LastUpdated			DATETIME2 (7)
    ,@Immutable				BIT

AS
BEGIN

UPDATE [dbo].[tbl_Claims]
SET
     Id						= @Id
	,IssuerId				= @IssuerId
    ,ActorId				= @ActorId
	,Subject				= @Subject
	,Type					= @Type
	,Value					= @Value
	,ValueType				= @ValueType
    ,LastUpdated			= @LastUpdated
    ,Immutable				= @Immutable
WHERE Id = @Id

END
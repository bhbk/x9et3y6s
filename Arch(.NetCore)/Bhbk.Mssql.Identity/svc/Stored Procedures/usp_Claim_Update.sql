

CREATE PROCEDURE [svc].[usp_Claim_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (MAX) 
    ,@Type					NVARCHAR (MAX)
    ,@Value					NVARCHAR (MAX) 
    ,@ValueType             NVARCHAR (64) 
    ,@Created				DATETIME2 (7) 
    ,@LastUpdated			DATETIME2 (7)
    ,@Immutable				BIT

AS

UPDATE [dbo].[tbl_Claims]
SET
     Id						= @Id
	,IssuerId				= @IssuerId
    ,ActorId				= @ActorId
	,Subject				= @Subject
	,Type					= @Type
	,Value					= @Value
	,ValueType				= @ValueType
    ,Created				= @Created
    ,LastUpdated			= @LastUpdated
    ,Immutable				= @Immutable
WHERE Id = @Id
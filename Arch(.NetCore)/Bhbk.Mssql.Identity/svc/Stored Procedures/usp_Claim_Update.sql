
CREATE PROCEDURE [svc].[usp_Claim_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (MAX) 
    ,@Type					NVARCHAR (MAX)
    ,@Value					NVARCHAR (MAX) 
    ,@ValueType             NVARCHAR (64) 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @LASTUPDATED DATETIME2 (7) = GETDATE()

UPDATE [dbo].[tbl_Claim]
SET
     Id						= @Id
	,IssuerId				= @IssuerId
    ,ActorId				= @ActorId
	,Subject				= @Subject
	,Type					= @Type
	,Value					= @Value
	,ValueType				= @ValueType
    ,LastUpdated			= @LASTUPDATED
    ,Immutable				= @Immutable
WHERE Id = @Id

SELECT * FROM [svc].[uvw_Claim] WHERE [svc].[uvw_Claim].Id = @Id

END
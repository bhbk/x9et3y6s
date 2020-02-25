



CREATE PROCEDURE [svc].[usp_Issuer_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@IssuerKey				NVARCHAR (MAX) 
    ,@Enabled				BIT 
    ,@LastUpdated			DATETIME2 (7)
    ,@Immutable				BIT

AS
BEGIN

UPDATE [dbo].[tbl_Issuers]
SET
     Id						= @Id
    ,ActorId				= @ActorId
	,Name					= @Name
	,Description			= @Description
	,IssuerKey				= @IssuerKey
	,Enabled				= @Enabled
    ,LastUpdated			= @LastUpdated
    ,Immutable				= @Immutable
WHERE Id = @Id

END




CREATE PROCEDURE [svc].[usp_Issuer_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@IssuerKey				NVARCHAR (MAX) 
    ,@Enabled				BIT 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @LASTUPDATED DATETIME2 (7) = GETDATE()

UPDATE [dbo].[tbl_Issuers]
SET
     Id						= @Id
    ,ActorId				= @ActorId
	,Name					= @Name
	,Description			= @Description
	,IssuerKey				= @IssuerKey
	,Enabled				= @Enabled
    ,LastUpdated			= @LASTUPDATED
    ,Immutable				= @Immutable
WHERE Id = @Id

SELECT * FROM [svc].[uvw_Issuers] WHERE [svc].[uvw_Issuers].Id = @Id

END
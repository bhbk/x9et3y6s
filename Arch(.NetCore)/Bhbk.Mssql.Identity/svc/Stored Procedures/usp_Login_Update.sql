
CREATE PROCEDURE [svc].[usp_Login_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@Enabled				BIT 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @LASTUPDATED DATETIME2 (7) = GETDATE()

UPDATE [dbo].[tbl_Logins]
SET
     Id						= @Id
    ,ActorId				= @ActorId
	,Name					= @Name
	,Description			= @Description
	,Enabled				= @Enabled
    ,LastUpdated			= @LASTUPDATED
    ,Immutable				= @Immutable
WHERE Id = @Id

SELECT * FROM [svc].[uvw_Logins] WHERE [svc].[uvw_Logins].Id = @Id

END
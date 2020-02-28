

CREATE PROCEDURE [svc].[usp_Login_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@Enabled				BIT 
    ,@LastUpdated			DATETIME2 (7)
    ,@Immutable				BIT

AS
BEGIN

UPDATE [dbo].[tbl_Logins]
SET
     Id						= @Id
    ,ActorId				= @ActorId
	,Name					= @Name
	,Description			= @Description
	,Enabled				= @Enabled
    ,LastUpdated			= @LastUpdated
    ,Immutable				= @Immutable
WHERE Id = @Id

SELECT * FROM [svc].[uvw_Logins] WHERE [svc].[uvw_Logins].Id = @Id

END
﻿
CREATE PROCEDURE [svc].[usp_Role_Update]
     @Id					UNIQUEIDENTIFIER 
	,@AudienceId			UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@Enabled				BIT 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @LASTUPDATED DATETIME2 (7) = GETDATE()

UPDATE [dbo].[tbl_Roles]
SET
     Id						= @Id
	,AudienceId				= @AudienceId
    ,ActorId				= @ActorId
	,Name					= @Name
	,Description			= @Description
	,Enabled				= @Enabled
    ,LastUpdated			= @LASTUPDATED
    ,Immutable				= @Immutable
WHERE Id = @Id

SELECT * FROM [svc].[uvw_Roles] WHERE [svc].[uvw_Roles].Id = @Id

END
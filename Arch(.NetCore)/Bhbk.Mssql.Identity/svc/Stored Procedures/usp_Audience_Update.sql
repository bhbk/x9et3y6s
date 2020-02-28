﻿


CREATE PROCEDURE [svc].[usp_Audience_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@AudienceType			NVARCHAR (MAX) 
    ,@Created				DATETIME2 (7) 
    ,@LastUpdated			DATETIME2 (7)
    ,@LockoutEnabled		BIT     
    ,@LockoutEnd			DATETIMEOFFSET (7)
    ,@LastLoginSuccess		DATETIME2 (7)
    ,@LastLoginFailure		DATETIME2 (7)
    ,@AccessFailedCount		INT  
    ,@AccessSuccessCount	INT  
    ,@Immutable				BIT

AS
BEGIN

UPDATE [dbo].[tbl_Audiences]
SET
     Id						= @Id
    ,ActorId				= @ActorId
    ,IssuerId				= @IssuerId
	,Name					= @Name
	,Description			= @Description
	,AudienceType			= @AudienceType
	,Created				= @Created
    ,LastUpdated			= @LastUpdated
    ,LockoutEnabled			= @LockoutEnabled
    ,LockoutEnd				= @LockoutEnd
    ,LastLoginSuccess		= @LastLoginSuccess
    ,LastLoginFailure		= @LastLoginFailure
    ,AccessFailedCount		= @AccessFailedCount
    ,AccessSuccessCount		= @AccessSuccessCount
    ,Immutable				= @Immutable
WHERE Id = @Id

SELECT * FROM [svc].[uvw_Audiences] WHERE [svc].[uvw_Audiences].Id = @Id

END
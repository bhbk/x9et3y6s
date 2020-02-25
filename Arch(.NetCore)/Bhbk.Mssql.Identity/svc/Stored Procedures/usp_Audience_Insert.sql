


CREATE PROCEDURE [svc].[usp_Audience_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@ConcurrencyStamp		NVARCHAR (MAX)
    ,@PasswordHash			NVARCHAR (MAX)
    ,@SecurityStamp			NVARCHAR (MAX)
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

DECLARE @AUDIENCEID UNIQUEIDENTIFIER = NEWID()

INSERT INTO [dbo].[tbl_Audiences]
	(
     Id         
	,IssuerId
    ,ActorId    
    ,Name           
    ,Description   
	,ConcurrencyStamp
	,PasswordHash
	,SecurityStamp
	,AudienceType
    ,Created           
    ,LastUpdated      
    ,LockoutEnabled      
    ,LockoutEnd        
    ,LastLoginSuccess 
    ,LastLoginFailure  
    ,AccessFailedCount  
    ,AccessSuccessCount  
    ,Immutable        
	)
VALUES
	(
     @AUDIENCEID          
	,@IssuerId
    ,@ActorId   
    ,@Name           
    ,@Description       
	,@ConcurrencyStamp
	,@PasswordHash
	,@SecurityStamp
	,@AudienceType
    ,@Created           
    ,@LastUpdated      
    ,@LockoutEnabled      
    ,@LockoutEnd        
    ,@LastLoginSuccess 
    ,@LastLoginFailure  
    ,@AccessFailedCount  
    ,@AccessSuccessCount  
    ,@Immutable        
	);

SELECT * FROM [dbo].[tbl_Audiences] WHERE [dbo].[tbl_Audiences].Id = @AUDIENCEID

END
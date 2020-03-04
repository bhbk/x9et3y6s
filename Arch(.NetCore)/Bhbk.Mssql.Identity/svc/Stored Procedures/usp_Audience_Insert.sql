


CREATE PROCEDURE [svc].[usp_Audience_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@AudienceType			NVARCHAR (MAX) 
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
DECLARE @CREATED DATETIME2 (7) = GETDATE()

INSERT INTO [dbo].[tbl_Audiences]
	(
     Id         
	,IssuerId
    ,ActorId    
    ,Name           
    ,Description   
	,AudienceType
    ,Created           
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
	,@AudienceType
    ,@CREATED         
    ,@LockoutEnabled      
    ,@LockoutEnd        
    ,@LastLoginSuccess 
    ,@LastLoginFailure  
    ,@AccessFailedCount  
    ,@AccessSuccessCount  
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Audiences] WHERE [svc].[uvw_Audiences].Id = @AUDIENCEID

END
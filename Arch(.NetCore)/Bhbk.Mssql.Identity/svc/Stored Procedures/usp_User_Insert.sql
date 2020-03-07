


CREATE PROCEDURE [svc].[usp_User_Insert]
    @ActorId              UNIQUEIDENTIFIER
    ,@UserName             NVARCHAR (MAX) 
    ,@EmailAddress         NVARCHAR (MAX)
    ,@FirstName            NVARCHAR (MAX)
    ,@LastName             NVARCHAR (MAX) 
    ,@PhoneNumber          NVARCHAR (16)
    ,@LockoutEnabled       BIT     
    ,@LockoutEnd           DATETIMEOFFSET (7)
    ,@HumanBeing           BIT
    ,@Immutable            BIT

AS
BEGIN

DECLARE @USERID UNIQUEIDENTIFIER = NEWID()
DECLARE @CREATED DATETIME2 (7) = GETDATE()

INSERT INTO [dbo].[tbl_Users]
	(
     Id           
    ,ActorId    
    ,UserName  
	,EmailAddress
    ,EmailConfirmed       
    ,FirstName       
    ,LastName     
    ,PhoneNumber      
    ,PhoneNumberConfirmed  
    ,Created           
    ,LockoutEnabled      
    ,LockoutEnd        
    ,AccessFailedCount  
    ,AccessSuccessCount  
	,ConcurrencyStamp
    ,PasswordConfirmed
	,SecurityStamp
    ,TwoFactorEnabled 
    ,HumanBeing         
    ,Immutable        
	)
VALUES
	(
     @USERID       
    ,@ActorId    
    ,@UserName         
	,@EmailAddress
    ,'FALSE'     
    ,@FirstName       
    ,@LastName     
    ,@PhoneNumber      
    ,'FALSE'     
    ,@CREATED        
    ,@LockoutEnabled      
    ,@LockoutEnd        
    ,0
    ,0  
	,CAST(NEWID() AS nvarchar(36))
    ,'FALSE'  
	,CAST(NEWID() AS nvarchar(36))
    ,'FALSE'     
    ,@HumanBeing         
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Users] WHERE [svc].[uvw_Users].Id = @USERID

END
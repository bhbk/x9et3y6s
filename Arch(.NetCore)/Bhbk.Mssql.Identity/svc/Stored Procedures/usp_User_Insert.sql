
CREATE PROCEDURE [svc].[usp_User_Insert]
     @Id                   UNIQUEIDENTIFIER 
    ,@ActorId              UNIQUEIDENTIFIER
    ,@Email                NVARCHAR (MAX) 
    ,@EmailConfirmed       BIT          
    ,@FirstName            NVARCHAR (MAX)
    ,@LastName             NVARCHAR (MAX) 
    ,@PhoneNumber          NVARCHAR (16)
    ,@PhoneNumberConfirmed BIT      
    ,@Created              DATETIME2 (7) 
    ,@LastUpdated          DATETIME2 (7)
    ,@LockoutEnabled       BIT     
    ,@LockoutEnd           DATETIMEOFFSET (7)
    ,@LastLoginSuccess     DATETIME2 (7)
    ,@LastLoginFailure     DATETIME2 (7)
    ,@AccessFailedCount    INT  
    ,@AccessSuccessCount   INT  
    ,@ConcurrencyStamp     NVARCHAR (MAX)
    ,@PasswordHash         NVARCHAR (MAX)
    ,@PasswordConfirmed    BIT  
    ,@SecurityStamp        NVARCHAR (MAX)
    ,@TwoFactorEnabled     BIT
    ,@HumanBeing           BIT
    ,@Immutable            BIT

AS

INSERT INTO [dbo].[tbl_Users]
	(
     Id           
    ,ActorId    
    ,Email           
    ,EmailConfirmed       
    ,FirstName       
    ,LastName     
    ,PhoneNumber      
    ,PhoneNumberConfirmed  
    ,Created           
    ,LastUpdated      
    ,LockoutEnabled      
    ,LockoutEnd        
    ,LastLoginSuccess 
    ,LastLoginFailure  
    ,AccessFailedCount  
    ,AccessSuccessCount  
    ,ConcurrencyStamp  
    ,PasswordHash      
    ,PasswordConfirmed 
    ,SecurityStamp     
    ,TwoFactorEnabled 
    ,HumanBeing         
    ,Immutable        
	)
VALUES
	(
     @Id           
    ,@ActorId    
    ,@Email           
    ,@EmailConfirmed       
    ,@FirstName       
    ,@LastName     
    ,@PhoneNumber      
    ,@PhoneNumberConfirmed  
    ,@Created           
    ,@LastUpdated      
    ,@LockoutEnabled      
    ,@LockoutEnd        
    ,@LastLoginSuccess 
    ,@LastLoginFailure  
    ,@AccessFailedCount  
    ,@AccessSuccessCount  
    ,@ConcurrencyStamp  
    ,@PasswordHash      
    ,@PasswordConfirmed 
    ,@SecurityStamp     
    ,@TwoFactorEnabled 
    ,@HumanBeing         
    ,@Immutable        
	);

SELECT StudentID = @@IDENTITY;
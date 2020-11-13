
CREATE PROCEDURE [svc].[usp_User_Insert]
    @ActorId            UNIQUEIDENTIFIER
    ,@UserName          NVARCHAR (MAX) 
    ,@EmailAddress      NVARCHAR (MAX)
    ,@FirstName         NVARCHAR (MAX)
    ,@LastName          NVARCHAR (MAX) 
    ,@PhoneNumber       NVARCHAR (16)
    ,@IsHumanBeing      BIT
    ,@IsLockedOut       BIT     
    ,@IsDeletable       BIT
    ,@LockoutEndUtc     DATETIMEOFFSET (7)

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @USERID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_User]
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
            ,IsHumanBeing         
            ,IsMultiFactor 
            ,IsLockedOut      
            ,IsDeletable        
            ,AccessFailedCount  
            ,AccessSuccessCount  
            ,PasswordConfirmed
	        ,ConcurrencyStamp
	        ,SecurityStamp
            ,LockoutEndUtc        
            ,CreatedUtc           
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
            ,@IsHumanBeing         
            ,'FALSE'     
            ,@IsLockedOut      
            ,@IsDeletable        
            ,0
            ,0  
            ,'FALSE'  
	        ,CAST(NEWID() AS nvarchar(36))
	        ,CAST(NEWID() AS nvarchar(36))
            ,@LockoutEndUtc        
            ,@CREATEDUTC        
	        );

        SELECT * FROM [svc].[uvw_User] WHERE [svc].[uvw_User].Id = @USERID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
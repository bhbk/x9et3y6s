
CREATE PROCEDURE [svc].[usp_User_Insert]
    @UserName          NVARCHAR (256) 
    ,@EmailAddress      NVARCHAR (256)
    ,@FirstName         NVARCHAR (128)
    ,@LastName          NVARCHAR (128) 
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
            ,UserName  
	        ,EmailAddress
            ,EmailConfirmed       
            ,FirstName       
            ,LastName     
            ,PhoneNumber      
            ,PhoneNumberConfirmed  
            ,IsHumanBeing         
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
            ,@UserName         
	        ,@EmailAddress
            ,'FALSE'     
            ,@FirstName       
            ,@LastName     
            ,@PhoneNumber      
            ,'FALSE'     
            ,@IsHumanBeing         
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
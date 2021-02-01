
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

    	BEGIN TRANSACTION;

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
            ,'FALSE'  
	        ,CAST(NEWID() AS nvarchar(36))
	        ,CAST(NEWID() AS nvarchar(36))
            ,@LockoutEndUtc        
            ,@CREATEDUTC        
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_User]
            WHERE Id = @USERID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

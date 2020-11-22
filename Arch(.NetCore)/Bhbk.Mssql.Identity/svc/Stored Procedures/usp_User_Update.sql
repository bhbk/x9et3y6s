
CREATE PROCEDURE [svc].[usp_User_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@UserName				NVARCHAR (256) 
    ,@EmailAddress			NVARCHAR (256)
    ,@EmailConfirmed   		BIT     
    ,@FirstName				NVARCHAR (128)
    ,@LastName				NVARCHAR (128) 
    ,@PhoneNumber			NVARCHAR (16)
    ,@PhoneNumberConfirmed	BIT     
	,@PasswordConfirmed		BIT
    ,@IsHumanBeing			BIT
    ,@IsLockedOut   		BIT     
    ,@IsDeletable			BIT
    ,@LockoutEndUtc			DATETIMEOFFSET (7)

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_User]
        SET
             Id						= @Id
            ,UserName				= @UserName
	        ,EmailAddress			= @EmailAddress
			,EmailConfirmed			= @EmailConfirmed
            ,FirstName				= @FirstName
            ,LastName				= @LastName
            ,PhoneNumber			= @PhoneNumber
			,PhoneNumberConfirmed	= @PhoneNumberConfirmed
			,PasswordConfirmed		= @PasswordConfirmed
            ,LastUpdatedUtc			= @LASTUPDATED
            ,IsHumanBeing			= @IsHumanBeing
            ,IsLockedOut			= @IsLockedOut
            ,IsDeletable			= @IsDeletable
            ,LockoutEndUtc	    	= @LockoutEndUtc
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_User] WHERE [svc].[uvw_User].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
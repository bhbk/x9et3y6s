

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

    	BEGIN TRANSACTION;

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
            ,IsHumanBeing			= @IsHumanBeing
            ,IsLockedOut			= @IsLockedOut
            ,IsDeletable			= @IsDeletable
            ,LockoutEndUtc	    	= @LockoutEndUtc
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_User]
            WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

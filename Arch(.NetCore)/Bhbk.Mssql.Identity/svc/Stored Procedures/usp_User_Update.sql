
CREATE PROCEDURE [svc].[usp_User_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Email					NVARCHAR (MAX) 
    ,@EmailConfirmed		BIT          
    ,@FirstName				NVARCHAR (MAX)
    ,@LastName				NVARCHAR (MAX) 
    ,@PhoneNumber			NVARCHAR (16)
    ,@PhoneNumberConfirmed	BIT      
    ,@Created				DATETIME2 (7) 
    ,@LastUpdated			DATETIME2 (7)
    ,@LockoutEnabled		BIT     
    ,@LockoutEnd			DATETIMEOFFSET (7)
    ,@LastLoginSuccess		DATETIME2 (7)
    ,@LastLoginFailure		DATETIME2 (7)
    ,@AccessFailedCount		INT  
    ,@AccessSuccessCount	INT  
    ,@ConcurrencyStamp		NVARCHAR (MAX)
    ,@PasswordHash			NVARCHAR (MAX)
    ,@PasswordConfirmed		BIT  
    ,@SecurityStamp			NVARCHAR (MAX)
    ,@TwoFactorEnabled		BIT
    ,@HumanBeing			BIT
    ,@Immutable				BIT

AS

UPDATE [dbo].[tbl_Users]
SET
     Id						= @Id
    ,ActorId				= @ActorId
    ,Email					= @Email
    ,EmailConfirmed		    = @EmailConfirmed
    ,FirstName				= @FirstName
    ,LastName				= @LastName
    ,PhoneNumber			= @PhoneNumber
    ,PhoneNumberConfirmed	= @PhoneNumberConfirmed  
    ,Created				= @Created
    ,LastUpdated			= @LastUpdated
    ,LockoutEnabled			= @LockoutEnabled
    ,LockoutEnd				= @LockoutEnd
    ,LastLoginSuccess		= @LastLoginSuccess
    ,LastLoginFailure		= @LastLoginFailure
    ,AccessFailedCount		= @AccessFailedCount
    ,AccessSuccessCount		= @AccessSuccessCount
    ,ConcurrencyStamp		= @ConcurrencyStamp
    ,PasswordHash			= @PasswordHash
    ,PasswordConfirmed		= @PasswordConfirmed
    ,SecurityStamp			= @SecurityStamp
    ,TwoFactorEnabled		= @TwoFactorEnabled
    ,HumanBeing				= @HumanBeing
    ,Immutable				= @Immutable
WHERE Id = @Id
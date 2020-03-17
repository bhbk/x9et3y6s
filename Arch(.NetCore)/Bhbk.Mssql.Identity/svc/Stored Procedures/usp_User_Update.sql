
CREATE PROCEDURE [svc].[usp_User_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@UserName				NVARCHAR (MAX) 
    ,@EmailAddress			NVARCHAR (MAX)
    ,@FirstName				NVARCHAR (MAX)
    ,@LastName				NVARCHAR (MAX) 
    ,@PhoneNumber			NVARCHAR (16)
    ,@LockoutEnabled		BIT     
    ,@LockoutEnd			DATETIMEOFFSET (7)
    ,@HumanBeing			BIT
    ,@Immutable				BIT

AS
BEGIN

DECLARE @LASTUPDATED DATETIME2 (7) = GETDATE()

UPDATE [dbo].[tbl_Users]
SET
     Id						= @Id
    ,ActorId				= @ActorId
    ,UserName				= @UserName
	,EmailAddress			= @EmailAddress
    ,FirstName				= @FirstName
    ,LastName				= @LastName
    ,PhoneNumber			= @PhoneNumber
    ,LastUpdated			= @LASTUPDATED
    ,LockoutEnabled			= @LockoutEnabled
    ,LockoutEnd				= @LockoutEnd
    ,HumanBeing				= @HumanBeing
    ,Immutable				= @Immutable
WHERE Id = @Id

SELECT * FROM [svc].[uvw_Users] WHERE [svc].[uvw_Users].Id = @Id
END
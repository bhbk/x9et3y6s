


CREATE PROCEDURE [svc].[usp_User_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Email                NVARCHAR (MAX) 
    ,@FirstName            NVARCHAR (MAX)
    ,@LastName             NVARCHAR (MAX) 
    ,@PhoneNumber          NVARCHAR (16)
    ,@Created              DATETIME2 (7) 
    ,@LastUpdated          DATETIME2 (7)
    ,@LockoutEnabled       BIT     
    ,@LockoutEnd           DATETIMEOFFSET (7)
    ,@HumanBeing           BIT
    ,@Immutable            BIT

AS
BEGIN

UPDATE [dbo].[tbl_Users]
SET
     Id						= @Id
    ,ActorId				= @ActorId
    ,Email					= @Email
    ,FirstName				= @FirstName
    ,LastName				= @LastName
    ,PhoneNumber			= @PhoneNumber
    ,LastUpdated			= @LastUpdated
    ,LockoutEnabled			= @LockoutEnabled
    ,LockoutEnd				= @LockoutEnd
    ,HumanBeing				= @HumanBeing
    ,Immutable				= @Immutable
WHERE Id = @Id

SELECT * FROM [svc].[uvw_Users] WHERE [svc].[uvw_Users].Id = @Id
END
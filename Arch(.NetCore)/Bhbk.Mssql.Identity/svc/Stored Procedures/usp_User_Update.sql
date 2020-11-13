
CREATE PROCEDURE [svc].[usp_User_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@UserName				NVARCHAR (MAX) 
    ,@EmailAddress			NVARCHAR (MAX)
    ,@FirstName				NVARCHAR (MAX)
    ,@LastName				NVARCHAR (MAX) 
    ,@PhoneNumber			NVARCHAR (16)
    ,@IsLockedOut   		BIT     
    ,@IsHumanBeing			BIT
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
            ,ActorId				= @ActorId
            ,UserName				= @UserName
	        ,EmailAddress			= @EmailAddress
            ,FirstName				= @FirstName
            ,LastName				= @LastName
            ,PhoneNumber			= @PhoneNumber
            ,LastUpdatedUtc			= @LASTUPDATED
            ,IsLockedOut			= @IsLockedOut
            ,IsHumanBeing			= @IsHumanBeing
            ,IsDeletable			= @IsDeletable
            ,LockoutEndUtc	    	= @LockoutEndUtc
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_User] WHERE [svc].[uvw_User].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
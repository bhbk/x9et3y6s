
CREATE PROCEDURE [svc].[usp_UserPassword_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@PasswordHashPBKDF2	NVARCHAR (2048)
    ,@PasswordHashSHA256    NVARCHAR (2048)     

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_User]
        SET
             Id						= @Id
			,ConcurrencyStamp		= CAST(NEWID() AS nvarchar(36))
			,PasswordHashPBKDF2		= @PasswordHashPBKDF2
			,PasswordHashSHA256		= @PasswordHashSHA256
			,SecurityStamp			= CAST(NEWID() AS nvarchar(36))
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_User] 
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
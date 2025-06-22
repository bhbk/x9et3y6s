
CREATE PROCEDURE [svc].[usp_AudiencePassword_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@PasswordHashPBKDF2	NVARCHAR (2048)
    ,@PasswordHashSHA256    NVARCHAR (2048)     

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Audience]
        SET
             Id						= @Id
			,ConcurrencyStamp		= CAST(NEWID() AS nvarchar(36))
			,PasswordHashPBKDF2		= @PasswordHashPBKDF2
			,PasswordHashSHA256		= @PasswordHashSHA256
			,SecurityStamp			= CAST(NEWID() AS nvarchar(36))
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Audience]
            WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

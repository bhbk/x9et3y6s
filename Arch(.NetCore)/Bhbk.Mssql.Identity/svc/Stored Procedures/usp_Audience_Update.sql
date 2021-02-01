
CREATE PROCEDURE [svc].[usp_Audience_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@IsLockedOut		    BIT     
    ,@IsDeletable			BIT
    ,@LockoutEndUtc			DATETIMEOFFSET (7)

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        UPDATE [dbo].[tbl_Audience]
        SET
             Id						= @Id
            ,IssuerId				= @IssuerId
	        ,Name					= @Name
	        ,Description			= @Description
            ,IsLockedOut			= @IsLockedOut
            ,IsDeletable			= @IsDeletable
            ,LockoutEndUtc			= @LockoutEndUtc
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

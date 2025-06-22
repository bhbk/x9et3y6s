
CREATE PROCEDURE [svc].[usp_Role_Update]
     @Id					UNIQUEIDENTIFIER 
	,@AudienceId			UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@IsEnabled				BIT 
    ,@IsDeletable	    	BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        UPDATE [dbo].[tbl_Role]
        SET
             Id						= @Id
	        ,AudienceId				= @AudienceId
	        ,Name					= @Name
	        ,Description			= @Description
	        ,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Role]
            WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

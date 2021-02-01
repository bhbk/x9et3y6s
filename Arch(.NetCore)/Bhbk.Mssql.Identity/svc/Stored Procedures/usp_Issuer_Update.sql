
CREATE PROCEDURE [svc].[usp_Issuer_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@IssuerKey				NVARCHAR (1024) 
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        UPDATE [dbo].[tbl_Issuer]
        SET
             Id						= @Id
	        ,Name					= @Name
	        ,Description			= @Description
	        ,IssuerKey				= @IssuerKey
	        ,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Issuer]
            WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

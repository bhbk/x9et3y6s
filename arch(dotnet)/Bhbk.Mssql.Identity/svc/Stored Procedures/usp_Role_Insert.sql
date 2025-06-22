
CREATE PROCEDURE [svc].[usp_Role_Insert]
	 @AudienceId			UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @ROLEID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_Role]
	        (
             Id           
	        ,AudienceId
            ,Name           
            ,Description       
            ,IsEnabled     
            ,IsDeletable        
            ,CreatedUtc           
	        )
        VALUES
	        (
             @ROLEID         
	        ,@AudienceId
            ,@Name           
            ,@Description       
            ,@IsEnabled     
            ,@IsDeletable        
            ,@CREATEDUTC         
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Role]
            WHERE Id = @ROLEID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

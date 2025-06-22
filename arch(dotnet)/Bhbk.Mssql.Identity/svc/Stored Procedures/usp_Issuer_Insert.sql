
CREATE PROCEDURE [svc].[usp_Issuer_Insert]
    @Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@IssuerKey				NVARCHAR (1024) 
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @ISSUERID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_Issuer]
	        (
             Id           
            ,Name           
            ,Description       
            ,IssuerKey       
            ,IsEnabled     
            ,IsDeletable        
            ,CreatedUtc           
	        )
        VALUES
	        (
             @ISSUERID           
            ,@Name           
            ,@Description       
            ,@IssuerKey       
            ,@IsEnabled     
            ,@IsDeletable        
            ,@CREATEDUTC          
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Issuer]
            WHERE Id = @ISSUERID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END


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

        SELECT * FROM [svc].[uvw_Issuer] WHERE [svc].[uvw_Issuer].Id = @ISSUERID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
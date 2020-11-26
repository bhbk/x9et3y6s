
CREATE PROCEDURE [svc].[usp_Login_Insert]
    @Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@LoginKey				NVARCHAR (2048)
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LOGINID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_Login]
	        (
             Id           
            ,Name           
            ,Description
	        ,LoginKey
            ,IsEnabled     
            ,CreatedUtc           
            ,IsDeletable        
	        )
        VALUES
	        (
             @LOGINID         
            ,@Name           
            ,@Description       
	        ,@LoginKey
            ,@IsEnabled     
            ,@CREATEDUTC       
            ,@IsDeletable        
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Login]
            WHERE Id = @LOGINID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE PROCEDURE [svc].[usp_EmailActivity_Insert]
     @EmailId				UNIQUEIDENTIFIER
    ,@SendgridId            NVARCHAR (50) 
    ,@SendgridStatus        NVARCHAR (100) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @ACTIVITYID UNIQUEIDENTIFIER = NEWID()
        DECLARE @STATUSATUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_EmailActivity]
	        (
             Id           
            ,EmailId    
            ,SendgridId    
			,SendgridStatus
			,StatusAtUtc
	        )
        VALUES
	        (
             @ACTIVITYID          
            ,@EmailId    
            ,@SendgridId    
            ,@SendgridStatus           
            ,@STATUSATUTC 
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_EmailActivity]
            WHERE Id = @ACTIVITYID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

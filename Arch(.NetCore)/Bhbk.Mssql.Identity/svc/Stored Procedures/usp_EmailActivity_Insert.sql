


CREATE PROCEDURE [svc].[usp_EmailActivity_Insert]
     @EmailId				UNIQUEIDENTIFIER
    ,@SendgridId            NVARCHAR (50) 
    ,@SendgridStatus        NVARCHAR (100) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

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

        SELECT * FROM [svc].[uvw_EmailActivity] WHERE [svc].[uvw_EmailActivity].Id = @ACTIVITYID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
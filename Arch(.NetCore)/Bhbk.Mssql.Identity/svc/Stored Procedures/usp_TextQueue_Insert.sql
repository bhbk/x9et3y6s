
CREATE PROCEDURE [svc].[usp_TextQueue_Insert]
    @FromPhoneNumber       NVARCHAR (15) 
    ,@ToPhoneNumber         NVARCHAR (15) 
    ,@Body					NVARCHAR (MAX)
    ,@SendAtUtc             DATETIMEOFFSET (7) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @TEXTID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_TextQueue]
	        (
             Id           
			,FromPhoneNumber
			,ToPhoneNumber
            ,Body 
			,IsCancelled
			,CreatedUtc
            ,SendAtUtc           
	        )
        VALUES
	        (
             @TEXTID          
            ,@FromPhoneNumber       
            ,@ToPhoneNumber
			,@Body
			,'FALSE'
            ,@CREATEDUTC           
            ,@SendAtUtc        
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_TextQueue]
            WHERE Id = @TEXTID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
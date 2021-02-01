
CREATE PROCEDURE [svc].[usp_Audience_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@IsLockedOut   		BIT     
    ,@IsDeletable			BIT
    ,@LockoutEndUtc			DATETIMEOFFSET (7)

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @AUDIENCEID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_Audience]
	        (
             Id         
	        ,IssuerId
            ,Name           
            ,Description   
            ,IsLockedOut   
            ,IsDeletable        
            ,LockoutEndUtc        
            ,CreatedUtc           
	        )
        VALUES
	        (
             @AUDIENCEID          
	        ,@IssuerId
            ,@Name           
            ,@Description       
            ,@IsLockedOut  
            ,@IsDeletable        
            ,@LockoutEndUtc        
            ,@CREATEDUTC         
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Audience]
            WHERE Id = @AUDIENCEID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END


CREATE PROCEDURE [svc].[usp_Audience_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@IsLockedOut   		BIT     
    ,@IsDeletable			BIT
    ,@AccessFailedCount		INT  
    ,@AccessSuccessCount	INT  
    ,@LockoutEndUtc			DATETIMEOFFSET (7)
    ,@LastLoginSuccessUtc	DATETIMEOFFSET (7)
    ,@LastLoginFailureUtc	DATETIMEOFFSET (7)

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

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
            ,AccessFailedCount  
            ,AccessSuccessCount  
            ,LockoutEndUtc        
            ,LastLoginSuccessUtc 
            ,LastLoginFailureUtc  
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
            ,@AccessFailedCount  
            ,@AccessSuccessCount  
            ,@LockoutEndUtc        
            ,@LastLoginSuccessUtc 
            ,@LastLoginFailureUtc  
            ,@CREATEDUTC         
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Audience]
            WHERE Id = @AUDIENCEID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
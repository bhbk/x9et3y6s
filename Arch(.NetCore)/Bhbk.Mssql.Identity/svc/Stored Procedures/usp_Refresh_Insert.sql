
CREATE PROCEDURE [svc].[usp_Refresh_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@RefreshValue			NVARCHAR (2048) 
    ,@RefreshType			NVARCHAR (64)
    ,@IssuedUtc				DATETIMEOFFSET (7) 
    ,@ValidFromUtc			DATETIMEOFFSET (7)
    ,@ValidToUtc			DATETIMEOFFSET (7) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @REFRESHID UNIQUEIDENTIFIER = NEWID()

        INSERT INTO [dbo].[tbl_Refresh]
	        (
             Id         
	        ,IssuerId
            ,AudienceId    
            ,UserId           
            ,RefreshValue   
	        ,RefreshType
	        ,IssuedUtc
	        ,ValidFromUtc
	        ,ValidToUtc
	        )
        VALUES
	        (
             @REFRESHID          
	        ,@IssuerId
            ,@AudienceId   
            ,@UserId         
            ,@RefreshValue       
	        ,@RefreshValue
	        ,@IssuedUtc
	        ,@ValidFromUtc
	        ,@ValidToUtc
	        );

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Refresh]
            WHERE Id = @REFRESHID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

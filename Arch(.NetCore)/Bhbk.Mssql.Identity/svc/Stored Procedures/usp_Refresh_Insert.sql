
CREATE PROCEDURE [svc].[usp_Refresh_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@RefreshValue			NVARCHAR (512) 
    ,@RefreshType			NVARCHAR (64)
    ,@IssuedUtc				DATETIMEOFFSET (7) 
    ,@ValidFromUtc			DATETIMEOFFSET (7)
    ,@ValidToUtc			DATETIMEOFFSET (7) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

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

        SELECT * FROM [svc].[uvw_Refresh] WHERE [svc].[uvw_Refresh].Id = @REFRESHID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
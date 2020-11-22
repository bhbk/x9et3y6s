
CREATE PROCEDURE [svc].[usp_Claim_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (128) 
    ,@Type					NVARCHAR (128)
    ,@Value					NVARCHAR (256) 
    ,@ValueType             NVARCHAR (64) 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @CLAIMID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_Claim]
	        (
             Id           
            ,IssuerId    
            ,Subject           
            ,Type       
            ,Value       
            ,ValueType     
            ,CreatedUtc           
            ,IsDeletable        
	        )
        VALUES
	        (
             @CLAIMID          
            ,@IssuerId    
            ,@Subject           
            ,@Type       
            ,@Value       
            ,@ValueType     
            ,@CREATEDUTC           
            ,@IsDeletable        
	        );

        SELECT * FROM [svc].[uvw_Claim] WHERE [svc].[uvw_Claim].Id = @CLAIMID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
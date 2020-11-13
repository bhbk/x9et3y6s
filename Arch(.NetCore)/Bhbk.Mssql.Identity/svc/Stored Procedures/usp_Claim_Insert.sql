
CREATE PROCEDURE [svc].[usp_Claim_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (MAX) 
    ,@Type					NVARCHAR (MAX)
    ,@Value					NVARCHAR (MAX) 
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
            ,ActorId    
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
            ,@ActorId    
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
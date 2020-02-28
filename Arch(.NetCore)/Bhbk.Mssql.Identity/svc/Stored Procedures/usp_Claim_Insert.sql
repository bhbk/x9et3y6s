



CREATE PROCEDURE [svc].[usp_Claim_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (MAX) 
    ,@Type					NVARCHAR (MAX)
    ,@Value					NVARCHAR (MAX) 
    ,@ValueType             NVARCHAR (64) 
    ,@Created				DATETIME2 (7) 
    ,@LastUpdated			DATETIME2 (7)
    ,@Immutable				BIT

AS
BEGIN

DECLARE @CLAIMID UNIQUEIDENTIFIER = NEWID()

INSERT INTO [dbo].[tbl_Claims]
	(
     Id           
    ,IssuerId    
    ,ActorId    
    ,Subject           
    ,Type       
    ,Value       
    ,ValueType     
    ,Created           
    ,LastUpdated      
    ,Immutable        
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
    ,@Created           
    ,@LastUpdated      
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Claims] WHERE [svc].[uvw_Claims].Id = @CLAIMID

END
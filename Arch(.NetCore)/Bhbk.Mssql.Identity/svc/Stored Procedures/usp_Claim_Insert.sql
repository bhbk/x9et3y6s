
CREATE PROCEDURE [svc].[usp_Claim_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (MAX) 
    ,@Type					NVARCHAR (MAX)
    ,@Value					NVARCHAR (MAX) 
    ,@ValueType             NVARCHAR (64) 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @CLAIMID UNIQUEIDENTIFIER = NEWID()
DECLARE @CREATED DATETIME2 (7) = GETDATE()

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
    ,@CREATED           
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Claims] WHERE [svc].[uvw_Claims].Id = @CLAIMID

END
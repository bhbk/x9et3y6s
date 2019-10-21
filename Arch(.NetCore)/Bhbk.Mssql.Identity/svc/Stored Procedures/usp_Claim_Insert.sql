

CREATE PROCEDURE [svc].[usp_Claim_Insert]
     @Id					UNIQUEIDENTIFIER 
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (MAX) 
    ,@Type					NVARCHAR (MAX)
    ,@Value					NVARCHAR (MAX) 
    ,@ValueType             NVARCHAR (64) 
    ,@Created				DATETIME2 (7) 
    ,@LastUpdated			DATETIME2 (7)
    ,@Immutable				BIT

AS

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
     @Id           
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
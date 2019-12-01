

CREATE PROCEDURE [svc].[usp_Activity_Insert]
     @Id					UNIQUEIDENTIFIER 
    ,@UserId				UNIQUEIDENTIFIER
    ,@ClientId				UNIQUEIDENTIFIER
    ,@ActivityType          NVARCHAR (64) 
    ,@TableName				NVARCHAR (MAX)
    ,@KeyValues				NVARCHAR (MAX) 
    ,@OriginalValues		NVARCHAR (MAX) 
    ,@CurrentValues			NVARCHAR (MAX) 
    ,@Created				DATETIME2 (7) 
    ,@Immutable				BIT

AS

INSERT INTO [dbo].[tbl_Activities]
	(
     Id           
    ,UserId    
    ,AudienceId    
    ,ActivityType           
    ,TableName
    ,KeyValues   
    ,OriginalValues     
    ,CurrentValues        
    ,Created    
    ,Immutable        
	)
VALUES
	(
     @Id           
    ,@UserId    
    ,@ClientId    
    ,@ActivityType
	,@TableName
    ,@KeyValues    
    ,@OriginalValues   
    ,@CurrentValues
    ,@Created           
    ,@Immutable        
	);
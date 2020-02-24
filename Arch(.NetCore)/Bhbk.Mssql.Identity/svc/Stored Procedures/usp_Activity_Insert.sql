

CREATE PROCEDURE [svc].[usp_Activity_Insert]
     @Id					UNIQUEIDENTIFIER 
    ,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
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
    ,AudienceId    
    ,UserId    
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
    ,@AudienceId    
    ,@UserId    
    ,@ActivityType
	,@TableName
    ,@KeyValues    
    ,@OriginalValues   
    ,@CurrentValues
    ,@Created           
    ,@Immutable        
	);
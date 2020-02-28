


CREATE PROCEDURE [svc].[usp_Activity_Insert]
     @AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@ActivityType          NVARCHAR (64) 
    ,@TableName				NVARCHAR (MAX)
    ,@KeyValues				NVARCHAR (MAX) 
    ,@OriginalValues		NVARCHAR (MAX) 
    ,@CurrentValues			NVARCHAR (MAX) 
    ,@Created				DATETIME2 (7) 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @ACTIVITYID UNIQUEIDENTIFIER = NEWID()

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
     @ACTIVITYID         
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

SELECT * FROM [svc].[uvw_Activities] WHERE [svc].[uvw_Activities].Id = @ACTIVITYID

END
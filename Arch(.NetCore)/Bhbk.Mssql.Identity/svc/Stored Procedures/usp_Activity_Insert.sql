


CREATE PROCEDURE [svc].[usp_Activity_Insert]
     @AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@ActivityType          NVARCHAR (64) 
    ,@TableName				NVARCHAR (MAX)
    ,@KeyValues				NVARCHAR (MAX) 
    ,@OriginalValues		NVARCHAR (MAX) 
    ,@CurrentValues			NVARCHAR (MAX) 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @ACTIVITYID UNIQUEIDENTIFIER = NEWID()
DECLARE @CREATED DATETIME2 (7) = GETDATE()

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
    ,@CREATED        
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Activities] WHERE [svc].[uvw_Activities].Id = @ACTIVITYID

END
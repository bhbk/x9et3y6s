


CREATE PROCEDURE [svc].[usp_Role_Insert]
	 @AudienceId			UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@Enabled				BIT 
    ,@Created				DATETIME2 (7) 
    ,@LastUpdated			DATETIME2 (7)
    ,@Immutable				BIT

AS
BEGIN

DECLARE @ROLEID UNIQUEIDENTIFIER = NEWID()

INSERT INTO [dbo].[tbl_Roles]
	(
     Id           
	,AudienceId
    ,ActorId    
    ,Name           
    ,Description       
    ,Enabled     
    ,Created           
    ,LastUpdated      
    ,Immutable        
	)
VALUES
	(
     @ROLEID         
	,@AudienceId
    ,@ActorId    
    ,@Name           
    ,@Description       
    ,@Enabled     
    ,@Created           
    ,@LastUpdated      
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Roles] WHERE [svc].[uvw_Roles].Id = @ROLEID

END
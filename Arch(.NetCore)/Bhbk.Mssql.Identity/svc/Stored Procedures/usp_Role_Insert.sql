


CREATE PROCEDURE [svc].[usp_Role_Insert]
	 @AudienceId			UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@Enabled				BIT 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @ROLEID UNIQUEIDENTIFIER = NEWID()
DECLARE @CREATED DATETIME2 (7) = GETDATE()

INSERT INTO [dbo].[tbl_Roles]
	(
     Id           
	,AudienceId
    ,ActorId    
    ,Name           
    ,Description       
    ,Enabled     
    ,Created           
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
    ,@CREATED         
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Roles] WHERE [svc].[uvw_Roles].Id = @ROLEID

END
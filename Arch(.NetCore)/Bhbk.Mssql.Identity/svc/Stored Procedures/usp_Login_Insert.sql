



CREATE PROCEDURE [svc].[usp_Login_Insert]
    @ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@LoginKey				NVARCHAR (MAX)
    ,@Enabled				BIT 
    ,@Created				DATETIME2 (7) 
    ,@LastUpdated			DATETIME2 (7)
    ,@Immutable				BIT

AS
BEGIN

DECLARE @LOGINID UNIQUEIDENTIFIER = NEWID()

INSERT INTO [dbo].[tbl_Logins]
	(
     Id           
    ,ActorId    
    ,Name           
    ,Description
	,LoginKey
    ,Enabled     
    ,Created           
    ,LastUpdated      
    ,Immutable        
	)
VALUES
	(
     @LOGINID         
    ,@ActorId    
    ,@Name           
    ,@Description       
	,@LoginKey
    ,@Enabled     
    ,@Created           
    ,@LastUpdated      
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Logins] WHERE [svc].[uvw_Logins].Id = @LOGINID

END




CREATE PROCEDURE [svc].[usp_Login_Insert]
    @ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@LoginKey				NVARCHAR (MAX)
    ,@Enabled				BIT 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @LOGINID UNIQUEIDENTIFIER = NEWID()
DECLARE @CREATED DATETIME2 (7) = GETDATE()

INSERT INTO [dbo].[tbl_Logins]
	(
     Id           
    ,ActorId    
    ,Name           
    ,Description
	,LoginKey
    ,Enabled     
    ,Created           
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
    ,@CREATED       
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Logins] WHERE [svc].[uvw_Logins].Id = @LOGINID

END

CREATE PROCEDURE [svc].[usp_Issuer_Insert]
     @ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@IssuerKey				NVARCHAR (MAX) 
    ,@Enabled				BIT 
    ,@Immutable				BIT

AS
BEGIN

DECLARE @ISSUERID UNIQUEIDENTIFIER = NEWID()
DECLARE @CREATED DATETIME2 (7) = GETDATE()

INSERT INTO [dbo].[tbl_Issuers]
	(
     Id           
    ,ActorId    
    ,Name           
    ,Description       
    ,IssuerKey       
    ,Enabled     
    ,Created           
    ,Immutable        
	)
VALUES
	(
     @ISSUERID           
    ,@ActorId    
    ,@Name           
    ,@Description       
    ,@IssuerKey       
    ,@Enabled     
    ,@CREATED          
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Issuers] WHERE [svc].[uvw_Issuers].Id = @ISSUERID

END
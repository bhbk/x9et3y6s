

CREATE PROCEDURE [svc].[usp_Issuer_Insert]
     @ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@IssuerKey				NVARCHAR (MAX) 
    ,@Enabled				BIT 
    ,@Created				DATETIME2 (7) 
    ,@LastUpdated			DATETIME2 (7)
    ,@Immutable				BIT

AS
BEGIN

DECLARE @ISSUERID UNIQUEIDENTIFIER = NEWID()

INSERT INTO [dbo].[tbl_Issuers]
	(
     Id           
    ,ActorId    
    ,Name           
    ,Description       
    ,IssuerKey       
    ,Enabled     
    ,Created           
    ,LastUpdated      
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
    ,@Created           
    ,@LastUpdated      
    ,@Immutable        
	);

SELECT * FROM [svc].[uvw_Issuers] WHERE [svc].[uvw_Issuers].Id = @ISSUERID

END

CREATE PROCEDURE [svc].[usp_State_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@StateValue			NVARCHAR (512) 
    ,@StateType				NVARCHAR (64)
    ,@StateDecision			BIT
    ,@StateConsume			BIT
    ,@IssuedUtc				DATETIME2 (7) 
    ,@ValidFromUtc			DATETIME2 (7)
    ,@ValidToUtc			DATETIME2 (7) 
    ,@LastPolling			DATETIME2 (7) 

AS
BEGIN

DECLARE @STATEID UNIQUEIDENTIFIER = NEWID()

INSERT INTO [dbo].[tbl_State]
	(
     Id         
	,IssuerId
    ,AudienceId    
    ,UserId           
    ,StateValue   
	,StateType
	,StateDecision
	,StateConsume
	,IssuedUtc
	,ValidFromUtc
	,ValidToUtc
	,LastPolling
	)
VALUES
	(
     @STATEID          
	,@IssuerId
    ,@AudienceId   
    ,@UserId         
    ,@StateValue       
	,@StateType
	,@StateDecision
	,@StateConsume
	,@IssuedUtc
	,@ValidFromUtc
	,@ValidToUtc
	,@LastPolling
	);

SELECT * FROM [svc].[uvw_State] WHERE [svc].[uvw_State].Id = @STATEID

END
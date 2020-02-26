
CREATE PROCEDURE [svc].[usp_Refresh_Insert]
     @IssuerId				UNIQUEIDENTIFIER
    ,@AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@RefreshValue			NVARCHAR (512) 
    ,@RefreshType			NVARCHAR (64)
    ,@IssuedUtc				DATETIME2 (7) 
    ,@ValidFromUtc			DATETIME2 (7)
    ,@ValidToUtc			DATETIME2 (7) 

AS
BEGIN

DECLARE @REFRESHID UNIQUEIDENTIFIER = NEWID()

INSERT INTO [dbo].[tbl_Refreshes]
	(
     Id         
	,IssuerId
    ,AudienceId    
    ,UserId           
    ,RefreshValue   
	,RefreshType
	,IssuedUtc
	,ValidFromUtc
	,ValidToUtc
	)
VALUES
	(
     @REFRESHID          
	,@IssuerId
    ,@AudienceId   
    ,@UserId         
    ,@RefreshValue       
	,@RefreshValue
	,@IssuedUtc
	,@ValidFromUtc
	,@ValidToUtc
	);

SELECT * FROM [dbo].[tbl_Refreshes] WHERE [dbo].[tbl_Refreshes].Id = @REFRESHID

END
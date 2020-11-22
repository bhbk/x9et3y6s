﻿
CREATE PROCEDURE [svc].[usp_Activity_Insert]
     @AudienceId			UNIQUEIDENTIFIER
    ,@UserId				UNIQUEIDENTIFIER
    ,@ActivityType          NVARCHAR (64) 
    ,@TableName				NVARCHAR (256)
    ,@KeyValues				NVARCHAR (MAX) 
    ,@OriginalValues		NVARCHAR (MAX) 
    ,@CurrentValues			NVARCHAR (MAX) 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @ACTIVITYID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_Activity]
	        (
             Id           
            ,AudienceId    
            ,UserId    
            ,ActivityType           
            ,TableName
            ,KeyValues   
            ,OriginalValues     
            ,CurrentValues        
            ,IsDeletable        
            ,CreatedUtc    
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
            ,@IsDeletable        
            ,@CREATEDUTC        
	        );

        SELECT * FROM [svc].[uvw_Activity] WHERE [svc].[uvw_Activity].Id = @ACTIVITYID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
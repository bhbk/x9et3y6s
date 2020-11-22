
CREATE   PROCEDURE [svc].[usp_AudienceRole_Insert]
     @AudienceId			UNIQUEIDENTIFIER 
    ,@RoleId				UNIQUEIDENTIFIER
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

        INSERT INTO [dbo].[tbl_AudienceRole]
	        (
             AudienceId         
	        ,RoleId
            ,IsDeletable        
            ,CreatedUtc           
	        )
        VALUES
	        (
             @AudienceId          
	        ,@RoleId
            ,@IsDeletable        
            ,@CREATEDUTC         
	        );

        SELECT * FROM [svc].[uvw_AudienceRole] 
			WHERE [svc].[uvw_AudienceRole].AudienceId = @AudienceID AND [svc].[uvw_AudienceRole].RoleId = @RoleID 

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
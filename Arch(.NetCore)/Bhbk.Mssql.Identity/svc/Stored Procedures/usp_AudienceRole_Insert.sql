
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

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_AudienceRole] 
			WHERE AudienceId = @AudienceID AND RoleId = @RoleID 

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
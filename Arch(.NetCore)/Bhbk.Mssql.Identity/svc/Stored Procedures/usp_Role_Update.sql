
CREATE PROCEDURE [svc].[usp_Role_Update]
     @Id					UNIQUEIDENTIFIER 
	,@AudienceId			UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@IsEnabled				BIT 
    ,@IsDeletable	    	BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Role]
        SET
             Id						= @Id
	        ,AudienceId				= @AudienceId
            ,ActorId				= @ActorId
	        ,Name					= @Name
	        ,Description			= @Description
	        ,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_Role] WHERE [svc].[uvw_Role].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
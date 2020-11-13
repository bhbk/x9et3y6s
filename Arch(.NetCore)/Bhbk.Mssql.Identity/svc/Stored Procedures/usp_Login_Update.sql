
CREATE PROCEDURE [svc].[usp_Login_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (MAX) 
    ,@Description			NVARCHAR (MAX)
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Login]
        SET
             Id						= @Id
            ,ActorId				= @ActorId
	        ,Name					= @Name
	        ,Description			= @Description
	        ,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_Login] WHERE [svc].[uvw_Login].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
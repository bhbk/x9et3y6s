
CREATE PROCEDURE [svc].[usp_Issuer_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@IssuerKey				NVARCHAR (1024) 
    ,@IsEnabled				BIT 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Issuer]
        SET
             Id						= @Id
	        ,Name					= @Name
	        ,Description			= @Description
	        ,IssuerKey				= @IssuerKey
	        ,IsEnabled				= @IsEnabled
            ,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_Issuer] WHERE [svc].[uvw_Issuer].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
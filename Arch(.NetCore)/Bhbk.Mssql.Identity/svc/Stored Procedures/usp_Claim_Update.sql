
CREATE PROCEDURE [svc].[usp_Claim_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (128) 
    ,@Type					NVARCHAR (128)
    ,@Value					NVARCHAR (256) 
    ,@ValueType             NVARCHAR (64) 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Claim]
        SET
             Id						= @Id
	        ,IssuerId				= @IssuerId
	        ,Subject				= @Subject
	        ,Type					= @Type
	        ,Value					= @Value
	        ,ValueType				= @ValueType
            ,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_Claim] WHERE [svc].[uvw_Claim].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
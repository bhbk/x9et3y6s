
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

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_Claim] 
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE PROCEDURE [svc].[usp_Claim_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@ActorId				UNIQUEIDENTIFIER
    ,@Subject               NVARCHAR (MAX) 
    ,@Type					NVARCHAR (MAX)
    ,@Value					NVARCHAR (MAX) 
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
            ,ActorId				= @ActorId
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
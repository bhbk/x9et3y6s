
CREATE PROCEDURE [svc].[usp_Audience_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IssuerId				UNIQUEIDENTIFIER
    ,@Name					NVARCHAR (128) 
    ,@Description			NVARCHAR (256)
    ,@IsLockedOut		    BIT     
    ,@IsDeletable			BIT
    ,@AccessFailedCount		INT  
    ,@AccessSuccessCount	INT  
    ,@LockoutEndUtc			DATETIMEOFFSET (7)
    ,@LastLoginSuccessUtc	DATETIMEOFFSET (7)
    ,@LastLoginFailureUtc	DATETIMEOFFSET (7)

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_Audience]
        SET
             Id						= @Id
            ,IssuerId				= @IssuerId
	        ,Name					= @Name
	        ,Description			= @Description
            ,IsLockedOut			= @IsLockedOut
            ,IsDeletable			= @IsDeletable
            ,AccessFailedCount		= @AccessFailedCount
            ,AccessSuccessCount		= @AccessSuccessCount
            ,LockoutEndUtc			= @LockoutEndUtc
            ,LastLoginSuccessUtc	= @LastLoginSuccessUtc
            ,LastLoginFailureUtc	= @LastLoginFailureUtc
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_Audience] WHERE [svc].[uvw_Audience].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
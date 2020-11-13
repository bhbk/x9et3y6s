

CREATE PROCEDURE [svc].[usp_TextQueue_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@ActorId				UNIQUEIDENTIFIER
    ,@FromId				UNIQUEIDENTIFIER
    ,@FromPhoneNumber       NVARCHAR (MAX) 
    ,@ToId					UNIQUEIDENTIFIER
    ,@ToPhoneNumber         NVARCHAR (MAX) 
    ,@Body	                NVARCHAR (MAX) 
    ,@SendAtUtc             DATETIMEOFFSET (7) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        UPDATE [dbo].[tbl_TextQueue]
        SET
             Id						= @Id
            ,ActorId				= @ActorId
            ,FromId					= @FromId
	        ,FromPhoneNumber		= @FromPhoneNumber
            ,ToId					= @ToId
            ,ToPhoneNumber			= @ToPhoneNumber
            ,Body					= @Body
            ,SendAtUtc				= @SendAtUtc
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_TextQueue] WHERE [svc].[uvw_TextQueue].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
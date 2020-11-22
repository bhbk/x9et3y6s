﻿

CREATE PROCEDURE [svc].[usp_EmailQueue_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@FromId				UNIQUEIDENTIFIER
    ,@FromEmail             NVARCHAR (MAX) 
    ,@FromDisplay           NVARCHAR (MAX) 
    ,@ToId					UNIQUEIDENTIFIER
    ,@ToEmail               NVARCHAR (MAX) 
    ,@ToDisplay             NVARCHAR (MAX) 
    ,@Subject               NVARCHAR (MAX) 
    ,@Body      			NVARCHAR (MAX)
    ,@SendAtUtc             DATETIMEOFFSET (7) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_EmailQueue]
        SET
             Id						= @Id
            ,FromId					= @FromId
	        ,FromEmail				= @FromEmail
	        ,FromDisplay			= @FromDisplay
            ,ToId					= @ToId
            ,ToEmail				= @ToEmail
            ,ToDisplay				= @ToDisplay
            ,Subject				= @Subject
            ,Body       			= @Body
            ,SendAtUtc				= @SendAtUtc
        WHERE Id = @Id

        SELECT * FROM [svc].[uvw_EmailQueue] WHERE [svc].[uvw_EmailQueue].Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
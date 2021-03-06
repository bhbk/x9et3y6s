﻿

CREATE PROCEDURE [svc].[usp_TextQueue_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IsCancelled			BIT
    ,@SendAtUtc             DATETIMEOFFSET (7) 
    ,@DeliveredUtc			DATETIMEOFFSET (7) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        UPDATE [dbo].[tbl_TextQueue]
        SET
             Id						= @Id
			,IsCancelled			= @IsCancelled
            ,SendAtUtc				= @SendAtUtc
			,DeliveredUtc			= @DeliveredUtc
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_TextQueue]
            WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

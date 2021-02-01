﻿
CREATE PROCEDURE [svc].[usp_MOTD_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_MOTD]
            WHERE Id = @Id

        DELETE [dbo].[tbl_MOTD]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
﻿
CREATE PROCEDURE [svc].[usp_Url_Delete]
    @Id uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [dbo].[tbl_Url]
            WHERE Id = @Id

        DELETE [dbo].[tbl_Url]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
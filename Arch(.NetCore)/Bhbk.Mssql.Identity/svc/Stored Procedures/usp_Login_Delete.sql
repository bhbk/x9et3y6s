﻿
CREATE PROCEDURE [svc].[usp_Login_Delete]
    @ID uniqueidentifier

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        SELECT * FROM [svc].[uvw_Login] WHERE [svc].[uvw_Login].Id = @ID

        DELETE [dbo].[tbl_Login]
        WHERE Id = @ID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
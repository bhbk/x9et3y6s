
CREATE PROCEDURE [svc].[usp_Refresh_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Refresh] WHERE [svc].[uvw_Refresh].Id = @ID

DELETE [dbo].[tbl_Refresh]
WHERE Id = @ID

END
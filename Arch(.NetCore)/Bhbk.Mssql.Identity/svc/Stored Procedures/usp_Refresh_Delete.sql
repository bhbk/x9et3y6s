
CREATE PROCEDURE [svc].[usp_Refresh_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Refreshes] WHERE [svc].[uvw_Refreshes].Id = @ID

DELETE [dbo].[tbl_Refreshes]
WHERE Id = @ID

END
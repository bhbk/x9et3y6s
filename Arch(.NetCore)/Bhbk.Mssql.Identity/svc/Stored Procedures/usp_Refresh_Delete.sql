

CREATE PROCEDURE [svc].[usp_Refresh_Delete]
    @RefreshID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Refreshes] WHERE [svc].[uvw_Refreshes].Id = @RefreshID

DELETE [dbo].[tbl_Refreshes]
WHERE Id = @RefreshID

END
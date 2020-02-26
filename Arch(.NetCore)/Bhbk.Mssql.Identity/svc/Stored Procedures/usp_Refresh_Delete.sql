

CREATE PROCEDURE [svc].[usp_Refresh_Delete]
    @RefreshID uniqueidentifier

AS
BEGIN

DELETE [dbo].[tbl_Refreshes]
WHERE Id = @RefreshID

END

CREATE PROCEDURE [svc].[usp_Activity_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Activity] WHERE [svc].[uvw_Activity].Id = @ID

DELETE [dbo].[tbl_Activity]
WHERE Id = @ID

END
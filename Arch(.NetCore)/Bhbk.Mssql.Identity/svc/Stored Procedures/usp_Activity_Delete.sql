
CREATE PROCEDURE [svc].[usp_Activity_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Activities] WHERE [svc].[uvw_Activities].Id = @ID

DELETE [dbo].[tbl_Activities]
WHERE Id = @ID

END
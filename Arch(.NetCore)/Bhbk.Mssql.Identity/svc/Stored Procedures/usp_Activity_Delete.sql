


CREATE PROCEDURE [svc].[usp_Activity_Delete]
    @ActivityID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_Activities] WHERE [svc].[uvw_Activities].Id = @ActivityID

DELETE [dbo].[tbl_Activities]
WHERE Id = @ActivityID

END
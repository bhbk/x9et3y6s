


CREATE PROCEDURE [svc].[usp_Activity_Delete]
    @ActivityID uniqueidentifier

AS
BEGIN

DELETE [dbo].[tbl_Activities]
WHERE Id = @ActivityID

END

CREATE PROCEDURE [svc].[usp_State_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_States] WHERE [svc].[uvw_States].Id = @ID

DELETE [dbo].[tbl_States]
WHERE Id = @ID

END
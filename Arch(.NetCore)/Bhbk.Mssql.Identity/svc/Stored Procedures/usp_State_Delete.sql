
CREATE PROCEDURE [svc].[usp_State_Delete]
    @ID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_State] WHERE [svc].[uvw_State].Id = @ID

DELETE [dbo].[tbl_State]
WHERE Id = @ID

END
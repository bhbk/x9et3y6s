
CREATE PROCEDURE [svc].[usp_State_Delete]
    @StateID uniqueidentifier

AS
BEGIN

SELECT * FROM [svc].[uvw_States] WHERE [svc].[uvw_States].Id = @StateID

DELETE [dbo].[tbl_States]
WHERE Id = @StateID

END

CREATE VIEW [svc].[uvw_Login]
AS
SELECT        Id, ActorId, Name, Description, LoginKey, IsEnabled, IsDeletable, CreatedUtc, LastUpdatedUtc
FROM            [dbo].[tbl_Login]
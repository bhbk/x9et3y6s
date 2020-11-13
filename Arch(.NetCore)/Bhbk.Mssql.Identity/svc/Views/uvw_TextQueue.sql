

CREATE VIEW [svc].[uvw_TextQueue]
AS
SELECT        Id, ActorId, FromId, FromPhoneNumber, ToId, ToPhoneNumber, Body, CreatedUtc, SendAtUtc
FROM            [dbo].[tbl_TextQueue]
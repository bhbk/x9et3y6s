
CREATE VIEW [svc].[uvw_QueueTexts]
AS
SELECT        Id, ActorId, FromId, FromPhoneNumber, ToId, ToPhoneNumber, Body, Created, SendAt
FROM            dbo.tbl_QueueTexts
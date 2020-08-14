
CREATE VIEW [svc].[uvw_QueueText]
AS
SELECT        Id, ActorId, FromId, FromPhoneNumber, ToId, ToPhoneNumber, Body, Created, SendAt
FROM            dbo.tbl_QueueText

CREATE VIEW [svc].[uvw_QueueEmail]
AS
SELECT        Id, ActorId, FromId, FromEmail, FromDisplay, ToId, ToEmail, ToDisplay, Subject, HtmlContent, PlaintextContent, Created, SendAt
FROM            dbo.tbl_QueueEmail